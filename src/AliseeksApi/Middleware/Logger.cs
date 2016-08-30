using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

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
        public Logger(RequestDelegate next, ILogger<Logger> logger)
        {
            this._next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var sw = new Stopwatch();
            sw.Start();
            await _next.Invoke(context);
            sw.Stop();
            logger.LogInformation($"{context.Request.Path}\t{sw.Elapsed.TotalMilliseconds}(ms)");
        }
    }
}
