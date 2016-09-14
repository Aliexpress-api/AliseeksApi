using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using Microsoft.Extensions.Options;
using AliseeksApi.Configuration;
using System.Net;
using System.Text.RegularExpressions;

namespace AliseeksApi.Storage.Redis
{
    public class RedisCache
    {
        RedisOptions config;

        public RedisCache(IOptions<RedisOptions> config)
        {
            this.config = config.Value;
        }

        public ConnectionMultiplexer Connect()
        {
            var connectionString = $"{config.Host}:{config.Port},password={config.Password}";
            ConfigurationOptions configOptions = ConfigurationOptions.Parse(connectionString);
            DnsEndPoint addressEndpoint = configOptions.EndPoints.First() as DnsEndPoint;

            if(addressEndpoint != null)
            {
                int port = addressEndpoint.Port;
                bool isIp = isIpAddress(addressEndpoint.Host);
                if (!isIp)
                {
                    IPHostEntry ip = Dns.GetHostEntryAsync(addressEndpoint.Host).Result;
                    configOptions.EndPoints.Remove(addressEndpoint);
                    configOptions.EndPoints.Add(ip.AddressList.First(), port);
                }
            }

            return ConnectionMultiplexer.Connect(configOptions);
        }

        bool isIpAddress(string host)
        {
            string ipPattern = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
            return Regex.IsMatch(host, ipPattern);
        }
    }
}
