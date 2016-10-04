using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Storage.Postgres.ORM;
using AliseeksApi.Models.Dropshipping;

namespace AliseeksApi.Storage.Postgres.Dropshipping
{
    public class DropshipItemsPostgres : TableContext<DropshipItemModel>
    {
        public DropshipItemsPostgres(IPostgresDb db) : base(db)
        {
        }
    }
}
