using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Feedback;

namespace AliseeksApi.Storage.Postgres.Feedback
{
    public interface IFeedbackPostgres
    {
        Task InsertFeedback(FeedbackModel model);
    }
}
