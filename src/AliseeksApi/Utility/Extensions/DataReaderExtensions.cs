
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using System.Data.Common;
using Newtonsoft.Json;
using System.Reflection;
using AliseeksApi.Storage.Postgres.ORM;
using AliseeksApi.Utility;
using System.Data.SqlTypes;

namespace AliseeksApi.Utility.Extensions
{
    public static class DataReaderExtensions
    {
        public static T ReadModel<T>(this DbDataReader reader, T model)
        {
            var tableName = ORMQueryHelper.GetTableName<T>();
            var props = typeof(T).GetProperties();

            var fieldNames = new List<string>();
            for(int i = 0; i != reader.FieldCount; i++)
            {
                fieldNames.Add(reader.GetName(i));
            }

            foreach (var prop in props)
            {
                var attribute = prop.GetCustomAttribute<DataColumn>();
                if (attribute != null)
                {
                    var index = reader.GetOrdinal($"{tableName}{attribute.Name}");
                    switch (attribute.DbType)
                    {
                        case NpgsqlTypes.NpgsqlDbType.Jsonb:
                            try
                            {
                                if (reader.IsDBNull(index))
                                    prop.SetValue(model, HandleNulls(reader.GetFieldType(index)));
                                else
                                    prop.SetValue(model, JsonConvert.DeserializeObject((string)reader.GetValue(index), prop.PropertyType));
                            }
                            catch
                            {
                                prop.SetValue(model, HandleNulls(reader.GetFieldType(index)));
                            }
                            break;

                        default:
                            prop.SetValue(model, !reader.IsDBNull(index) ? reader.GetValue(index) : HandleNulls(reader.GetFieldType(index)));
                            break;
                    }
                }
            }

            return model;
        }

        public static object HandleNulls(Type t)
        {
            if (t == typeof(string))
                return String.Empty;

            if (t == typeof(DateTime))
                return (DateTime)SqlDateTime.MinValue;

            if (t == typeof(int))
                return 0;

            if (t == typeof(bool))
                return false;

            return null;
        }
    }
}
