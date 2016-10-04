using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Storage.Postgres.ORM;

namespace AliseeksApi.Models.OAuth
{
    [TableName("oauthaccounts")]
    public class OAuthAccountModel
    {
        [DataColumn("id", Usages = new DataColumnUsage[] { DataColumnUsage.Create, DataColumnUsage.Retrieve, DataColumnUsage.Update, DataColumnUsage.Delete })]
        public int ID { get; set; }

        [DataColumn("username")]
        public string Username { get; set; }

        [DataColumn("access_token")]
        public string AccessToken { get; set; }

        [DataColumn("service")]
        public string Service { get; set; }
    }
}
