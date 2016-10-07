using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AliseeksApi.Models.Shopify
{
    public enum InventoryPolicy
    {
        Deny, Continue
    }

    public enum InventoryManagement
    {
        Blank, Shopify
    }

    public enum FulfillmentService
    {
        Manual
    }

    public class ShopifyVarant
    {
        public static JsonSerializerSettings SerializationSettings
        {
            get
            {
                return new JsonSerializerSettings()
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };
            }
        }

        public string ID { get; set; }
        public string Title { get; set; }
        public decimal? Price { get; set; }
        public string Sku { get; set; }
        public string Position { get; set; }
        public int? Grams { get; set; }

        [JsonProperty(PropertyName = "inventory_policy")]
        [JsonConverter(typeof(LowerCaseEnumConverter))]
        public InventoryPolicy? InventoryPolicy { get; set; } = null;

        [JsonProperty(PropertyName = "compare_at_price")]
        public int? CompareAtPrice { get; set; }

        [JsonProperty(PropertyName = "fulfillment_service")]
        [JsonConverter(typeof(LowerCaseEnumConverter))]
        public FulfillmentService? FulfillmentService { get; set; } = null;

        [JsonProperty(PropertyName = "inventory_management")]
        [JsonConverter(typeof(LowerCaseEnumConverter))]
        public InventoryManagement? InventoryManagement { get; set; } = null;

        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty(PropertyName = "updated_at")]
        public DateTime? UpdatedAt { get; set; }
        
        public bool? Taxable { get; set; }
        public string Barcode { get; set; }

        [JsonProperty(PropertyName = "image_id")]
        public string ImageID { get; set; }     
        
        [JsonProperty(PropertyName = "inventory_quantity")]
        public int? InventoryQuantity { get; set; }   

        public decimal? Weight { get; set; }

        [JsonProperty(PropertyName = "weight_unit")]
        public string WeightUnit { get; set; }

        [JsonProperty(PropertyName = "old_inventory_quantity")]
        public int? OldInventoryQuantity { get; set; }

        [JsonProperty(PropertyName = "requires_shipping")]
        public bool? RequiresShipping { get; set; }
    }

    public class LowerCaseEnumConverter : StringEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string enString = value.ToString();

            writer.WriteValue(enString.ToLower());
        }
    }
}
