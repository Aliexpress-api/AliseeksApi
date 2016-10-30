using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Dropshipping;
using AliseeksApi.Storage.Postgres.ORM;
using AliseeksApi.Utility;
using Npgsql;
using Newtonsoft.Json;

namespace AliseeksApi.Storage.Postgres.Dropshipping
{
    public class DropshipAccountsPostgres : TableContext<DropshipAccount>
    {
        public DropshipAccountsPostgres(IPostgresDb db) : base(db)
        {
        }

        public async Task CreateAccount(DropshipAccount account)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"INSERT INTO {tableName} (username, account, subscription) VALUES (@username, @account, @subscription);";
            command.Parameters.AddWithValue("@username", account.Username);
            command.Parameters.AddWithValue("@account", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonConvert.SerializeObject(account.Account));
            command.Parameters.AddWithValue("@subscription", account.Subscription);

            await db.CommandNonqueryAsync(command);
        }

        public async Task<DropshipAccount> GetOneByUsername(string username)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {ORMQueryHelper.GetSelectColumns<DropshipAccount>()} FROM {tableName} WHERE username=@username";
            command.Parameters.AddWithValue("@username", username);

            DropshipAccount account = null;

            await db.CommandReaderAsync(command, reader =>
            {
                    account = new DropshipAccount();
                    LoadModel(reader, account);
            });

            return account;            
        }
    }
}
