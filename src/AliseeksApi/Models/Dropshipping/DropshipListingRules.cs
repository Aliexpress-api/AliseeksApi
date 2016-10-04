using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Models.Dropshipping
{
    public class DropshipListingRules
    {
        public static DropshipListingRules Default
        {
            get
            {
                return new DropshipListingRules()
                {
                    FixedPricePercentage = 10,
                    FixedPriceAdjustment = 10
                };
            }
        }

        public decimal? FixedPrice { get; set; }
        public decimal? FixedPriceAdjustment { get; set; }
        public decimal? FixedPricePercentage { get; set; }
        public decimal? FixedStock { get; set; }
        public decimal? FixedStockAdjustment { get; set; }
    }
}
