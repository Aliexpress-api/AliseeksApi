using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AliseeksApi.Storage.Postgres.ORM;
using AliseeksApi.Models.Shopify;

namespace AliseeksApi.Models.Dropshipping.Orders
{
    public class DropshipOrder
    {
        public ShopifyOrder Order { get; set; }
        public DropshipItemModel[] Items { get; set; }
    }
}
