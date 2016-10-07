using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AliseeksApi.Models.Shopify
{
    public class ShopifyProductModel
    {
        public string Title { get; set; }
        public string ID { get; set; }

        [JsonProperty("body_html", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string BodyHtml { get; set; }
        public string Vendor { get; set; }

        [JsonProperty("product_type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ProductType { get; set; }
        public ShopifyImageType[] Images { get; set; }

        public List<ShopifyVarant> Variants { get; set; } = new List<ShopifyVarant>();
    }

    public class ShopifyImageType
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ID { get; set; }
        public string Src { get; set; }
    }
}
