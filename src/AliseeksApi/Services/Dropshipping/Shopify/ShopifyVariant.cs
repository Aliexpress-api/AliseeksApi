using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Dynamic;

namespace AliseeksApi.Services.Dropshipping.Shopify
{
    public enum InventoryPolicy
    {
        Deny, Continue
    }

    public enum InventoryManagement
    {
        Blank, Shopify
    }

    public class ShopifyVariant
    {
        dynamic variant = new ExpandoObject();

        public ShopifyVariant Price(decimal price)
        {
            variant.price = price;
            return this;
        }

        public ShopifyVariant InventoryManagement(InventoryManagement managed)
        {
            variant.inventory_management = managed.ToString().ToLower();
            return this;
        }

        public ShopifyVariant RequireShipping(bool shipping)
        {
            variant.requires_shipping = shipping;
            return this;
        }

        public ShopifyVariant InventoryPolicy(InventoryPolicy policy)
        {
            variant.inventory_policy = policy.ToString().ToLower();
            return this;
        }

        public ShopifyVariant InventoryQuantity(int quantity)
        {
            variant.inventory_quantity = quantity;
            return this;
        }

        public ShopifyVariant Taxable(bool taxable)
        {
            variant.taxable = taxable;
            return this;
        }

        public object Build()
        {
            return variant;
        }
    }
}
