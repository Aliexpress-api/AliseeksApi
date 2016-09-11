using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Microsoft.Extensions.Options;
using AliseeksApi.Configuration;
using System.Data.Common;

namespace AliseeksApi.Storage.Postgres
{
    public class PostgresDb : IPostgresDb
    {
        PostgresOptions config;

        public PostgresDb(IOptions<PostgresOptions> config)
        {
            this.config = config.Value;
        }

        public NpgsqlConnection Connect()
        {
            return new NpgsqlConnection($@"Host={config.Host};Port={config.Port};
                        Username={config.Username};Password={config.Password};Database={config.Database}");
        }

        public async Task TransactionAsync(Action<NpgsqlTransaction> func)
        {
            try
            {
                using (var connection = this.Connect())
                {
                    await connection.OpenAsync();

                    var transaction = connection.BeginTransaction();

                    func(transaction);

                    try
                    {
                        await transaction.CommitAsync();
                    }
                    catch(Exception e)
                    {
                        await transaction.RollbackAsync();
                        throw e;
                    }
                }
            }
            catch (Exception e)
            {
                //Rethrow until we can find a better way to handle errors
                throw e;
            }
        }

        public async Task CommandReaderAsync(NpgsqlCommand command, Action<DbDataReader> func)
        {
            try
            {
                using (var connection = this.Connect())
                {
                    await connection.OpenAsync();

                    command.Connection = connection;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            func(reader);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                //Rethrow until we can find a better way to handle errors
                throw e;
            }
        }

        public async Task CommandNonqueryAsync(NpgsqlCommand command)
        {
            try
            {
                using (var connection = this.Connect())
                {
                    await connection.OpenAsync();

                    command.Connection = connection;

                    await command.ExecuteNonQueryAsync();
                }
            }
            catch(Exception e)
            {
                //Rethrow until we can find a better way to handle errors
                throw e;
            }
        }
    }
}
