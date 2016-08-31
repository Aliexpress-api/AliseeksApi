using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AliseeksApi.Storage.Cache
{
    public interface IApplicationCache
    {
        Task<bool> StoreString(string key, string json);
        Task<string> GetString(string key);
        Task<bool> Exists(string key);
    }
}
