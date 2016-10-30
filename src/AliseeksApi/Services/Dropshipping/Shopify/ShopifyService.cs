using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpRaven.Core;
using AliseeksApi.Models.Shopify;
using AliseeksApi.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using System.Net.Security;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using AliseeksApi.Models.Dropshipping.Orders;
using AliseeksApi.Utility;
using AliseeksApi.Storage.Postgres.OAuth;
using AliseeksApi.Models.OAuth;
using AliseeksApi.Storage.Cache;
using AliseeksApi.Services.OAuth;
using Microsoft.AspNetCore.Http;
using AliseeksApi.Models.Dropshipping;

namespace AliseeksApi.Services.Dropshipping.Shopify
{
    public class ShopifyService
    {
        private readonly IHttpService http;
        private readonly IRavenClient raven;
        private readonly ShopifyOptions config;
        private readonly OAuthPostgres oauthDb;
        private readonly ShopifyOAuth oauth;
        private readonly IApplicationCache cache;
        private readonly OAuthService oauthRetriever;

        //Populated by SetupScope function, contains shopify credentials
        private OAuthShopifyModel creds { get; set; }

        JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        //Setup this class as a Scoped dependency injection and call SetupScope
        public ShopifyService(IHttpService http, IApplicationCache cache,
            ShopifyOAuth oauth, OAuthPostgres oauthDb, OAuthService oauthRetriever, IRavenClient raven, IOptions<ShopifyOptions> config)
        {
            this.http = http;
            this.raven = raven;
            this.config = config.Value;
            this.oauthDb = oauthDb;
            this.cache = cache;
            this.oauthRetriever = oauthRetriever;
        }

        public async Task<bool> AddShopifyIntegration(DropshipAccount account, ShopifyOAuthResponse oauth, ShopifyOAuth verify)
        {
            var username = account.Username;

            var endpoint = ShopifyEndpoints.OAuthEndpoint(oauth.Shop);

            var requestType = new
            {
                client_id = config.ClientID,
                client_secret = config.ClientSecret,
                code = oauth.Code
            };

            var requestContent = JsonConvert.SerializeObject(requestType, jsonSettings);
            var content = new JsonContent(requestContent);

            var response = await http.Post(endpoint, content);

            string message = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = JsonConvert.DeserializeObject<ShopifyOAuthAccessResponse>(message, jsonSettings);
                verify.VerifyScope(tokenResponse.Scope);

                await oauthDb.CreateOAuth(new OAuthAccountModel()
                {
                    AccessToken = tokenResponse.AccessToken,
                    Username = username,
                    Service = "Shopify",
                    Extra = new Dictionary<string, string>()
                    {
                        { "Shop", oauth.Shop }
                    },
                    AccountID = account.ID
                });

                return true;
            }
            else
                return false;            
        }

        public async Task<ShopifyProductModel> AddProduct(string username, ShopifyProductModel product, OAuthShopifyModel creds)
        {
            string endpoint = ShopifyEndpoints.BaseEndpoint(creds.Shop, ShopifyEndpoints.Products);

            var requestType = new
            {
                Product = product
            };

            var requestContent = JsonConvert.SerializeObject(requestType, jsonSettings);
            var content = new StringContent(requestContent);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await http.Post(endpoint, content, (client) =>
               {
                   addAuthenticatoin(client, creds.AccessToken);
               });

            string message = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var productResponse = JObject.Parse(message).SelectToken("product").ToString();
                var ret = JsonConvert.DeserializeObject<ShopifyProductModel>(productResponse, jsonSettings);
                return ret;
            }
            else
                return new ShopifyProductModel();
        }

        public async Task<ShopifyProductModel[]> GetProducts(string username)
        {
            var creds = await GetCredentials(username);
            if (creds == null)
                return new ShopifyProductModel[0]; //Not integrated into shopify

            string endpoint = ShopifyEndpoints.BaseEndpoint(creds.Shop, ShopifyEndpoints.Products);

            var response = await http.Get(endpoint, (client) =>
            {
                addAuthenticatoin(client, creds.AccessToken);
            });

            string message = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var productResponse = JObject.Parse(message).SelectToken("product").ToString();
                var ret = JsonConvert.DeserializeObject<ShopifyProductModel[]>(productResponse, jsonSettings);
                return ret;
            }

            return new ShopifyProductModel[0];       
        }

        public async Task<ShopifyProductModel[]> GetProductsByID(string username, string[] ids, OAuthShopifyModel creds)
        {
            string endpoint = ShopifyEndpoints.BaseEndpoint(creds.Shop, ShopifyEndpoints.Products) + $"?ids={String.Join(",", ids)}";

            var response = await http.Get(endpoint, (client) =>
            {
                addAuthenticatoin(client, creds.AccessToken);
            });

            string message = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var productResponse = JObject.Parse(message).SelectToken("products").ToString();
                var ret = JsonConvert.DeserializeObject<ShopifyProductModel[]>(productResponse, jsonSettings);

                foreach(var item in ret)
                {
                    item.Link = $"https://{creds.Shop}/admin/products/{item.ID}";
                }

                return ret;
            }

            return new ShopifyProductModel[0];
        }

        public async Task<ShopifyProductModel> UpdateProduct(string username, ShopifyProductModel product, OAuthShopifyModel creds)
        {
            string endpoint = ShopifyEndpoints.BaseEndpoint(creds.Shop, "products") + $"/{product.ID}.json";

            var requestType = new
            {
                Product = product
            };

            var requestContent = JsonConvert.SerializeObject(requestType, jsonSettings);
            var content = new StringContent(requestContent);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await http.Put(endpoint, content, (client) =>
            {
                addAuthenticatoin(client, creds.AccessToken);
            });

            string message = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var productResponse = JObject.Parse(message).SelectToken("product").ToString();
                var ret = JsonConvert.DeserializeObject<ShopifyProductModel>(productResponse, jsonSettings);
                return ret;
            }
            else
                return null;
        }

        public async Task<ShopifyOrder[]> GetOrders(string username)
        {
            var creds = await GetCredentials(username);
            if (creds == null)
                return new ShopifyOrder[0]; //Not integrated into shopify

            string endpoint = ShopifyEndpoints.BaseEndpoint(creds.Shop, "orders.json") + "?fulfillment_status=unshipped,partial";

            var response = await http.Get(endpoint, (client) =>
            {
                addAuthenticatoin(client, creds.AccessToken);
            });

            string message = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var ordersResponse = JObject.Parse(message).SelectToken("orders").ToString();
                var ret = JsonConvert.DeserializeObject<ShopifyOrder[]>(ordersResponse, jsonSettings);
                return ret;
            }
            else
                return new ShopifyOrder[0];
        }

        async Task<OAuthShopifyModel> GetCredentials(string username)
        {
            var key = $"OAuth:Shopify:{username}";

            if (await cache.Exists(key))
                return JsonConvert.DeserializeObject<OAuthShopifyModel>(await cache.GetString(key));

            var integrations = await oauthDb.GetMultipleByUsername(username);

            var shopifyIntegration = integrations.FirstOrDefault(x => x.Service == "Shopify");

            if (shopifyIntegration != null)
            {
                await cache.StoreString(key, JsonConvert.SerializeObject(shopifyIntegration));
                return JsonConvert.DeserializeObject<OAuthShopifyModel>(JsonConvert.SerializeObject(shopifyIntegration));
            }
            else
            {
                return null;
            }
        }

        void addAuthenticatoin(HttpClient client, string accesstoken)
        {
            client.DefaultRequestHeaders.Add("X-Shopify-Access-Token", accesstoken);
        }
    }
}
