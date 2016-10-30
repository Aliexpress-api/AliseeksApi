using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

namespace AliseeksApi.Storage.Postgres.ORM
{
    public enum DataColumnUsage
    {
        Create, Retrieve, Update, Delete
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited =  false)]
    public class TableName : System.Attribute
    {
        public string Name { get; set; }

        public TableName(string tblName)
        {
            Name = tblName;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class DataColumn : System.Attribute
    {
        public string Name { get; set; }
        public DataColumnUsage[] Usages { get; set; }
        public NpgsqlTypes.NpgsqlDbType DbType { get; set; }

        public DataColumn(string name, DataColumnUsage[] usages = null, NpgsqlDbType dbType = NpgsqlDbType.Unknown)
        {
            Name = name;
            Usages = usages == null ? new DataColumnUsage[] { } : usages;
            DbType = dbType;
        }
    }
}
