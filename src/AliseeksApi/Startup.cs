#region Usings

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AliseeksApi.Services;
using AliseeksApi.Services.Email;
using AliseeksApi.Middleware;
using AliseeksApi.Storage.Redis;
using StackExchange.Redis;
using AliseeksApi.Storage.Cache;
using AliseeksApi.Configuration;
using AliseeksApi.Services.User;
using AliseeksApi.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using AliseeksApi.Storage.Postgres;
using AliseeksApi.Storage.Postgres.Users;
using AliseeksApi.Storage.Postgres.Search;
using AliseeksApi.Storage.Postgres.Feedback;
using AliseeksApi.Utility.Security;
using AliseeksApi.Services.Logging;
using AliseeksApi.Storage.Postgres.Logging;
using AliseeksApi.Services.Search;
using AliseeksApi.Services.Dropshipping;
using AliseeksApi.Services.Dropshipping.Shopify;
using AliseeksApi.Storage.Postgres.Dropshipping;
using AliseeksApi.Storage.Postgres.OAuth;
using Microsoft.AspNetCore.Http;
using SharpRaven.Core.Configuration;
using SharpRaven.Core;
using AliseeksApi.Services.DHGate;
using Hangfire;
using Hangfire.PostgreSql;

#endregion

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
                .AddJsonFile("appsecrets.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables("ALISEEKS_")
                .AddEnvironmentVariables("ALISEEKSAPI_");

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.AddMemoryCache();

            services.AddOptions();
            services.Configure<EmailOptions>(Configuration.GetSection("EmailOptions"));
            services.Configure<JwtOptions>(Configuration.GetSection("JwtOptions"));
            services.Configure<RedisOptions>(Configuration.GetSection("RedisOptions"));
            services.Configure<PostgresOptions>(Configuration.GetSection("PostgresOptions"));
            services.Configure<RavenOptions>(Configuration.GetSection("RavenOptions"));
            services.Configure<ShopifyOptions>(Configuration.GetSection("ShopifyOptions"));

            configureDependencyInjection(services);

            //Add Hangfire with PostgresDb
            var postgresConfig = services.BuildServiceProvider().GetRequiredService<IOptions<PostgresOptions>>().Value;
            services.AddHangfire(x => { x.UseStorage(new PostgreSqlStorage($"Host={postgresConfig.Host};Port={postgresConfig.Port};Username={postgresConfig.Username};Password={postgresConfig.Password};Database={postgresConfig.Database}")); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug(LogLevel.Warning);

            app.UseMiddleware<ExceptionHandler>();

            app.ApplyApiLogging();

            //Configure Hangfire
            app.UseHangfireServer();
            app.UseHangfireDashboard();

            var jwtOptions = app.ApplicationServices.GetService<IOptions<JwtOptions>>().Value;
            app.UseJwtBearerAuthentication(new JwtBearerOptions()
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme,
                TokenValidationParameters = AliseeksJwtAuthentication.TokenValidationParameters(jwtOptions.SecretKey)
            });

            app.UseMvc();

            Jobs.JobScheduler.ScheduleJobs();
        }

        void configureDependencyInjection(IServiceCollection services)
        {
            //Intermiediate service provider with IOptions
            var serviceProvider = services.BuildServiceProvider();

            //Add HTTPContextAccessor as Singleton
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //Configure Redis Cache
            services.AddSingleton<IConnectionMultiplexer>((provider) =>
            {
                var config = provider.GetRequiredService<IOptions<RedisOptions>>();
                return new RedisCache(config).Connect();
            });
            services.AddScoped<IDatabase>(x => x.GetService<IConnectionMultiplexer>().GetDatabase());

            //Configure Postgres
            services.AddTransient<IPostgresDb, PostgresDb>();
            services.AddTransient<IUsersPostgres, UsersPostgres>();
            services.AddTransient<ISearchPostgres, SearchPostgres>();
            services.AddTransient<ILoggingPostgres, LoggingPostgres>();
            services.AddTransient<IFeedbackPostgres, FeedbackPostgres>();
            services.AddTransient<DropshipItemsPostgres>();
            services.AddTransient<OAuthPostgres>();
            services.AddTransient<DropshipAccountsPostgres>();

            //Configure RavenClient
            services.AddScoped<IRavenClient, RavenClient>((s) => {

                var rc = new RavenClient(s.GetRequiredService<IOptions<RavenOptions>>(), s.GetRequiredService<IHttpContextAccessor>())
                {
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                };
                return rc;
            });

            services.AddTransient<IApplicationCache, ApplicationCache>();
            services.AddTransient<AliexpressService, AliexpressService>();
            services.AddTransient<DHGateService, DHGateService>();
            services.AddTransient<ISearchService, SearchService>();
            services.AddTransient<IHttpService, HttpService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IJwtFactory, AliseeksJwtAuthentication>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ILoggingService, LoggingService>();
            services.AddTransient<DropshippingService>();
            services.AddTransient<ShopifyService>();

            services.AddTransient<WebSearchService[]>((provider) =>
            {
                var webServices = new List<WebSearchService>();
                webServices.Add(provider.GetRequiredService<AliexpressService>());

                return webServices.ToArray();
            });

            services.AddTransient<SearchCache, SearchCache>();

            //Add Utilities
            services.AddTransient<ISecurityHasher, SecurityHasher>();

            //Add Jobs
            services.AddTransient<Jobs.PriceHistoryJob>();
        }
    }
}
