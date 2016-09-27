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
    }
}
