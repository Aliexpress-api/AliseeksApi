using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Services.Dropshipping.Shopify
{
    public class ShopifyEndpoints
    {
        public const string Products = "products.json";

        public static string BaseEndpoint(string apikey, string password, string store, string endpoint)
        {
            return $"https://{store}.com/admin/{endpoint}";
        }
    }
}
