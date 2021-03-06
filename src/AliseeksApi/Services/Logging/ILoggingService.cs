﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Logging;

namespace AliseeksApi.Services.Logging
{
    public interface ILoggingService
    {
        Task LogException(ExceptionLogModel model);
        Task LogActivity(ActivityLogModel model);
    }
}
