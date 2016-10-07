using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Storage.Postgres.ORM;
using AliseeksApi.Models.Search;

namespace AliseeksApi.Models.Dropshipping
{
    [TableName("dropshipitems")]
    public class DropshipItemModel
    {
        [DataColumn("id", Usages = new DataColumnUsage[] { DataColumnUsage.Create, DataColumnUsage.Retrieve, DataColumnUsage.Update, DataColumnUsage.Delete })]
        public int ID { get; set; }

        [DataColumn("username")]
        public string Username { get; set; }

        [DataColumn("source", DbType = NpgsqlTypes.NpgsqlDbType.Jsonb)]
        public SingleItemRequest Source { get; set; }

        [DataColumn("listingid")]
        public string ListingID { get; set; }

        [DataColumn("listing")]
        public string Listing { get; set; }

        [DataColumn("rules", DbType = NpgsqlTypes.NpgsqlDbType.Jsonb)]
        public DropshipListingRules Rules { get; set; }
    }
}
