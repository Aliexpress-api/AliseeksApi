using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.Shopify
{
    public class ShopifyAccountModel
    {
        public string Store { get; set; }
        public string AccessToken { get; set; }
        public string Scope { get; set; }
    }
}
