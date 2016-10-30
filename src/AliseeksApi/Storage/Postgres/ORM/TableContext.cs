using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AliseeksApi.Storage.Postgres;
using Npgsql;
using System.Reflection;
using Newtonsoft.Json;
using System.Data;
using System.Data.Common;
using AliseeksApi.Utility.Extensions;
using AliseeksApi.Utility;

namespace AliseeksApi.Storage.Postgres.ORM
{
    public class TableContext<T> : ITableContext<T>
    {
        protected string tableName
        {
            get
            {
                return ORMQueryHelper.GetTableName<T>();
            }
        }
        protected readonly IPostgresDb db;

        public TableContext(IPostgresDb db)
        {
            this.db = db;
        }

        public async Task<T> GetOne(T model)
        {
            var command = new NpgsqlCommand();

            T retrieved = default(T);

            command.CommandText = $"SELECT {ORMQueryHelper.GetSelectColumns<T>()} FROM {tableName} WHERE {String.Join("AND", WhereCriteria(model, command, DataColumnUsage.Retrieve, null))}";
            await db.CommandReaderAsync(command, (reader) =>
            {
                while(reader.Read())
                {
                    LoadModel(reader, retrieved);
                }
            });

            return retrieved;
        }

        public async Task<T[]> GetMultiple(T model)
        {
            var command = new NpgsqlCommand();

            var retrieved = new List<T>();

            command.CommandText = $"SELECT {ORMQueryHelper.GetSelectColumns<T>()} FROM {tableName} WHERE {String.Join("AND", WhereCriteria(model, command, DataColumnUsage.Retrieve, null))}";
            await db.CommandReaderAsync(command, (reader) =>
            {
                    T row = default(T);
                    LoadModel(reader, row);
                    retrieved.Add(row);
            });

            return retrieved.ToArray();
        }

        public async Task Save(T model)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"INSERT INTO {tableName} ({String.Join(",",InsertColumns(null))}) VALUES ({String.Join(",",InsertValues(model))});";

            await db.CommandNonqueryAsync(command);
        }

        public async Task Delete(T model)
        {
            var command = new NpgsqlCommand();
            command.CommandText = $"DELETE FROM {tableName} WHERE {DeleteCriteria(model, command)};";

            await db.CommandNonqueryAsync(command);
        }

        internal void LoadModel(DbDataReader reader, T model)
        {
            model = reader.ReadModel<T>(model);
        }

        internal string[] InsertColumns(string schema = null)
        {
            var properties = typeof(T).GetProperties();
            var columns = new List<string>();

            foreach (var prop in properties)
            {
                var attribute = prop.GetCustomAttribute<DataColumn>();
                if (attribute != null && !attribute.Usages.Contains(DataColumnUsage.Create))
                {
                    columns.Add(attribute.Name);
                }
            }

            return columns.ToArray();
        }

        internal string[] InsertValues(T model, string schema = null)
        {
            var properties = typeof(T).GetProperties();
            var columns = new List<string>();

            foreach (var prop in properties)
            {
                var attribute = prop.GetCustomAttribute<DataColumn>();
                if (attribute != null && !attribute.Usages.Contains(DataColumnUsage.Create))
                {
                    columns.Add($"@{attribute.Name}");
                }
            }

            return columns.ToArray();
        }

        internal string[] DeleteCriteria(T model, NpgsqlCommand command, string schema = null)
        {
            var criteria = new List<string>();
            var props = typeof(T).GetProperties();
            foreach (var prop in props)
            {
                var attribute = prop.GetCustomAttribute<DataColumn>();
                if (attribute != null && attribute.Usages.Contains(DataColumnUsage.Delete))
                {
                    criteria.Add($"{attribute.Name}=@{attribute.Name}");
                    switch (attribute.DbType)
                    {
                        case NpgsqlTypes.NpgsqlDbType.Jsonb:
                            command.Parameters.AddWithValue($"@{attribute.Name}", JsonConvert.SerializeObject(model));
                            break;

                        default:
                            command.Parameters.AddWithValue($"@{attribute.Name}", prop.GetValue(model));
                            break;
                    }
                }
            }

            return criteria.ToArray();
        }

        internal string[] UpdateSet(T model, NpgsqlCommand command, string schema = null)
        {
            var criteria = new List<string>();
            var props = typeof(T).GetProperties();
            foreach (var prop in props)
            {
                var attribute = prop.GetCustomAttribute<DataColumn>();
                if (attribute != null && !attribute.Usages.Contains(DataColumnUsage.Update))
                {
                    criteria.Add($"{attribute.Name}=@{attribute.Name}");
                    switch (attribute.DbType)
                    {
                        case NpgsqlTypes.NpgsqlDbType.Jsonb:
                            command.Parameters.AddWithValue($"@{attribute.Name}", NpgsqlTypes.NpgsqlDbType.Jsonb, JsonConvert.SerializeObject(prop.GetValue(model)));
                            break;

                        default:
                            command.Parameters.AddWithValue($"@{attribute.Name}", prop.GetValue(model));
                            break;
                    }
                }
            }

            return criteria.ToArray();
        }

        internal string[] WhereCriteria(T Model, NpgsqlCommand command, DataColumnUsage usage, string schema)
        {
            var criteria = new List<string>();
            var props = typeof(T).GetProperties();
            foreach(var prop in props)
            {
                var attribute = prop.GetCustomAttribute<DataColumn>();
                if(attribute != null && attribute.Usages.Contains(usage))
                {
                    var val = prop.GetValue(Model);

                    criteria.Add($"{attribute.Name}=@{attribute.Name}");
                    switch(attribute.DbType)
                    {
                        case NpgsqlTypes.NpgsqlDbType.Jsonb:
                            command.Parameters.AddWithValue($"@{attribute.Name}", JsonConvert.SerializeObject(val));
                            break;

                        default:
                            command.Parameters.AddWithValue($"@{attribute.Name}", val);
                            break;
                    }
                }
            }

            return criteria.ToArray();
        }
    }
}
