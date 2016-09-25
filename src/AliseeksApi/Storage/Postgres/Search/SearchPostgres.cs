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
        const string itemHistoryTable = "itemhistory";
        const string searchCacheInsertColumns = @"searchid, itemid, currency, link, title, price, quantity, shippingprice, unit, lotprice, freeshipping, imageurl, mobileonly, storename, toprateseller, feedback, orders, source";
        const string searchCacheTable = "searchcache";
        const string searchCacheSelectColumns = @"itemid, currency, link, title, price, quantity, shippingprice, unit, lotprice, freeshipping, imageurl, mobileonly, storename, toprateseller, feedback, orders, source";

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
    }
}
