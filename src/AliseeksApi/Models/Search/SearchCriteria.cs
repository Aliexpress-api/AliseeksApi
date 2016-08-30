using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Utility.Attributes;

namespace AliseeksApi.Models.Search
{
    public class SearchCriteria
    {
        [QueryStringEncode("SearchText")]
        public string SearchText { get; set; }

        [QueryStringEncode("minPrice")]
        public double? PriceFrom { get; set; }

        [QueryStringEncode("maxPrice")]
        public double? PriceTo { get; set; }

        [QueryStringEncode("shipFromCountry")]
        public string ShipFrom { get; set; }

        [QueryStringEncode("shipCountry")]
        public string ShipTo { get; set; }

        [QueryStringEncode("isFreeShip")]
        public bool? FreeShipping { get; set; }

        [QueryStringEncode("isOnSale")]
        public bool? SaleItems { get; set; }

        [QueryStringEncode("isMobileExclusive")]
        public bool? AppOnly { get; set; }

        [QueryStringEncode("page")]
        public int? Page { get; set; }
    }
}
