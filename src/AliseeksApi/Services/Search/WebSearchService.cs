using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;

namespace AliseeksApi.Services.Search
{
    public abstract class WebSearchService
    {
        public SearchServiceType ServiceType { get; set; }
        public SearchServiceModel ServiceModel { get; set; }

        public WebSearchService(SearchServiceType type)
        {
            ServiceType = type;
        }

        public abstract Task<SearchResultOverview> SearchItems();
        public virtual Task<SearchResultOverview> SearchItems(SearchServiceModel model)
        {
            this.ServiceModel = model;
            return this.SearchItems();
        }
    }
}
