using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Services.Dropshipping;
using AliseeksApi.Storage.Postgres.Dropshipping;
using AliseeksApi.Models.Dropshipping;
using SharpRaven.Core;
using Hangfire;

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

            foreach(var item in items)
            {
                if(item.Account.Subscription != "Expired")
                {
                    await dropship.SyncProduct(item.Item);
                }
            }

            skip += itemsPerJob;
            if (skip < count)
                BackgroundJob.Enqueue<DropshippingJobs>(x => x.RunUpdateItems(skip));
        }
    }
}
