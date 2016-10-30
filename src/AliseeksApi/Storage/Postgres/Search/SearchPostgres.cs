using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models;
using AliseeksApi.Models.Search;
using Npgsql;
using Newtonsoft.Json;

namespace AliseeksApi.Storage.Postgres.Search
{
    public class SearchPostgres : ISearchPostgres
    {
        const string searchHistoryInsertColumns = "search, meta, username";
        const string searchHistoryTable = "searchhistory";
        const string itemHistoryInsertColumns = "itemid, price, quantity, seller, meta, lotprice, currency, title, source";
        const string itemHistorySelectColumns = "id, itemid, price, quantity, seller, meta, lotprice, currency, title, source, date";
        const string itemHistoryTable = "itemhistory";
        const string searchCacheInsertColumns = @"searchid, itemid, currency, link, title, price, quantity, shippingprice, unit, lotprice, freeshipping, imageurl, mobileonly, storename, toprateseller, feedback, orders, source";
        const string searchCacheTable = "searchcache";
        const string searchCacheSelectColumns = @"itemid, currency, link, title, price, quantity, shippingprice, unit, lotprice, freeshipping, imageurl, mobileonly, storename, toprateseller, feedback, orders, source";

        const string searchSaveInsertColumns = @"username, criteria";
        const string searchSaveSelectColumns = "id, username, criteria, created";
        const string searchSaveTable = "savesearch";

        const string itemPriceHistorySelectColumns = "id, itemid, source, title, history";
        const string itemPriceHistoryInsertColumns = "itemid, source, title, history";
        const string itemPriceHistoryUpdateColumns = "history";
        const string itemPriceTable = "itemprice";

        IPostgresDb db;

        public SearchPostgres(IPostgresDb db)
        {
            this.db = db;
        }

        public async Task AddSearchAsync(SearchHistoryModel history)
        {
            await db.TransactionAsync(transaction =>
            {
                var cmdParameters = "@search, @meta, @username";
                var cmd = new NpgsqlCommand();
                cmd.Transaction = transaction;
                cmd.Connection = transaction.Connection;
                cmd.CommandText = $@"INSERT INTO {searchHistoryTable} ({searchHistoryInsertColumns}) VALUES ({cmdParameters});";
                cmd.Parameters.AddWithValue("@search", history.Search);
                cmd.Parameters.AddWithValue("@meta", NpgsqlTypes.NpgsqlDbType.Jsonb, (history.Meta == null) ? "" : JsonConvert.SerializeObject(history.Meta));
                cmd.Parameters.AddWithValue("@username", history.User);
                cmd.ExecuteNonQuery();
            });
        }

        public async Task AddItemsAsync(IEnumerable<ItemModel> items)
        {
            await db.TransactionAsync(transaction =>
            {
                foreach (var item in items)
                {
                    var parameters = "@itemid, @price, @quantity, @seller, @meta, @lotprice, @currency, @title, @source";
                    var command = new NpgsqlCommand();
                    command.Transaction = transaction;
                    command.Connection = transaction.Connection;
                    command.CommandText = $@"INSERT INTO {itemHistoryTable} 
                        ({itemHistoryInsertColumns})
                        VALUES ({parameters});";
                    command.Parameters.AddWithValue("@itemid", item.ItemID);
                    command.Parameters.AddWithValue("@price", item.Price);
                    command.Parameters.AddWithValue("@quantity", item.Quantity);
                    command.Parameters.AddWithValue("@seller", item.Seller);
                    command.Parameters.AddWithValue("@meta", NpgsqlTypes.NpgsqlDbType.Jsonb, (item.Meta == null) ? "{}" : JsonConvert.SerializeObject(item.Meta));
                    command.Parameters.AddWithValue("@lotprice", item.LotPrice);
                    command.Parameters.AddWithValue("@currency", item.Currency);
                    command.Parameters.AddWithValue("@title", item.Title);
                    command.Parameters.AddWithValue("@source", item.Source);

                    command.ExecuteNonQuery();
                }
            });
        }

