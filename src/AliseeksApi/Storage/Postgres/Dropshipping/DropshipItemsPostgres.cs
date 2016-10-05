using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Storage.Postgres.ORM;
using AliseeksApi.Models.Dropshipping;
using Npgsql;

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
            command.CommandText = $"SELECT {String.Join(",", SelectColumns())} FROM {tableName} WHERE username=@username";
            command.Parameters.AddWithValue("@username", username);

            var items = new List<DropshipItemModel>();

            await db.CommandReaderAsync(command, reader =>
            {
                while (reader.Read())
                {
                    var item = new DropshipItemModel();
                    LoadModel(reader, item);
                    items.Add(item);
                }
            });

            return items.ToArray();
        }
    }
}
