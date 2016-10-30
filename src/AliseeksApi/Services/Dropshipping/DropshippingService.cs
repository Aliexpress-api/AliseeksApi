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
using AliseeksApi.Storage.Postgres.OAuth;
using AliseeksApi.Models.OAuth;
using AliseeksApi.Services.OAuth;

namespace AliseeksApi.Services.Dropshipping
{
    public class DropshippingService
    {
        private readonly ShopifyService shopify;
        private readonly ISearchService search;
        private readonly DropshipItemsPostgres dbItems;
        private readonly DropshipAccountsPostgres dbAccounts;
        private readonly IApplicationCache cache;
        private readonly OAuthService oauthdb;
        private readonly OAuthPostgres dbOAuth;

        public DropshippingService(ShopifyService shopify, ISearchService search, OAuthPostgres dbOauth,
            OAuthService oauthdb, DropshipItemsPostgres dbItems, DropshipAccountsPostgres dbAccounts, IApplicationCache cache)
        {
            this.shopify = shopify;
            this.search = search;
            this.dbItems = dbItems;
            this.dbAccounts = dbAccounts;
            this.cache = cache;
            this.dbOAuth = dbOauth;
            this.oauthdb = oauthdb; 
        }

        public async Task<DropshipAccount> SetupAccount(DropshipAccountConfiguration account, string username)
        {
            var dropshipAccount = new DropshipAccount()
            {
                Username = username,
                Status = AccountStatus.Existing,
                Subscription = account.Subscription
            };

            await dbAccounts.CreateAccount(dropshipAccount);

            return dropshipAccount;
        }

        public async Task<DropshipOverview> GetOverview(string username)
        {
            var accountTask = GetAccount(username);
            var productsTask = dbItems.GetMultipleByUsername(username);
            var integrationsTask = dbOAuth.GetMultipleByUsername(username);
            //var ordersTask = GetOrders(username);

            var account = await accountTask;
            var items = await productsTask;
            var integrations = await integrationsTask;

            var dropshipOverview = new DropshipOverview()
            {
                Account = account,
                IntegrationCount = integrations.Length,
                ProductCount = items.Length
            };

            return dropshipOverview;
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

        public async Task<DropshipItem[]> GetProducts(string username, int offset = 0, int limit = 50)
        {
            var dropshipItems = new List<DropshipItem>();

            var oauth = await oauthdb.RetrieveOAuth<OAuthShopifyModel>(username);
            if (oauth == null)
                return new DropshipItem[0];

            var items = await dbItems.GetMultipleByUsername(username);

            if (items.Length == 0)
                return new DropshipItem[0];

            var ids = new List<string>();
            foreach(var item in items)
            {
                ids.Add(item.ListingID);
            }

            var shopifyItems = await shopify.GetProductsByID(username, ids.ToArray(), oauth);

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

        public async Task<DropshipItem[]> GetProducts(string username, DropshipItemModel[] items)
        {
            var dropshipItems = new List<DropshipItem>();

            var oauth = await oauthdb.RetrieveOAuth<OAuthShopifyModel>(username);
            if (oauth == null)
                return new DropshipItem[0];

            var ids = new List<string>();
            foreach (var item in items)
            {
                ids.Add(item.ListingID);
            }

            var shopifyItems = await shopify.GetProductsByID(username, ids.ToArray(), oauth);

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
            var shopifyTask = shopify.GetOrders(username);
            var dropshipTask = dbItems.GetMultipleByUsername(username);

            if (shopifyTask.IsFaulted || dropshipTask.IsFaulted)
                return new DropshipOrder[0];

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

        public async Task AddProduct(string username, SingleItemRequest item)
        {
            var detail = await search.ItemSearch(item);

            //Get integration access tokens
            var oauth = await oauthdb.RetrieveOAuth<OAuthShopifyModel>(username);
            if (oauth == null)
                return;

            //Create the dropship item and model
            var dropshipItem = new DropshipItem()
            {
                Dropshipping = new DropshipItemModel()
                {
                    Source = item,
                    Username = item.Username,
                    Rules = DropshipListingRules.Default,
                    OAuthID = oauth.ID
                },
                Product = new ShopifyProductModel()
                {
                    BodyHtml = detail.Description,
                    Title = detail.Name.Replace("/", "-"), //Fix slash in name issue

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
            
            //Add images from shopify
            var images = new List<ShopifyImageType>();
            foreach(var image in detail.ImageUrls)
            {
                images.Add(new ShopifyImageType()
                {
                    Src = image
                });
            }

            //Set the first image to the main dropship model image
            if (images.Count > 0)
                dropshipItem.Dropshipping.Image = images[0].Src;

            dropshipItem.Product.Images = images.ToArray();

            //Apply dropshipping rules
            dropshipItem.Dropshipping.Rules.ApplyRules(detail, dropshipItem.Product);

            //Add product
            var product = await shopify.AddProduct(username, dropshipItem.Product, oauth);

            dropshipItem.Dropshipping.ListingID = product.ID;

            await dbItems.Save(dropshipItem.Dropshipping);
        }

        //Obsolete version of this method
  /*      public async Task AddProduct(string username, DropshipItemModel model)
        {
            if (model.Source.Link.EmptyOrNull())
                return;

            switch(model.Source.Source)
            {
                case "Aliexpress":
                    await AddProduct(username, model.Source);
                    break;

                default:
                    throw new Exception("Invalid DropshipItemModel Source");
            }
        }
        */

        //Obsolete method, this is now done in the DropshippingController

            /*
        public async Task UpdateProduct(string username, DropshipItem item)
        {
            await shopify.UpdateProduct(username, item.Product);
        } */

        public async Task SyncProduct(string username, DropshipItem item)
        {
            var sourceItem = await search.ItemSearch(item.Dropshipping.Source);

            var rules = item.Dropshipping.Rules ?? DropshipListingRules.Default;

            var creds = await oauthdb.RetrieveOAuth<OAuthShopifyModel>(username);

            if(rules.ApplyRules(sourceItem, item.Product) && creds != null)
                await shopify.UpdateProduct(username, item.Product, creds);
        }

        public async Task<DropshipIntegration[]> GetIntegrations(string username)
        {
            var items = await oauthdb.RetrieveMultiple(username);
            var integrations = new List<DropshipIntegration>();

            var shopifyIntegration = items.FirstOrDefault(x => x.Service == "Shopify");

            if(shopifyIntegration != null)
            {
                var shopifyTyped = JsonConvert.DeserializeObject<OAuthShopifyModel>(JsonConvert.SerializeObject(shopifyIntegration));

                integrations.Add(new DropshipIntegration()
                {
                    ID = shopifyTyped.ID,
                    Service = "Shopify",
                    AccountInfo = shopifyTyped.Shop
                });
            }

            return integrations.ToArray();
        }
    }
}
