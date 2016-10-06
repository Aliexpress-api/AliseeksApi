using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpRaven.Core;
using SharpRaven.Core.Data;

namespace AliseeksApi.Utility.Extensions
{
    public static class RavenClientExtensions
    {
        public static async Task<string> CaptureNetCoreEventAsync(this IRavenClient client, SentryEvent @event)
        {
            //Add "beautified" async recovery data
            @event.Exception.Data.Add("AsyncStackTrace", @event.Exception.StackTraceEx());

            return await CaptureNetCoreEvent(client, @event);
        }

        public static async Task<string> CaptureNetCoreEventAsync(this IRavenClient client, Exception e)
        {
            SentryEvent @event = new SentryEvent(e);

            return await CaptureNetCoreEventAsync(client, @event);
        }

        public static async Task<string> CaptureNetCoreEvent(this IRavenClient client, SentryEvent @event)
        {
            //Do not send errors during development
            if (client.Environment == "Development")
                return "development";

            return await client.CaptureAsync(@event);
        }

        public static async Task<string> CaptureNetCoreEvent(this IRavenClient client, Exception e)
        {
            //Do not send errors during development
            if (client.Environment == "Development")
                return "development";

            SentryEvent @event = new SentryEvent(e);

            return await client.CaptureAsync(@event);
        }
    }
}
