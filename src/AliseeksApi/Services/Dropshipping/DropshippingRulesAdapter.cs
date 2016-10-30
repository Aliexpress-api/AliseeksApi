using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Shopify;
using AliseeksApi.Models.Search;
using AliseeksApi.Models;
using AliseeksApi.Models.Dropshipping;
using AliseeksApi.Services.Dropshipping.Shopify;
using AliseeksApi.Utility.Extensions;

namespace AliseeksApi.Services.Dropshipping
{
    public class DropshippingRulesAdapter
    {
        public DropshippingRulesAdapter()
        {

        }
    }

    public static class DropshippingRulesExtensions
    {

        public static bool ApplyRules(this DropshipListingRules rules, ItemDetail detail, ShopifyProductModel product)
        {
            var changes = false;

            changes = changes.Consume(ApplyPricingRules(rules, detail, product));
            changes = changes.Consume(ApplyStockRules(rules, detail, product));

            return changes;
        }

        private static bool ApplyPricingRules(DropshipListingRules rules, ItemDetail detail, ShopifyProductModel product)
        {
            var actualPrice = detail.Price + detail.ShippingPrice;
            var adjusted = actualPrice;

            switch(rules.PriceRule)
            {
                case PriceRule.FixedPrice:
                    adjusted = rules.Price;
                    break;

                case PriceRule.PriceAdjustment:
                    adjusted = actualPrice + rules.Price;
                    break;

                case PriceRule.PricePercentage:
                    adjusted = actualPrice * (1.00m + rules.Price / 100); //Convert to percentage
                    break;

                default:
                    break;
            }

            if(adjusted < 0) { adjusted = 0.01m; } //Cant have negative price

            var variant = product.Variants.FirstOrDefault() ?? new ShopifyVarant();
            if (variant.Price != Math.Truncate(adjusted * 100) / 100)
            {
                variant.Price = adjusted;
                return true;
            }

            return false;
        }

        private static bool ApplyStockRules(DropshipListingRules rules, ItemDetail detail, ShopifyProductModel product)
        {
            var actualStock = detail.Stock;
            var adjusted = actualStock;

            switch(rules.StockRule)
            {
                case StockRule.FixedStock:
                    adjusted = rules.Stock;
                    break;

                case StockRule.StockAdjustment:
                    adjusted = actualStock - rules.Stock;
                    break;

                default:
                    break;
            }

            if (adjusted < 0) { adjusted = 0; } //cant have negative inventory

            var variant = product.Variants.FirstOrDefault() ?? new ShopifyVarant();
            if (variant.InventoryQuantity != adjusted)
            {
                variant.InventoryQuantity = adjusted;
                return true;
            }

            return false;
        }
    }
}
