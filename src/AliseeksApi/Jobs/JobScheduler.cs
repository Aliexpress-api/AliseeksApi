using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Hangfire;

namespace AliseeksApi.Jobs
{
    public static class JobScheduler
    {
        public static void ScheduleJobs()
        {
            RecurringJob.AddOrUpdate<PriceHistoryJob>("pricehistory", x => x.RunJob(), Cron.MinuteInterval(5));
            RecurringJob.Trigger("pricehistory");
        }
    }
}
