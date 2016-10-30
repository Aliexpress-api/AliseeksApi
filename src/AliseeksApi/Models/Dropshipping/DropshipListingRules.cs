using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.Dropshipping
{
    public enum PriceRule
    {
        FixedPrice,
        PriceAdjustment,
        PricePercentage,
        None
    }

    public enum StockRule
    {
        FixedStock,
        StockAdjustment,
        None
    }

    public class DropshipListingRules
    {
        public static DropshipListingRules Default
        {
            get
            {
                return new DropshipListingRules()
                {
                    PriceRule = PriceRule.PricePercentage,
                    Price = 10.0m,
                    StockRule = StockRule.StockAdjustment,
                    Stock = 5
                };
            }
        }

        public decimal Price { get; set; }
        public PriceRule PriceRule { get; set; }
        public int Stock { get; set; }
        public StockRule StockRule { get; set; }
    }
}
