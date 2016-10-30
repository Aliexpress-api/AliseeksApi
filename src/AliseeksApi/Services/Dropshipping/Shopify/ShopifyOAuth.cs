using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using AliseeksApi.Models.Shopify;
using AliseeksApi.Configuration;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using AliseeksApi.Utility.Extensions;   

namespace AliseeksApi.Services.Dropshipping.Shopify
{
    public class ShopifyOAuth
    {
        private readonly ShopifyOptions config;
        private const string scopes = "read_products,write_products,read_orders,write_orders,read_shipping,write_shipping,read_fulfillments,write_fulfillments";
        
        public ShopifyOAuth(IOptions<ShopifyOptions> config)
        {
            this.config = config.Value;
        }

        public ShopifyOAuthRequest GetOAuthRequest(string shop, string redirecturi)
        {
            var nouce = GenerateNouce(shop);

            var shopifyOauth = new ShopifyOAuthRequest()
            {
                APIKey = config.APIKey,
                Nounce = nouce,
                Scopes = scopes,
                Shop = shop,
                RedirectUri = redirecturi,
                Uri = $"https://{shop}.myshopify.com/admin/oauth/authorize?client_id={config.APIKey}&scope={scopes}&redirect_uri={redirecturi}&state={nouce}"
            };

            return shopifyOauth;
        }

        public bool VerifyOAuthRequest(ShopifyOAuthResponse response)
        {
            var somethingNotRight = false;
            somethingNotRight.Consume(!VerifyHMAC(response));
            somethingNotRight.Consume(!VerifyHostname(response.Shop));
            somethingNotRight.Consume(GenerateNouce(response.Shop.Replace(".myshopify.com", "")) != response.State);

            return !somethingNotRight;
        }

        public bool VerifyScope(string scope)
        {
            return scope == scopes;
        }

        private bool VerifyHMAC(ShopifyOAuthResponse response)
        {
            var query = QueryHelpers.ParseQuery(response.Query);
            var keyvalueStrings = new List<string>();

            foreach (var key in query.Keys)
            {
                if(key != "hmac")
                {
                    var formatKey = key.Replace("=", "%3D");
                    var relation = $"{formatKey}={String.Join("", query[key])}".Replace("&", "%26").Replace("%", "%25");
                    keyvalueStrings.Add(relation);
                }
            }

            var sha256 = new HMACSHA256();
            sha256.Key = Encoding.UTF8.GetBytes(config.SharedSecret);

            var keyvaluestring = String.Join("&", keyvalueStrings.ToArray());
            
            var hmacBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyvaluestring));

            //Convert to Hex String
            var hmac = String.Empty;
            hmacBytes.ForEach(x =>
            {
                hmac += x.ToString("x2");
            });

            return hmac == response.Hmac;
        }

        private bool VerifyHostname(string hostname)
        {
            var somethingNotRight = false;
            somethingNotRight.Consume(!hostname.EndsWith("shopify.com"));

            string weirdCharacters = String.Empty;
            foreach(var c in hostname)
            {
                if (char.IsLetterOrDigit(c) || c == '.' || c == '-')
                    continue;
                weirdCharacters += c;
            }

            somethingNotRight.Consume(weirdCharacters != String.Empty);

            return !somethingNotRight;
        }

        private string GenerateNouce(string shop)
        {
            var sha1 = SHA1.Create();
            var hashedShop = Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(shop)));

            return hashedShop;
        }
    }
}
