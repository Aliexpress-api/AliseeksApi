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
using System.Net.Http;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;

namespace AliseeksApi.Services.Dropshipping.Shopify
{
    public class ShopifyService
    {
        private readonly IHttpService http;
        private readonly IRavenClient raven;
        private readonly ShopifyOptions config;

        JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        public ShopifyService(IHttpService http, IRavenClient raven, IOptions<ShopifyOptions> config)
        {
            this.http = http;
            this.raven = raven;
            this.config = config.Value;
        }

        public async Task<ShopifyProductModel> AddProduct(ShopifyProductModel product)
        {
            string endpoint = ShopifyEndpoints.BaseEndpoint(config.APIKey, config.Password, "Aliseeks.MyShopify", ShopifyEndpoints.Products);

            var requestType = new
            {
                Product = product
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestType, jsonSettings));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var response = await http.Post(endpoint, content, (client) =>
               {
                   addAuthenticatoin(client);
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

        public async Task<ShopifyProductModel[]> GetProducts()
        {
            string endpoint = ShopifyEndpoints.BaseEndpoint(config.APIKey, config.Password, "Aliseeks.MyShopify", ShopifyEndpoints.Products);

            var response = await http.Get(endpoint, (client) =>
            {
                addAuthenticatoin(client);
            });

            ShopifyProductModel[] products = null;

            products = JsonConvert.DeserializeObject<ShopifyProductModel[]>(await response.Content.ReadAsStringAsync());

            return products;       
        }

        void addAuthenticatoin(HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"{config.APIKey}:{config.Password}")));
        }
    }
}
