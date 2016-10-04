using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Storage.Postgres.ORM;
using AliseeksApi.Utility.Attributes;


namespace AliseeksApi.Models.Dropshipping
{
    public enum AccountStatus
    {
        New, Existing
    }

    [TableName("dropshipaccounts")]
    public class DropshipAccount
    {
        [DataColumn("id", Usages = new DataColumnUsage[] {  DataColumnUsage.Create,
        DataColumnUsage.Retrieve,
        DataColumnUsage.Update,
        DataColumnUsage.Delete})]
        [RedisIgnore]
        public int ID { get; set; }

        [DataColumn("username")]
        public string Username { get; set; }

        [RedisIgnore]
        public AccountStatus Status { get; set; }

        [DataColumn("subscription")]
        [RedisIgnore]
        public string Subscription { get; set; }

        [DataColumn("account", DbType = NpgsqlTypes.NpgsqlDbType.Jsonb)]
        [RedisIgnore]
        public DropshipAccountRules Account { get; set; }

        public DropshipAccount()
        {
            Status = AccountStatus.Existing;
            Account = new DropshipAccountRules();
        }
    }

    public class DropshipAccountRules
    {
        public DropshipListingRules Default { get; set; }

        public DropshipAccountRules()
        {
            Default = DropshipListingRules.Default;
        }
    }
}
