using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Storage.Postgres.ORM
{
    public interface ITableContext<T>
    {
        Task Save(T model);
        Task<T[]> GetMultiple(T model);
        Task<T> GetOne(T model);
    }
}
