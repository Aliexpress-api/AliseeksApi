using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Hangfire;
using Hangfire.Server;
using Hangfire.Storage;
using AliseeksApi.Utility.Extensions;

namespace AliseeksApi.Jobs
{
    public static class JobScheduler
    {
        public static bool RunJobs = true;

        public static void ScheduleJobs()
        {
            //RemoveJobs();

            if(!RunJobs) { return; }

            //BackgroundJob.Enqueue<DropshippingJobs>(x => x.RunUpdateItems(0));

            BackgroundJob.Schedule<DropshippingJobs>(x => x.RunUpdateItems(0), TimeSpan.FromDays(1));

            //RecurringJob.AddOrUpdate<PriceHistoryJob>("pricehistory", x => x.RunJob(), Cron.MinuteInterval(5));
            //RecurringJob.Trigger("pricehistory");
        }

        public static void RemoveJobs()
        {
            var monitor = JobStorage.Current.GetMonitoringApi();
            monitor.PurgeJobs();
        }
    }
}
