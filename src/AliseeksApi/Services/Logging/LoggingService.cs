using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Logging;
using AliseeksApi.Storage.Postgres.Logging;

namespace AliseeksApi.Services.Logging
{
    public class LoggingService : ILoggingService
    {
        ILoggingPostgres db;

        public LoggingService(ILoggingPostgres db)
        {
            this.db = db;
        }

        public async Task LogActivity(ActivityLogModel model)
        {
            await db.AddActivityLogAsync(model);
        }

        public async Task LogException(ExceptionLogModel model)
        {
            await db.AddExceptionLogAsync(model);
        }
    }
}
