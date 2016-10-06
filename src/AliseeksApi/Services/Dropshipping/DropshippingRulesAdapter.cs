using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Shopify;
using AliseeksApi.Models.Search;
using AliseeksApi.Models;
using AliseeksApi.Models.Dropshipping;
using AliseeksApi.Services.Dropshipping.Shopify;

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

        public static ShopifyProductModel ApplyRules(this DropshipListingRules rules, ItemDetail detail, ShopifyProductModel product)
        {
            ApplyPricingRules(rules, detail, product);
            ApplyStockRules(rules, detail, product);

            return product;
        }

        private static ShopifyProductModel ApplyPricingRules(DropshipListingRules rules, ItemDetail detail, ShopifyProductModel product)
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

            product.Variants.Add(new ShopifyVariant().Price(adjusted).Build());

            return product;
        }

        private static ShopifyProductModel ApplyStockRules(DropshipListingRules rules, ItemDetail detail, ShopifyProductModel product)
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

            product.Variants.Add(new ShopifyVariant().InventoryQuantity(adjusted).Build());

            return product;
        }
    }
}
