using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Logging;

namespace AliseeksApi.Storage.Postgres.Logging
{
    public interface ILoggingPostgres
    {
        Task AddExceptionLogAsync(ExceptionLogModel model);
        Task AddActivityLogAsync(ActivityLogModel model);
    }
}
