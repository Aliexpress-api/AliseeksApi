using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Utility.Attributes;
using Newtonsoft.Json;

namespace AliseeksApi.Models.Search
{
    public class SearchCriteria
    {
        [QueryStringEncode(SearchService.Aliexpress, "SearchText")]
        [QueryStringEncode(SearchService.DHGate, "searchKey")]
        public string SearchText { get; set; }

        [QueryStringEncode(SearchService.Aliexpress, "minPrice")]
        public double? PriceFrom { get; set; }

        [QueryStringEncode(SearchService.Aliexpress, "maxPrice")]
        public double? PriceTo { get; set; }

        [QueryStringEncode(SearchService.Aliexpress, "shipFromCountry")]
        public string ShipFrom { get; set; }

        [QueryStringEncode(SearchService.Aliexpress, "shipCountry")]
        public string ShipTo { get; set; }

        [QueryStringEncode(SearchService.Aliexpress, "isFreeShip")]
        public bool? FreeShipping { get; set; }

        [QueryStringEncode(SearchService.Aliexpress, "isOnSale")]
        public bool? SaleItems { get; set; }

        [QueryStringEncode(SearchService.Aliexpress, "isMobileExclusive")]
        public bool? AppOnly { get; set; }

        [QueryStringEncode(SearchService.Aliexpress, "page")]
        public int? Page { get; set; }

        [JsonIgnore]
        public SearchCriteriaMeta Meta { get; set; }
    }

    public class SearchCriteriaMeta
    {
        public string User { get; set; }
    }
}
