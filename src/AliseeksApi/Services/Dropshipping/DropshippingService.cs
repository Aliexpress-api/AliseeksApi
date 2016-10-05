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

        public async Task<DropshipItemModel> Update(DropshipItemModel model)
        {
            await dbItems.Save(model);

            return model;
        }

        public async Task AddProduct(SingleItemRequest item)
        {
            var detail = await search.ItemSearch(new ItemDetail()
            {
                Source = item.Source,
                ItemID = item.ID,
                Name = item.Title
            });

            var shopifyModel = new ShopifyProductModel()
            {
                BodyHtml = detail.Description,
                Title = detail.Name,
                Vendor = "Me",
                ProductType = detail.Source,

                Variants = new List<object>()
                {
                    new ShopifyVariant().Price((detail.Price + detail.ShippingPrice) * (decimal)1.05)
                    .InventoryQuantity(detail.Quantity - 100 > 0 ? detail.Quantity - 100 : 0)
                    .InventoryPolicy(InventoryPolicy.Deny)
                    .InventoryManagement(InventoryManagement.Shopify)
                    .RequireShipping(true)
                    .Taxable(false)
                    .Build()
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

            shopifyModel.Images = images.ToArray();

            var product = await shopify.AddProduct(shopifyModel);

            await dbItems.Save(new DropshipItemModel()
            {
                Listing = "Shopify",
                ListingID = product.ID,
                Source = detail.Source,
                ItemID = detail.ItemID,
                Username = item.Username,
                Rules = DropshipListingRules.Default
            });
        }
    }
}
