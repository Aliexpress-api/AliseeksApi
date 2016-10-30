using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;
using AliseeksApi.Models;

namespace AliseeksApi.Storage.Postgres.Search
{
    public interface ISearchPostgres
    {
        Task AddSearchAsync(SearchHistoryModel history);
        Task AddItemsAsync(IEnumerable<ItemModel> items);
        Task AddSavedSearchAsync(SavedSearchModel search);
        Task AddItemPriceHistoryAsync(ItemPriceHistoryModel model);
        Task UpdateItemPriceHistoryAsync(ItemPriceHistoryModel model);
        Task<ItemPriceHistoryModel> SelectItemPriceHistoryAsync(ItemPriceHistoryModel model);
        Task<ItemModel[]> SelectItemsAsync(ItemModel item);
        Task<ItemModel> PullTopItemHistoryAsync();
        Task DeleteItemHistoryByItemIDAsync(ItemModel item);
        Task<ItemPriceHistoryModel[]> SelectItemPriceHistoriesAsync(PriceHistoryRequestModel[] models);
        Task<int> CountSavedSearchs(string username);
        Task<SavedSearchModel[]> SelectSaveSearches(string username);
        Task DeleteSavedSearch(string username, int id);
    }
}