        public async Task AddSavedSearchAsync(SavedSearchModel search)
        {
            await db.TransactionAsync(transaction =>
            {
                var cmdParameters = "@username, @criteria";
                var cmd = new NpgsqlCommand();
                cmd.Transaction = transaction;
                cmd.Connection = transaction.Connection;
                cmd.CommandText = $@"INSERT INTO {searchSaveTable} ({searchSaveInsertColumns}) VALUES ({cmdParameters});";
                cmd.Parameters.AddWithValue("@username", search.Username);
                cmd.Parameters.AddWithValue("@criteria", NpgsqlTypes.NpgsqlDbType.Jsonb, (search.Criteria == null) ? "" : JsonConvert.SerializeObject(search.Criteria));
                cmd.ExecuteNonQuery();
            });
        }

<<<<<<< HEAD
        public async Task DeleteSavedSearchAsync(SavedSearchModel search)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"DELETE FROM {searchSaveTable} WHERE id=@id";
            command.Parameters.AddWithValue("@id", search.ID);
=======
        public async Task AddItemPriceHistoryAsync(ItemPriceHistoryModel model)
        {
            await db.TransactionAsync(transaction =>
            {
                var cmdParameters = "@itemid, @source, @title, @history";
                var cmd = new NpgsqlCommand();
                cmd.Transaction = transaction;
                cmd.Connection = transaction.Connection;
                cmd.CommandText = $@"INSERT INTO {itemPriceTable} ({itemPriceHistoryInsertColumns}) VALUES ({cmdParameters});";
                cmd.Parameters.AddWithValue("@itemid", model.ItemID);
                cmd.Parameters.AddWithValue("@source", model.Source);
                cmd.Parameters.AddWithValue("@title", model.Title);
                cmd.Parameters.AddWithValue("@history", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonConvert.SerializeObject(model.Prices));
                cmd.ExecuteNonQuery();
            });
        }

        public async Task UpdateItemPriceHistoryAsync(ItemPriceHistoryModel model)
        {
            var parameters = "@history";
            var command = new NpgsqlCommand();
            string[] columnUpdates = itemPriceHistoryUpdateColumns.Replace(" ", "").Split(',');
            string[] columnUpdatesParameters = parameters.Replace(" ", "").Split(',');
            var columnUpdateCommand = new string[columnUpdates.Length];

            for (int i = 0; i != columnUpdates.Length && i != columnUpdatesParameters.Length; i++)
            {
                columnUpdateCommand[i] = $"{columnUpdates[i]}={columnUpdatesParameters[i]}";
            }

            command.CommandText = $"UPDATE {itemPriceTable} SET {String.Join(",", columnUpdateCommand)} WHERE id=@id;";
            command.Parameters.AddWithValue("@history", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonConvert.SerializeObject(model.Prices));
            command.Parameters.AddWithValue("@id", model.ID);

            await db.CommandNonqueryAsync(command);
        }

        public async Task<ItemModel[]> SelectItemsAsync(ItemModel item)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {itemHistorySelectColumns} FROM {itemHistoryTable} WHERE itemid=@itemid AND source=@source;";
            command.Parameters.AddWithValue("@itemid", item.ItemID);
            command.Parameters.AddWithValue("@source", item.Source);

            var savedSearches = new List<ItemModel>();

            await db.CommandReaderAsync(command, reader =>
            {
                int ordinal = 0;
                var savedSearch = new ItemModel();
                savedSearch.ID = reader.GetInt32(ordinal++);
                savedSearch.ItemID = reader.GetString(ordinal++);
                savedSearch.Price = (decimal[])reader.GetValue(ordinal++);
                savedSearch.Quantity = reader.GetInt32(ordinal++);
                savedSearch.Seller = reader.GetString(ordinal++);
                savedSearch.Meta = JsonConvert.DeserializeObject<ItemModelMeta>(reader.GetString(ordinal++));
                savedSearch.LotPrice = reader.GetDecimal(ordinal++);
                savedSearch.Currency = reader.GetString(ordinal++);
                savedSearch.Title = reader.GetString(ordinal++);
                savedSearch.Source = reader.GetString(ordinal++);
                savedSearch.CreatedDate = reader.GetDateTime(ordinal++);

                savedSearches.Add(savedSearch);
            });

            return savedSearches.ToArray();
        }

        public async Task<ItemModel> PullTopItemHistoryAsync()
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {itemHistorySelectColumns} FROM {itemHistoryTable} LIMIT 1";

            ItemModel savedSearch = null;

            await db.CommandReaderAsync(command, reader =>
            {
                savedSearch = new ItemModel();
                int ordinal = 0;
                savedSearch.ID = reader.GetInt32(ordinal++);
                savedSearch.ItemID = reader.GetString(ordinal++);
                savedSearch.Price = (decimal[])reader.GetValue(ordinal++);
                savedSearch.Quantity = reader.GetInt32(ordinal++);
                savedSearch.Seller = reader.GetString(ordinal++);
                savedSearch.Meta = JsonConvert.DeserializeObject<ItemModelMeta>(reader.GetString(ordinal++));
                savedSearch.LotPrice = reader.GetDecimal(ordinal++);
                savedSearch.Currency = reader.GetString(ordinal++);
                savedSearch.Title = reader.GetString(ordinal++);
                savedSearch.Source = reader.GetString(ordinal++);
                savedSearch.CreatedDate = reader.GetDateTime(ordinal++);
            });

