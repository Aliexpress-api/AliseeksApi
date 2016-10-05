using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Dropshipping;
using AliseeksApi.Storage.Postgres.ORM;
using Npgsql;

namespace AliseeksApi.Storage.Postgres.Dropshipping
{
    public class DropshipAccountsPostgres : TableContext<DropshipAccount>
    {
        public DropshipAccountsPostgres(IPostgresDb db) : base(db)
        {
        }

        public async Task<DropshipAccount> GetOneByUsername(string username)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"SELECT {String.Join(",", SelectColumns())} FROM {tableName} WHERE username=@username";
            command.Parameters.AddWithValue("@username", username);

            DropshipAccount account = null;

            await db.CommandReaderAsync(command, reader =>
            {
                while(reader.Read())
                {
                    account = new DropshipAccount();
                    LoadModel(reader, account);
                }
            });

            return account;            
        }
    }
}
