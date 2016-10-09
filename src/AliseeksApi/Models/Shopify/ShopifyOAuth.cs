using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace AliseeksApi.Models.Shopify
{
    public class ShopifyOAuthRequest
    {
        public string Shop { get; set; }
        public string APIKey { get; set; }
        public string Scopes { get; set; }
        public string RedirectUri { get; set; }
        public string Nounce { get; set; }
        public string Uri { get; set; }
    }

    public class ShopifyOAuthResponse
    {
        public string Code { get; set; }
        public string Hmac { get; set; }
        public string Shop { get; set; }
        public string State { get; set; }
        public string Query { get; set; }
    }

    public class ShopifyOAuthAccessResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        public string Scope { get; set; }
    }
}
