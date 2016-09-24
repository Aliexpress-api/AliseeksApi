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
        public string SearchText { get; set; }

        public double? PriceFrom { get; set; }

        public double? PriceTo { get; set; }

        [RedisDefault("")]
        public string ShipFrom { get; set; }

        [RedisDefault(false)]
        public string ShipTo { get; set; }

        [RedisDefault(false)]
        public bool? FreeShipping { get; set; }

        [RedisDefault(false)]
        public bool? SaleItems { get; set; }

        [RedisDefault(false)]
        public bool? AppOnly { get; set; }

        [RedisIgnore("servicekey")]
        public int? Page { get; set; }

        [JsonIgnore]
        [RedisIgnore]
        public SearchCriteriaMeta Meta { get; set; }

        public SearchCriteria()
        {
            Page = 1;
        }
    }

    public class SearchCriteriaMeta
    {
        public string User { get; set; }
    }
}
