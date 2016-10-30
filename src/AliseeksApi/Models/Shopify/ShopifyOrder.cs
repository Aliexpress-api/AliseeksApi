using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AliseeksApi.Models.Shopify
{
    public enum FinancialStatus
    {
        Pending, Authorized, Partially_Paid, Paid, Partially_Refunded, Refunded, Voided
    }

    public enum FulfillmentStatus
    {
        Fulfilled, Partial
    }

    public class ShopifyOrder
    {
        public string ID { get; set; }
        public string Email { get; set; }

        [JsonProperty("closed_at")]
        public DateTime? ClosedAt { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        //        public int Number { get; set; }
        //        public string Note { get; set; }
        //        public string Token { get; set; }
        //        public string Gateway { get; set; }
        //        public bool Test { get; set; }

        [JsonProperty("total_price")]
        public decimal? TotalPrice { get; set; }

        [JsonProperty("subtotal_price")]
        public decimal? SubtotalPrice { get; set; }
        //        public decimal? TotalWeight { get; set; }

        [JsonProperty("total_tax")]
        public decimal? TotalTax { get; set; }

        [JsonProperty("taxes_included")]
        public bool? TaxesInclude { get; set; }
        public string Currency { get; set; }

        [JsonProperty("financial_status")]
        public string FinancialStatus { get; set; }
        //        public bool? Confirmed { get; set; }

        [JsonProperty("total_discounts")]
        public decimal? TotalDiscounts { get; set; }

        [JsonProperty("total_line_items_price")]
        public decimal? TotalLineItemsPrice { get; set; }
        //        public string CartToken { get; set; }
        //        public bool? BuyerAcceptsMarketing { get; set; }
        public string Name { get; set; }

        [JsonProperty("cancelled_at")]
        public DateTime? CancelledAt { get; set; }

        [JsonProperty("cancel_reason")]
        public string CancelReason { get; set; }
        //        public decimal? TotalPriceUSD { get; set; }
        //        public string CheckoutToken { get; set; }
        //        public string UserID { get; set; }
        //        public string LocationID { get; set; }
        [JsonProperty("order_number")]
        public string OrderNumber { get; set; }

        [JsonProperty("fulfillment_status")]
        public string FulfillmentStatus { get; set; }
        //        public string ContactEmail { get; set; }

        [JsonProperty("line_items")]
        public ShopifyOrderLineItem[] LineItems { get; set; } = new ShopifyOrderLineItem[0];
    }

    public class ShopifyOrderLineItem
    {
        public string ID { get; set; }

        [JsonProperty("fulfillment_status")]
        public string FulfillmentStatus { get; set; }

        [JsonProperty("fulfillment_quantity")]
        public int? FulfillableQuantity { get; set; }

        [JsonProperty("quantity")]
        public int? Quantity { get; set; }

        [JsonProperty("product_id")]
        public string ProductID { get; set; }

        [JsonProperty("variant_id")]
        public string VariantID { get; set; }

        [JsonProperty("variant_title")]
        public string VariantTitle { get; set; }
        public decimal? Price { get; set; }

        [JsonProperty("total_discount")]
        public string TotalDiscount { get; set; }
    }

    public class ShopifyAddress
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Company { get; set; }
        public string Country { get; set; }

        [JsonProperty("first_name")]
        public string Firstname { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Phone { get; set; }
        public string Providence { get; set; }
        public string Zip { get; set; }
        public string Name { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }

        [JsonProperty("province_code")]
        public string ProvinceCode { get; set; }
    }
}
