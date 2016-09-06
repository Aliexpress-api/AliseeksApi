using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AliseeksApi.Models;
using AliseeksApi.Services.Email;
using AliseeksApi.Scheduling;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AliseeksApi.Models.Feedback;

namespace AliseeksApi.Controllers
{
    [Route("api/[controller]")]
    public class FeedbackController : Controller
    {
        IEmailService email;
        ILogger logger;

        public FeedbackController(IEmailService email, ILogger<SearchController> logger)
        {
            this.email = email;
            this.logger = logger;
        }

        //TODO: Error handling
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]FeedbackModel model)
        {
            string body = $@"
            Response Email: {model.Email}
            Feedback: {model.Message}
            ";

            string subject = "Aliseeks Feedback Received";

            await email.SendMail(body, subject);

            return Ok();
        }
    }
}
