using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Models.Dropshipping;
using AliseeksApi.Storage.Postgres.ORM;

namespace AliseeksApi.Storage.Postgres.Dropshipping
{
    public class DropshipAccountsPostgres : TableContext<DropshipAccount>
    {
        public DropshipAccountsPostgres(IPostgresDb db) : base(db)
        {
        }
    }
}
