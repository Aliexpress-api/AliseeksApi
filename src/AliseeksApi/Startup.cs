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
using AliseeksApi.Services.Email;
using AliseeksApi.Utility;
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
using AliseeksApi.Utility.Security;
using AliseeksApi.Services.Logging;
using AliseeksApi.Storage.Postgres.Logging;

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

            services.AddOptions();
            services.Configure<EmailOptions>(Configuration.GetSection("EmailOptions"));
            services.Configure<JwtOptions>(Configuration.GetSection("JwtOptions"));
            services.Configure<RedisOptions>(Configuration.GetSection("RedisOptions"));
            services.Configure<PostgresOptions>(Configuration.GetSection("PostgresOptions"));

            configureDependencyInjection(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug(LogLevel.Warning);

            app.ApplyApiLogging();

            var jwtOptions = app.ApplicationServices.GetService<IOptions<JwtOptions>>().Value;
            app.UseJwtBearerAuthentication(new JwtBearerOptions()
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme,
                TokenValidationParameters = AliseeksJwtAuthentication.TokenValidationParameters(jwtOptions.SecretKey)
            });

            app.UseMvc();
        }

        void configureDependencyInjection(IServiceCollection services)
        {
            //Intermiediate service provider with IOptions
            var serviceProvider = services.BuildServiceProvider();

            //Configure Redis Cache
            var redisConfig = serviceProvider.GetService<IOptions<RedisOptions>>();
            services.AddSingleton<IConnectionMultiplexer>(new RedisCache(redisConfig).Connect());
            services.AddScoped<IDatabase>(x => x.GetService<IConnectionMultiplexer>().GetDatabase());

            //Configure Postgres
            services.AddTransient<IPostgresDb, PostgresDb>();
            services.AddTransient<IUsersPostgres, UsersPostgres>();
            services.AddTransient<ISearchPostgres, SearchPostgres>();
            services.AddTransient<ILoggingPostgres, LoggingPostgres>();

            services.AddTransient<IApplicationCache, ApplicationCache>();
            services.AddTransient<IAliexpressService, AliexpressService>();
            services.AddTransient<IHttpService, HttpService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IJwtFactory, AliseeksJwtAuthentication>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IExceptionLoggingService, ExceptionLoggingService>();

            //Add Utilities
            services.AddTransient<ISecurityHasher, SecurityHasher>();
        }
    }
}
