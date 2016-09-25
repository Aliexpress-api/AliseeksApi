using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Feedback;
using Npgsql;

namespace AliseeksApi.Storage.Postgres.Feedback
{
    public class FeedbackPostgres : IFeedbackPostgres
    {
        const string feedbackTable = "feedback";
        const string feedbackColumns = "message, email";

        private readonly IPostgresDb db;

        public FeedbackPostgres(IPostgresDb db)
        {
            this.db = db;
        }

        public async Task InsertFeedback(FeedbackModel model)
        { 
            var command = new NpgsqlCommand();
            var commandParameters = "@message, @email";
            command.CommandText = $"INSERT INTO {feedbackTable} ({feedbackColumns}) VALUES ({commandParameters});";
            command.Parameters.AddWithValue("@message", model.Message);
            command.Parameters.AddWithValue("@email", model.Email);

            await db.CommandNonqueryAsync(command);
        }
    }   
}
