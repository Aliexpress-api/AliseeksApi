using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Storage.Postgres.ORM;
using AliseeksApi.Models.OAuth;
using AliseeksApi.Utility;
using Npgsql;
using Newtonsoft.Json;

namespace AliseeksApi.Storage.Postgres.OAuth
{
    public class OAuthPostgres : TableContext<OAuthAccountModel>
    {
        public OAuthPostgres(IPostgresDb db) : base(db)
        {

        }

        public async Task CreateOAuth(OAuthAccountModel model)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"INSERT INTO {tableName} (username, access_token, meta, extra, accountid, service) VALUES (@username, @access_token, @meta, @extra, @accountid, @service);";
            command.Parameters.AddWithValue("@username", model.Username);
            command.Parameters.AddWithValue("@access_token", model.AccessToken);
            command.Parameters.AddWithValue("@meta", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonConvert.SerializeObject(model.Meta));
            command.Parameters.AddWithValue("@extra", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonConvert.SerializeObject(model.Extra));
            command.Parameters.AddWithValue("@accountid", model.AccountID);
            command.Parameters.AddWithValue("@service", model.Service);

            await db.CommandNonqueryAsync(command);
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

        public async Task DeleteByID(int id, string username)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"DELETE FROM {tableName} WHERE id=@id AND username=@username";
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@username", username);

            await db.CommandNonqueryAsync(command);
        }
    }
}
