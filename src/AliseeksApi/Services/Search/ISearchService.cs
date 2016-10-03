using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;
using AliseeksApi.Models;

namespace AliseeksApi.Services.Search
{
    public interface ISearchService
    {
        Task<SearchResultOverview> SearchItems(SearchCriteria search);
        void CacheItems(SearchCriteria search);
        Task<SavedSearchModel> SaveSearch(SavedSearchModel model);
        Task<ItemPriceHistoryModel[]> GetPriceHistories(PriceHistoryRequestModel[] models);
        Task<ItemDetail> ItemSearch(ItemDetail model);
    }
}
