using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AliseeksApi.Services;
using AliseeksApi.Utility;
using AliseeksApi.Middleware;
using AliseeksApi.Scheduling;
using AliseeksApi.Storage.Redis;
using StackExchange.Redis;
using AliseeksApi.Storage.Cache;

namespace AliseeksApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.AddMemoryCache();

            configureDependencyInjection(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug(LogLevel.Warning);

            app.ApplyApiLogging();

            app.UseMvc();
        }

        void configureDependencyInjection(IServiceCollection services)
        {
            services.AddSingleton<IConnectionMultiplexer>(new RedisConfiguration()
                                                                    .Configure("127.0.0.1:6379")
                                                                    .Connect());
            services.AddScoped<IDatabase>(x => x.GetService<IConnectionMultiplexer>().GetDatabase());

            services.AddTransient<IApplicationCache, ApplicationCache>();
            services.AddTransient<IAliexpressService, AliexpressService>();
            services.AddTransient<IHttpService, HttpService>();
            services.AddTransient<IScheduler, Scheduler>();
        }
    }
}
