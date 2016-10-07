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

            if(rules.FixedPricePercentage.HasValue)
            {
                adjusted = actualPrice * rules.FixedPricePercentage.Value;
            }
            else
            {
                if(rules.FixedPriceAdjustment.HasValue)
                {
                    adjusted = actualPrice + rules.FixedPriceAdjustment.Value;
                }
                else
                {
                    if(rules.FixedPrice.HasValue)
                    {
                        adjusted = rules.FixedPrice.Value;
                    }
                }
            }

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

            if (rules.FixedStockAdjustment.HasValue)
            {
                adjusted = actualStock + rules.FixedStockAdjustment.Value;
            }
            else
            {
                if (rules.FixedStock.HasValue)
                {
                    adjusted = rules.FixedStock.Value;
                }
            }

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
