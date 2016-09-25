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
using AliseeksApi.Storage.Postgres.Feedback;

namespace AliseeksApi.Controllers
{
    [Route("api/[controller]")]
    public class FeedbackController : Controller
    {
        //IEmailService email;
        private readonly IFeedbackPostgres db;
        ILogger logger;

        public FeedbackController(IFeedbackPostgres db, ILogger<SearchController> logger)
        {
            //this.email = email;
            this.db = db;
            this.logger = logger;
        }

        //TODO: Error handling
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]FeedbackModel model)
        {
            /*string body = $@"
            Response Email: {model.Email}
            Feedback: {model.Message}
            ";

            string subject = "Aliseeks Feedback Received";

            await email.SendMailTo(body, subject, "abello.2015@gmail.com");*/

            await db.InsertFeedback(model);

            return Ok();
        }
    }
}
