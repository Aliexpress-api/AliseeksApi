using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace AliseeksApi.Storage.Cache
{
    public class ApplicationCache
    {
        IDatabase persistentDb;

        public ApplicationCache(IDatabase db)
        {
            persistentDb = db;
        }

        public async Task<bool> Exists(string key)
        {
            return await persistentDb.KeyExistsAsync(key);
        }

        public async Task<string> GetString(string key)
        {
            return await persistentDb.StringGetAsync(key);
        }

        public async Task<bool> StoreString(string key, string json)
        {
            return await persistentDb.StringSetAsync(key, json, TimeSpan.FromHours(4), When.Always, CommandFlags.FireAndForget);
        }

        public async Task SendMessage(string message)
        {
            await persistentDb.PublishAsync("cache", "test message", CommandFlags.FireAndForget);
        }
    }
}
