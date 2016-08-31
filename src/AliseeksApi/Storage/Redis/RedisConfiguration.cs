using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace AliseeksApi.Storage.Redis
{
    public class RedisConfiguration
    {
        ConnectionMultiplexer redis;
        string configuration;

        public RedisConfiguration()
        {

        }

        public RedisConfiguration Configure(string config)
        {
            configuration = config;
            return this;
        }

        public ConnectionMultiplexer Connect()
        {
            return ConnectionMultiplexer.Connect(configuration);
        }
    }
}
