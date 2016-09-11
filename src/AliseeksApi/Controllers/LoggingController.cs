using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AliseeksApi.Models;
using AliseeksApi.Services.Email;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AliseeksApi.Models.Feedback;
using AliseeksApi.Services.Logging;
using AliseeksApi.Models.Logging;


namespace AliseeksApi.Controllers
{
    [Route("api/[controller]/")]
    public class LoggingController : Controller
    {
        IEmailService email;
        ILoggingService errors;
        ILogger<LoggingController> logger;

        public LoggingController(IEmailService email, ILoggingService errors,
            ILogger<LoggingController> logger)
        {
            this.email = email;
            this.errors = errors;
            this.logger = logger;
        }

        [HttpPost("Exception")]
        public async Task<IActionResult> Exception([FromBody]ExceptionLogModel model)
        {
            logger.LogCritical(new EventId(501), "Application Exception Recieved");
            logger.LogCritical(new EventId(501), $"Error Message: {model.Message}");
            logger.LogCritical(new EventId(501), $"Stack Trace: {model.StackTrace}");

            try
            { await errors.LogException(model); }
            catch (Exception e)
            { logger.LogError(new EventId(500), e, "Unable to log exception to Postgres"); }

            return Ok();
        }

        [HttpPost("Activity")]
        public async Task<IActionResult> Activity([FromBody]ActivityLogModel model)
        {
            await errors.LogActivity(model);

            return Ok();
        }

    }
}
