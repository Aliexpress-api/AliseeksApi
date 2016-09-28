using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Search;
using AliseeksApi.Storage.Postgres.Search;

namespace AliseeksApi.Jobs
{
    public class PriceHistoryJob
    {
        private readonly ISearchPostgres db;

        public PriceHistoryJob(ISearchPostgres db)
        {
            this.db = db;
        }

        public void RunJob()
        {
            
        }
    }
}
