using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;

namespace AliseeksApi.Services.Search
{
    public interface ISearchService
    {
        Task<SearchResultOverview> SearchItems(SearchCriteria search);
        Task CacheItems(SearchCriteria search);
    }
}
