using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Feedback;
using Npgsql;
using SharpRaven.Core;
using AliseeksApi.Utility.Extensions;

namespace AliseeksApi.Storage.Postgres.Feedback
{
    public class FeedbackPostgres : IFeedbackPostgres
    {
        const string feedbackTable = "feedback";
        const string feedbackColumns = "message, email";

        private readonly IPostgresDb db;
        private readonly IRavenClient raven;

        public FeedbackPostgres(IPostgresDb db, IRavenClient raven)
        {
            this.db = db;
            this.raven = raven;
        }

        public async Task InsertFeedback(FeedbackModel model)
        {
            try
            {
                var command = new NpgsqlCommand();
                var commandParameters = "@message, @email";
                command.CommandText = $"INSERT INTO {feedbackTable} ({feedbackColumns}) VALUES ({commandParameters});";
                command.Parameters.AddWithValue("@message", model.Message);
                command.Parameters.AddWithValue("@email", model.Email);

                await db.CommandNonqueryAsync(command);
            }
            catch(Exception e)
            {
                await raven.CaptureNetCoreEventAsync(e);
            }
        }
    }   
}
