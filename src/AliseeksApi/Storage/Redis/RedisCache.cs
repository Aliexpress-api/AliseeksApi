using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using Microsoft.Extensions.Options;
using AliseeksApi.Configuration;

namespace AliseeksApi.Storage.Redis
{
    public class RedisCache
    {
        ConnectionMultiplexer redis;
        RedisOptions config;

        public RedisCache(IOptions<RedisOptions> config)
        {
            this.config = config.Value;
        }

        public ConnectionMultiplexer Connect()
        {
            return ConnectionMultiplexer.Connect($"{config.Host}:{config.Port}");
        }
    }
}
