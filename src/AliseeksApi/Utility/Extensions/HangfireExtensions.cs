using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace AliseeksApi.Utility.Extensions
{
    public static class HangfireExtensions
    {
        public static void PurgeJobs(this IMonitoringApi monitor)
        {
            var toDelete = new List<string>();

            foreach(var quene in monitor.Queues())
            {
                for(var i = 0; i < Math.Ceiling(quene.Length / 1000d); i++)
                {
                    monitor.EnqueuedJobs(quene.Name, 1000 * i, 1000)
                        .ForEach(x => toDelete.Add(x.Key));
                }
            }

            foreach (var jobID in toDelete)
            {
                BackgroundJob.Delete(jobID);
            }
        }
    }
}
