using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Services.Dropshipping;
using AliseeksApi.Storage.Postgres.Dropshipping;
using AliseeksApi.Models.Dropshipping;
using SharpRaven.Core;
using Hangfire;
using AliseeksApi.Models.Shopify;

namespace AliseeksApi.Jobs
{
    public class DropshippingJobs
    {
        private readonly DropshippingService dropship;
        private readonly DropshipItemsPostgres dbItems;
        private readonly DropshipAccountsPostgres dbAccounts;
        private readonly IRavenClient raven;

        private const int itemsPerJob = 1000;

        public DropshippingJobs(DropshipAccountsPostgres dbAccounts, DropshipItemsPostgres dbItems, DropshippingService dropship, IRavenClient raven)
        {
            this.dbAccounts = dbAccounts;
            this.dbItems = dbItems;
            this.raven = raven;
            this.dropship = dropship;
        }

        public async Task RunUpdateItems(int skip)
        {
            var count = await dbItems.CountItems();
            if(skip > count) { return; }

            var items = await dbItems.GetMultipleWithAccount(itemsPerJob, skip);
            var products = new List<DropshipItem>();         

            foreach(var item in items)
            {
                if(item.Account.Subscription != "Expired")
                {
                    //Get Shopify product information from product list
                    var product = products.FirstOrDefault(x => x.Product.ID == item.Item.ListingID);
                    
                    //Retrieve products for a user from shopify once
                    if (product == null && products.Count(x => x.Dropshipping.Username == item.Account.Username) == 0)
                    {
                        var userItems = items.Where(x => x.Account.Username == item.Account.Username).Select(x => x.Item).ToArray();
                        var shopifyProducts = await dropship.GetProducts(item.Account.Username, userItems);
                        if (shopifyProducts != null)
                            products.AddRange(shopifyProducts);

                        product = products.FirstOrDefault(x => x.Product.ID == item.Item.ListingID);
                    }

                    //If its still null then either it was delete from Shopify product list or we are having communication issues
                    if (product == null)
                        continue;

                    
                    await dropship.SyncProduct(item.Account.Username, product);
                }
            }

            skip += itemsPerJob;
            if (skip < count)
                BackgroundJob.Enqueue<DropshippingJobs>(x => x.RunUpdateItems(skip));
        }
    }
}
