using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using AliseeksApi.Storage.Postgres.ORM;

namespace AliseeksApi.Utility
{
    public static class ORMQueryHelper
    {
        public static string GetSelectColumns<T>(string schema = null)
        {
            var tableName = GetTableName<T>();
            var properties = typeof(T).GetProperties();
            var columns = new List<string>();

            foreach(var prop in properties)
            {
                var attribute = prop.GetCustomAttribute<DataColumn>();
                if(attribute != null)
                {
                    columns.Add($"{tableName}.{attribute.Name} as {tableName}{attribute.Name}");
                }
            }

            return String.Join(",", columns);
        }

        public static string GetTableName<T>()
        {
            var attribute = typeof(T).GetTypeInfo().GetCustomAttribute<TableName>();
            if (attribute != null)
            {
                return attribute.Name;
            }

            return String.Empty;
        }
    }
}
