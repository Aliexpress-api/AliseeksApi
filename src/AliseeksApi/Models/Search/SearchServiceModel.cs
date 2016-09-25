using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Services.Search;
using AliseeksApi.Utility.Attributes;
using AliseeksApi.Utility;
using Newtonsoft.Json;

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

        [RedisIgnore]
        public int MaxPage { get; set; }

        [RedisIgnore]
        public int Page { get; set; }

        [RedisIgnore]
        [JsonIgnore]
        public int[] Pages { get; set; }

        [RedisIgnore]
        public string PageKey { get; set; }

        [RedisIgnore]
        public bool Searched { get; set; }

        public SearchServiceModel()
        {
            MaxPage = 0;
            Page = 0;
            PageKey = "";
        }

        public string GetRedisKey()
        {
            return $"{RedisKeyConvert.Serialize(this.Criteria, "servicekey")}:{this.Type.ToString()}";
        }

        public void PopulateState(SearchResultOverview results)
        {
            this.Searched = true;
            this.PageKey = results.Extra.ContainsKey("PagingKey") ? results.Extra["PagingKey"] : "";
            this.MaxPage = results.Pages;
        }
    }
}
