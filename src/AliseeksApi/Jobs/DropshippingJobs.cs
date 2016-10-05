using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Services.Dropshipping;
using AliseeksApi.Storage.Postgres.Dropshipping;
using AliseeksApi.Models.Dropshipping;
using SharpRaven.Core;

namespace AliseeksApi.Services.Dropshipping
{
    public class DropshippingJobs
    {
        private readonly DropshipItemsPostgres dbItems;
        private readonly DropshipAccountsPostgres dbAccounts;
        private readonly IRavenClient raven;

        public DropshippingJobs(DropshipAccountsPostgres dbAccounts, DropshipItemsPostgres dbItems, IRavenClient raven)
        {
            this.dbAccounts = dbAccounts;
            this.dbItems = dbItems;
            this.raven = raven;
        }
    }
}
