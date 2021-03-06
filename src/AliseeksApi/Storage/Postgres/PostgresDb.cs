﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Microsoft.Extensions.Options;
using AliseeksApi.Configuration;
using System.Data.Common;
using SharpRaven.Core;
using SharpRaven.Core.Data;
using AliseeksApi.Utility.Extensions;

namespace AliseeksApi.Storage.Postgres
{
    public class PostgresDb : IPostgresDb
    {
        PostgresOptions config;
        private readonly IRavenClient raven;

        public PostgresDb(IOptions<PostgresOptions> config, IRavenClient raven)
        {
            this.config = config.Value;
            this.raven = raven;
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
                var sentry = new SentryEvent(e);
                await raven.CaptureNetCoreEventAsync(sentry);
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
                var sentry = new SentryEvent(e);
                await raven.CaptureNetCoreEventAsync(sentry);
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
                var sentry = new SentryEvent(e);
                await raven.CaptureNetCoreEventAsync(sentry);
            }
        }
    }
}
