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
        void CacheItems(SearchCriteria search);
        Task<SavedSearchModel> SaveSearch(SavedSearchModel model);
        Task DeleteSearch(SavedSearchModel model);
    }
}
