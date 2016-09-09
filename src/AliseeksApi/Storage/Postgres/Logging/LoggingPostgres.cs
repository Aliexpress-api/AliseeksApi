using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Logging;
using Npgsql;

namespace AliseeksApi.Storage.Postgres.Logging
{
    public class LoggingPostgres : ILoggingPostgres
    {
        const string insertExceptionLogColumns = "criticality, message, stacktrace";
        const string exceptionLogTable = "exceptions";

        IPostgresDb db;

        public LoggingPostgres(IPostgresDb db)
        {
            this.db = db;
        }

        public async Task AddExceptionLogAsync(ExceptionLogModel model)
        {
            var cmdParameters = "@criticality, @message, @stacktrace";
            var cmd = new NpgsqlCommand();
            cmd.CommandText = $@"INSERT INTO {exceptionLogTable} ({insertExceptionLogColumns}) VALUES ({cmdParameters});";
            cmd.Parameters.AddWithValue("@criticality", model.Criticality);
            cmd.Parameters.AddWithValue("@message", model.Message);
            cmd.Parameters.AddWithValue("@stacktrace", model.StackTrace);

            await db.CommandNonqueryAsync(cmd);
        }
    }
}
