using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Storage.Postgres.ORM;
using AliseeksApi.Models.OAuth;

namespace AliseeksApi.Storage.Postgres.OAuth
{
    public class OAuthPostgres : TableContext<OAuthAccountModel>
    {
        public OAuthPostgres(IPostgresDb db) : base(db)
        {

        }
    }
}