            return savedSearch;
        }

        public async Task<ItemPriceHistoryModel> SelectItemPriceHistoryAsync(ItemPriceHistoryModel model)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {itemPriceHistorySelectColumns} FROM {itemPriceTable} WHERE itemid=@itemid AND source=@source";
            command.Parameters.AddWithValue("@itemid", model.ItemID);
            command.Parameters.AddWithValue("@source", model.Source);

            ItemPriceHistoryModel savedSearch = null;

            await db.CommandReaderAsync(command, reader =>
            {
                savedSearch = new ItemPriceHistoryModel();
                int ordinal = 0;
                savedSearch.ID = reader.GetInt32(ordinal++);
                savedSearch.ItemID = reader.GetString(ordinal++);
                savedSearch.Source = reader.GetString(ordinal++);
                savedSearch.Title = reader.GetString(ordinal++);
                savedSearch.Prices = JsonConvert.DeserializeObject<ItemPriceModel[]>(reader.GetString(ordinal++));
            });

            return savedSearch;
        }

        public async Task DeleteItemHistoryByItemIDAsync(ItemModel item)
        {
            await db.TransactionAsync(transaction =>
            {
                var cmd = new NpgsqlCommand();
                cmd.Transaction = transaction;
                cmd.Connection = transaction.Connection;
                cmd.CommandText = $@"DELETE FROM {itemHistoryTable} WHERE itemid=@itemid AND source=@source";
                cmd.Parameters.AddWithValue("@itemid", item.ItemID);
                cmd.Parameters.AddWithValue("@source", item.Source);
                cmd.ExecuteNonQuery();
            });
        }

        public async Task<ItemPriceHistoryModel[]> SelectItemPriceHistoriesAsync(PriceHistoryRequestModel[] models)
        {
            var ids = new List<string>();
            foreach(var item in models)
            {
                ids.Add("'" + item.ItemID + item.Source + "'");
            }

            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {itemPriceHistorySelectColumns} FROM {itemPriceTable} WHERE CONCAT(itemid, source) IN ({String.Join(",", ids)})";
            command.Parameters.AddWithValue("@itemids",ids.ToArray());

            var savedSearches = new List<ItemPriceHistoryModel>();

            await db.CommandReaderAsync(command, reader =>
            {
                var savedSearch = new ItemPriceHistoryModel();
                int ordinal = 0;
                savedSearch.ID = reader.GetInt32(ordinal++);
                savedSearch.ItemID = reader.GetString(ordinal++);
                savedSearch.Source = reader.GetString(ordinal++);
                savedSearch.Title = reader.GetString(ordinal++);
                savedSearch.Prices = JsonConvert.DeserializeObject<ItemPriceModel[]>(reader.GetString(ordinal++));
                savedSearches.Add(savedSearch);
            });

            return savedSearches.ToArray();
        }

        public async Task<int> CountSavedSearchs(string username)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT COUNT(id) FROM {searchSaveTable} WHERE username=@username";
            command.Parameters.AddWithValue("username", username);

            var count = 0;

            await db.CommandReaderAsync(command, reader =>
            {
                count = reader.GetInt32(0);
            });

            return count;
        }

        public async Task<SavedSearchModel[]> SelectSaveSearches(string username)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {searchSaveSelectColumns} FROM {searchSaveTable} WHERE username=@username";
            command.Parameters.AddWithValue("username", username);

            var saves = new List<SavedSearchModel>();

            await db.CommandReaderAsync(command, reader =>
            {
                var ord = 0;

                var save = new SavedSearchModel();
                save.ID = reader.GetInt32(ord++);
                save.Username = reader.GetString(ord++);
                save.Criteria = JsonConvert.DeserializeObject<SearchCriteria>(reader.GetString(ord++));
                save.Created = reader.GetDateTime(ord++);

                saves.Add(save);
            });

            return saves.ToArray();
        }

        public async Task DeleteSavedSearch(string username, int id)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"DELETE FROM {searchSaveTable} WHERE username=@username AND id=@id";
            command.Parameters.AddWithValue("username", username);
            command.Parameters.AddWithValue("id", id);

>>>>>>> refs/remotes/origin/feature/itempricetable
            await db.CommandNonqueryAsync(command);
        }
    }
}
