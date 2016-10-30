using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Shopify;

namespace AliseeksApi.Models.Dropshipping
{
    public class DropshipItem
    {
        public DropshipItemModel Dropshipping { get; set; }
        public ShopifyProductModel Product { get; set; }
    }
}
