using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Services.Dropshipping.Shopify
{
    public class ShopifyEndpoints
    {
        public const string Products = "products.json";

        public static string BaseEndpoint(string store, string endpoint, string apikey = "", string password = "")
        {
            return $"https://{store}/admin/{endpoint}";
        }

        public static string OAuthEndpoint(string hostname)
        {
            return $"https://{hostname}/admin/oauth/access_token";
        }
    }
}
