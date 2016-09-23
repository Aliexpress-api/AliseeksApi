﻿using System;
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

        public string ShipFrom { get; set; }

        public string ShipTo { get; set; }

        public bool? FreeShipping { get; set; }

        public bool? SaleItems { get; set; }

        public bool? AppOnly { get; set; }

        public int? Page { get; set; }

        [JsonIgnore]
        public SearchCriteriaMeta Meta { get; set; }
    }

    public class SearchCriteriaMeta
    {
        public string User { get; set; }
    }
}
