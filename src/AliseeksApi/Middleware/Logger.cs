using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using SharpRaven.Core;
using SharpRaven.Core.Data;
using AliseeksApi.Utility.Extensions;

namespace AliseeksApi.Middleware
{
    public static class LoggerExtension
    {
        public static IApplicationBuilder ApplyApiLogging(this IApplicationBuilder app)
        {
            app.UseMiddleware<Logger>();
            return app;
        }
    }

    public class Logger
    {
        private readonly RequestDelegate _next;
        private ILogger<Logger> logger;
        private IRavenClient raven;

        public Logger(RequestDelegate next, ILogger<Logger> logger, IRavenClient raven)
        {
            this._next = next;
            this.logger = logger;
            this.raven = raven;
        }

        public async Task Invoke(HttpContext context)
        {
            logger.LogInformation($"{context.Request.Path}\tRECEIVED");
            var sw = new Stopwatch();
            sw.Start();

            var crumb = new Breadcrumb("LoggerMiddleware");
            crumb.Message = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString.ToUriComponent()}";
            crumb.Data = new Dictionary<string, string>() {
                { "IsAuthenticated", context.User.Identity.IsAuthenticated.ToString() },
                { "Authentication", context.User.Identity.IsAuthenticated ? context.User.Identity.Name : "Unknown" }
            };
            raven.AddTrail(crumb);

            try
            {
                await _next.Invoke(context);
            }
            catch(Exception e)
            {
                //Log exception with RavenClient
                await raven.CaptureNetCoreEventAsync(e);
            }
   
            sw.Stop();
            logger.LogInformation($"{context.Request.Path}\t{sw.Elapsed.TotalMilliseconds}(ms)");
        }
    }
}
