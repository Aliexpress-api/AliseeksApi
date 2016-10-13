using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Storage.Postgres.ORM;
using AliseeksApi.Models.Dropshipping;
using Npgsql;
using AliseeksApi.Utility.Extensions;
using AliseeksApi.Utility;

namespace AliseeksApi.Storage.Postgres.Dropshipping
{
    public class DropshipItemsPostgres : TableContext<DropshipItemModel>
    {
        public DropshipItemsPostgres(IPostgresDb db) : base(db)
        {

        }

        public async Task<DropshipItemModel[]> GetMultipleByUsername(string username)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {ORMQueryHelper.GetSelectColumns<DropshipItemModel>()} FROM {tableName} WHERE username=@username";
            command.Parameters.AddWithValue("@username", username);

            var items = new List<DropshipItemModel>();

            await db.CommandReaderAsync(command, reader =>
            {
                var item = new DropshipItemModel();
                LoadModel(reader, item);
                items.Add(item);
            });

            return items.ToArray();
        }

        public async Task<DropshipItemModel> GetOneByID(int id)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {ORMQueryHelper.GetSelectColumns<DropshipItemModel>()} FROM {tableName} WHERE id=@id";
            command.Parameters.AddWithValue("@id", id);

            var item = new DropshipItemModel();

            await db.CommandReaderAsync(command, reader =>
            {
                item = new DropshipItemModel();
                LoadModel(reader, item);
            });

            return item;
        }

        public async Task<int> CountItems()
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT COUNT(id) FROM {tableName} WHERE 1=1;";

            int count = 0;

            await db.CommandReaderAsync(command, reader =>
            {
                    count = reader.GetInt32(0);
            });

            return count;
        }

        public async Task<DropshipAccountItem[]> GetMultipleWithAccount(int take, int skip)
        {
            var accountTableName = ORMQueryHelper.GetTableName<DropshipAccount>();

            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {ORMQueryHelper.GetSelectColumns<DropshipItemModel>()},{ORMQueryHelper.GetSelectColumns<DropshipAccount>()} FROM {tableName} JOIN {accountTableName} ON {tableName}.username={accountTableName}.username LIMIT {take} OFFSET {skip};";

            var items = new List<DropshipAccountItem>();

            await db.CommandReaderAsync(command, reader =>
            {
                var item = new DropshipAccountItem();
                reader.ReadModel<DropshipItemModel>(item.Item);
                reader.ReadModel<DropshipAccount>(item.Account);
                items.Add(item);
            });

            return items.ToArray();
        }

        public async Task<DropshipAccountItem[]> GetMultipleWithAccountByUsername(string username)
        {
            var accountTableName = ORMQueryHelper.GetTableName<DropshipAccount>();

            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {ORMQueryHelper.GetSelectColumns<DropshipItemModel>()},{ORMQueryHelper.GetSelectColumns<DropshipAccount>()} FROM {tableName} JOIN {accountTableName} ON {tableName}.username={accountTableName}.username WHERE {tableName}.username=@username;";
            command.Parameters.AddWithValue("@username", username);

            var items = new List<DropshipAccountItem>();

            await db.CommandReaderAsync(command, reader =>
            {
                    var item = new DropshipAccountItem();
                    reader.ReadModel<DropshipItemModel>(item.Item);
                    reader.ReadModel<DropshipAccount>(item.Account);
                    items.Add(item);
            });

            return items.ToArray();
        }
    }
}
