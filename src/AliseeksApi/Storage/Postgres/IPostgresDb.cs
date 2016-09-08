using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using System.Data.Common;

namespace AliseeksApi.Storage.Postgres
{
    public interface IPostgresDb
    {
        NpgsqlConnection Connect();
        Task CommandReaderAsync(NpgsqlCommand command, Action<DbDataReader> func);
        Task CommandNonqueryAsync(NpgsqlCommand command);
    }
}
