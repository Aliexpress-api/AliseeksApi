using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Logging;
using AliseeksApi.Storage.Postgres.Logging;

namespace AliseeksApi.Services.Logging
{
    public class ExceptionLoggingService : IExceptionLoggingService
    {
        ILoggingPostgres db;

        public ExceptionLoggingService(ILoggingPostgres db)
        {
            this.db = db;
        }

        public async Task LogException(ExceptionLogModel model)
        {
            await db.AddExceptionLogAsync(model);
        }
    }
}
