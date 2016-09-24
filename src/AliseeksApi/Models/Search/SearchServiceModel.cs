using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Services.Search;

namespace AliseeksApi.Models.Search
{
    public enum SearchServiceType
    {
        Aliexpress, DHGate
    }

    public class SearchServiceModelKey
    {
        public SearchCriteria Criteria { get; set; }
        public SearchServiceType Type { get; set; }
    }

    public class SearchServiceModel
    {
        public SearchCriteria Criteria { get; set; }
        public SearchServiceType Type { get; set; }
        public int MaxPage { get; set; }
        public int Page { get; set; }
        public string PageKey { get; set; }

        public SearchServiceModel()
        {
            MaxPage = 0;
            Page = 0;
            PageKey = "";
        }

        public SearchServiceModelKey GetKey()
        {
            return new SearchServiceModelKey()
            {
                Criteria = this.Criteria,
                Type = this.Type
            };
        }
    }
}
