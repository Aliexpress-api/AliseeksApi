using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Storage.Postgres.ORM;
using AliseeksApi.Models.OAuth;
using AliseeksApi.Utility;
using Npgsql;

namespace AliseeksApi.Storage.Postgres.OAuth
{
    public class OAuthPostgres : TableContext<OAuthAccountModel>
    {
        public OAuthPostgres(IPostgresDb db) : base(db)
        {

        }

        public async Task<OAuthAccountModel[]> GetMultipleByUsername(string username)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {ORMQueryHelper.GetSelectColumns<OAuthAccountModel>()} FROM {tableName} WHERE username=@username";
            command.Parameters.AddWithValue("@username", username);

            var items = new List<OAuthAccountModel>();

            await db.CommandReaderAsync(command, reader =>
            {
                var item = new OAuthAccountModel();
                LoadModel(reader, item);
                items.Add(item);
            });

            return items.ToArray();
        }
    }
}
