using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Storage.Postgres.Dropshipping;
using AliseeksApi.Models.Shopify;
using AliseeksApi.Models.Dropshipping;
using AliseeksApi.Services.Dropshipping.Shopify;
using AliseeksApi.Services.Search;
using AliseeksApi.Models.Search;
using AliseeksApi.Models;
using AliseeksApi.Storage.Cache;
using AliseeksApi.Utility;
using Newtonsoft.Json;
using AliseeksApi.Services.Aliexpress;
using Microsoft.AspNetCore.Routing;
using AliseeksApi.Exceptions;
using AliseeksApi.Utility.Extensions;
using AliseeksApi.Models.Dropshipping.Orders;

namespace AliseeksApi.Services.Dropshipping
{
    public class DropshippingService
    {
        private readonly ShopifyService shopify;
        private readonly ISearchService search;
        private readonly DropshipItemsPostgres dbItems;
        private readonly DropshipAccountsPostgres dbAccounts;
        private readonly IApplicationCache cache;

        public DropshippingService(ShopifyService shopify, ISearchService search, DropshipItemsPostgres dbItems, DropshipAccountsPostgres dbAccounts, IApplicationCache cache)
        {
            this.shopify = shopify;
            this.search = search;
            this.dbItems = dbItems;
            this.dbAccounts = dbAccounts;
            this.cache = cache;
        }

        public async Task<DropshipAccount> SetupAccount(DropshipAccountConfiguration account, string username)
        {
            var dropshipAccount = new DropshipAccount()
            {
                Username = username,
                Status = AccountStatus.Existing,
                Subscription = account.Subscription
            };

            await dbAccounts.Save(dropshipAccount);

            return dropshipAccount;
        }

        public async Task<DropshipAccount> GetAccount(string username)
        {
            //Check cache for account info
            var accountInfo = new DropshipAccount()
            {
                Username = username
            };

            if (await cache.Exists(RedisKeyConvert.Serialize(accountInfo)))
                return JsonConvert.DeserializeObject<DropshipAccount>(await cache.GetString(RedisKeyConvert.Serialize(accountInfo)));

            //Couldn't find it in the cache, look in db
            accountInfo = await dbAccounts.GetOneByUsername(username);

            if (accountInfo != null)
                return accountInfo;

            //Couldn't find it in db, must be a new account
            accountInfo = new DropshipAccount()
            {
                Username = username,
                Status = AccountStatus.New
            };

            return accountInfo;
        }

        public async Task<DropshipItem[]> GetProducts(string username)
        {
            var dropshipItems = new List<DropshipItem>();

            var items = await dbItems.GetMultipleByUsername(username);

            var ids = new List<string>();
            foreach(var item in items)
            {
                ids.Add(item.ListingID);
            }

            var shopifyItems = await shopify.GetProductsByID(ids.ToArray());

            foreach(var shopifyItem in shopifyItems)
            {
                var item = items.First(x => x.ListingID == shopifyItem.ID);
                dropshipItems.Add(new DropshipItem()
                {
                    Dropshipping = item,
                    Product = shopifyItem
                });
            }

            return dropshipItems.ToArray();
        }

        public async Task<DropshipItem[]> GetProducts(DropshipItemModel[] items)
        {
            var dropshipItems = new List<DropshipItem>();

            var ids = new List<string>();
            foreach (var item in items)
            {
                ids.Add(item.ListingID);
            }

            var shopifyItems = await shopify.GetProductsByID(ids.ToArray());

            foreach (var shopifyItem in shopifyItems)
            {
                var item = items.First(x => x.ListingID == shopifyItem.ID);
                dropshipItems.Add(new DropshipItem()
                {
                    Dropshipping = item,
                    Product = shopifyItem
                });
            }

            return dropshipItems.ToArray();
        }

        public async Task<DropshipOrder[]> GetOrders(string username)
        {
            var orders = new List<DropshipOrder>();

            //Get unfulfilled orders & dropship items from db
            var shopifyTask = shopify.GetOrders();
            var dropshipTask = dbItems.GetMultipleByUsername(username);

            var dropshipItems = await dropshipTask;
            var shopifyItems = await shopifyTask;

            foreach(var shopifyOrder in shopifyItems)
            {
                var dropshipItemsOnOrder = new List<DropshipItemModel>();
                shopifyOrder.LineItems.ForEach(x =>
                {
                    var dropshipItem = dropshipItems.FirstOrDefault(ds => ds.ListingID == x.ProductID);

                    if (dropshipItems != null)
                        dropshipItemsOnOrder.Add(dropshipItem);
                });

                if (dropshipItemsOnOrder.Count == 0)
                    continue;

                orders.Add(new DropshipOrder()
                {
                    Order = shopifyOrder,
                    Items = dropshipItemsOnOrder.ToArray()
                });
            }

            return orders.ToArray();
        }

        public async Task<DropshipItemModel> Update(DropshipItemModel model)
        {
            await dbItems.Save(model);

            return model;
        }

        public async Task AddProduct(SingleItemRequest item)
        {
            var detail = await search.ItemSearch(item);

            var dropshipItem = new DropshipItem()
            {
                Dropshipping = new DropshipItemModel()
                {
                    Listing = "Shopify",
                    Source = item,
                    Username = item.Username,
                    Rules = DropshipListingRules.Default
                },
                Product = new ShopifyProductModel()
                {
                    BodyHtml = detail.Description,
                    Title = detail.Name,
                    Vendor = "Me", //TODO: GET RID
                    ProductType = detail.Source,

                    Variants = new List<ShopifyVarant>()
                    {
                        new ShopifyVarant()
                        {
                            InventoryPolicy = InventoryPolicy.Deny,
                            InventoryManagement = InventoryManagement.Shopify,
                            RequiresShipping = true,
                            Taxable = true
                        }
                    }
                }
            };

            var images = new List<ShopifyImageType>();
            foreach(var image in detail.ImageUrls)
            {
                images.Add(new ShopifyImageType()
                {
                    Src = image
                });
            }

            dropshipItem.Product.Images = images.ToArray();

            dropshipItem.Dropshipping.Rules.ApplyRules(detail, dropshipItem.Product);

            var product = await shopify.AddProduct(dropshipItem.Product);

            dropshipItem.Dropshipping.ListingID = product.ID;

            await dbItems.Save(dropshipItem.Dropshipping);
        }

        public async Task AddProduct(DropshipItemModel model)
        {
            if (model.Source.Link.EmptyOrNull())
                return;

            switch(model.Source.Source)
            {
                case "Aliexpress":
                    await AddProduct(model.Source);
                    break;

                default:
                    throw new Exception("Invalid DropshipItemModel Source");
            }
        }

        public async Task UpdateProduct(DropshipItem item)
        {
            await shopify.UpdateProduct(item.Product);
        }

        public async Task SyncProduct(DropshipItem item)
        {
            var sourceItem = await search.ItemSearch(item.Dropshipping.Source);

            var rules = item.Dropshipping.Rules ?? DropshipListingRules.Default;

            if(rules.ApplyRules(sourceItem, item.Product))
                await UpdateProduct(item);
        }
    }
}
