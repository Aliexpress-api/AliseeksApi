using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Storage.Postgres.ORM;
using Newtonsoft.Json;

namespace AliseeksApi.Models.OAuth
{
    [TableName("oauthaccounts")]
    public class OAuthAccountModel
    {
        [DataColumn("id", Usages = new DataColumnUsage[] { DataColumnUsage.Create, DataColumnUsage.Retrieve, DataColumnUsage.Update, DataColumnUsage.Delete })]
        public int ID { get; set; }

        [DataColumn("username")]
        public string Username { get; set; }

        [DataColumn("acocuntid")]
        public int AccountID { get; set; } //FK to DropshipAccount

        [DataColumn("access_token")]
        public string AccessToken { get; set; }

        [DataColumn("service")]
        public string Service { get; set; }

        [DataColumn("meta", DbType = NpgsqlTypes.NpgsqlDbType.Jsonb)]
        public object Meta { get; set; }

        [DataColumn("extra", DbType = NpgsqlTypes.NpgsqlDbType.Jsonb)]
        public Dictionary<string, string> Extra { get; set; } = new Dictionary<string, string>();
    }

    public class OAuthShopifyModel : OAuthAccountModel, IOAuthModel
    {
        [JsonIgnore]
        public string Shop
        {
            get
            {
                if (Extra.ContainsKey("Shop"))
                    return Extra["Shop"];

                throw new Exception("OAuth Model not a valid model");
            }
        }

        public string GetServiceName()
        {
            return "Shopify";
        }
    }

    public interface IOAuthModel
    {
        string GetServiceName();
    }
}
