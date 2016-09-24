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
        const string searchCacheInsertColumns = @"searchid, itemid, currency, link, title, price, quantity, shippingprice,
            unit, lotprice, freeshipping, imageurl, mobileonly, storename, toprateseller, feedback, orders, source";
        const string searchCacheTable = "searchcache";
        const string searchCacheSelectColumns = @"itemid, currency, link, title, price, quantity, shippingprice,
            unit, lotprice, freeshipping, imageurl, mobileonly, storename, toprateseller, feedback, orders, source";

        IPostgresDb db;

        public SearchPostgres(IPostgresDb db)
        {
            this.db = db;
        }

        public async Task AddSearchAsync(SearchHistoryModel history, IEnumerable<ItemModel> items)
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

        public async Task AddSearchCacheAsync(string searchid, IEnumerable<Item> items)
        {
            await db.TransactionAsync(transaction =>
            {
                foreach (var item in items)
                {
                    var cmdParameters = @"@searchid, @itemid, @currency, @link, @title, @price, @quantity, @shippingprice, @unit,
                    @lotprice, @freeshipping, @imageurl, @mobileonly, @storename, @toprateseller, @feedback, @orders, @source";
                    var command = new NpgsqlCommand();
                    command.Transaction = transaction;
                    command.Connection = transaction.Connection;
                    command.CommandText = $@"INSERT INTO {searchCacheInsertColumns} 
                        ({searchCacheTable})
                        VALUES ({cmdParameters});";
                    command.Parameters.AddWithValue("@searchid", searchid);
                    command.Parameters.AddWithValue("@itemid", item.ItemID);
                    command.Parameters.AddWithValue("@currency", item.Currency);
                    command.Parameters.AddWithValue("@link", item.Link);
                    command.Parameters.AddWithValue("@title", item.Name);
                    command.Parameters.AddWithValue("@price", item.Price);
                    command.Parameters.AddWithValue("@quantity", item.Quantity);
                    command.Parameters.AddWithValue("@shippingprice", item.ShippingPrice);
                    command.Parameters.AddWithValue("@unit", item.Unit);
                    command.Parameters.AddWithValue("@lotprice", item.LotPrice);
                    command.Parameters.AddWithValue("@freeshipping", item.FreeShipping);
                    command.Parameters.AddWithValue("@imageurl", item.ImageURL);
                    command.Parameters.AddWithValue("@mobileonly", item.MobileOnly);
                    command.Parameters.AddWithValue("@storename", item.StoreName);
                    command.Parameters.AddWithValue("@topratedseller", item.TopRatedSeller);
                    command.Parameters.AddWithValue("@feedback", item.Feedback);
                    command.Parameters.AddWithValue("@order", item.Orders);

                    command.ExecuteNonQuery();
                }
            });
        }

        public async Task<IEnumerable<Item>> RetriveSearchCacheAsync(string searchid)
        {
            var items = new List<Item>();

            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {searchCacheSelectColumns} FROM {searchCacheTable} WHERE searchid=@searchid";
            command.Parameters.AddWithValue("@searchid", searchid);

            await db.CommandReaderAsync(command, reader =>
            {
                while(reader.Read())
                {
                    int o = 0;
                    var item = new Item();
                    item.ItemID = reader.GetString(o++);
                    item.Currency = reader.GetString(o++);
                    item.Link = reader.GetString(o++);
                    item.Name = reader.GetString(o++);
                    item.Price = reader[o++] as decimal[];
                    item.Quantity = reader.GetInt32(o++);
                    item.ShippingPrice = reader.GetDecimal(o++);
                    item.Unit = reader.GetString(o++);
                    item.LotPrice = reader.GetDecimal(o++);
                    item.FreeShipping = reader.GetBoolean(o++);
                    item.ImageURL = reader.GetString(o++);
                    item.StoreName = reader.GetString(o++);
                    item.TopRatedSeller = reader.GetBoolean(o++);
                    item.Feedback = reader.GetInt32(o++);
                    item.Orders = reader.GetInt32(o++);
                    item.Source = reader.GetString(o++);

                    items.Add(item);
                }

            });

            return items;
        }
    }
}
