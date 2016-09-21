using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AliseeksApi.Services.Logging;
using AliseeksApi.Models.Logging;
using SharpRaven.Core;
using SharpRaven.Core.Data;
using AliseeksApi.Utility.Extensions;

namespace AliseeksApi.Middleware
{
    public class ExceptionHandler
    {
        RequestDelegate _next;
        ILogger<ExceptionHandler> logger;
        ILoggingService dbLog;
        IRavenClient raven;

        public ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger, ILoggingService dbLog,
            IRavenClient raven)
        {
            _next = next;
            this.logger = logger;
            this.dbLog = dbLog;
            this.raven = raven;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch(Exception ex)
            {
                logger.LogError(0, ex, "An unhandled exception has occured: " + ex.Message);

                //If response has not been started, send back a 500
                if(!context.Response.HasStarted)
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("An unhandled exception in the API has occured.");
                }

                //Log the error into our DB
                ExceptionLogModel model = new ExceptionLogModel()
                {
                    Criticality = 10,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                };

                var sentry = new SentryEvent(ex)
                {
                    Extra = new { Request = context.Request.Path.ToString(), QueryString = context.Request.QueryString.ToUriComponent() }
                };

                try
                {
                    await raven.CaptureNetCoreEventAsync(sentry);
                }
                catch(Exception e)
                {
                    logger.LogError(0, e, "Error sending RavenClient error");
                }

                try
                {
                    await dbLog.LogException(model);
                }
                catch(Exception e)
                {
                    logger.LogError(0, e, "Error logging exception: " + e.Message);
                }
            }
        }
    }
}
