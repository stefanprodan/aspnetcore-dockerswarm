using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RethinkDb.Driver;
using Microsoft.Extensions.PlatformAbstractions;
using RethinkDbLogProvider;

namespace TokenGen
{
    public class Startup
    {
        private IHostingEnvironment _env;

        public Startup(IHostingEnvironment env)
        {
            _env = env;

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
            services.AddOptions();
            services.AddMemoryCache();

            // Add framework services.
            services.AddMvc();

            // RethinkDb connection is thread safe
            services.AddSingleton(new RethinkDbStore());

            services.Configure<RethinkDbOptions>(Configuration.GetSection("RethinkDbLogging"));
            services.AddSingleton<IRethinkDbConnectionFactory, RethinkDbConnectionFactory>();
            services.AddSingleton<IRethinkDbLoggerService, RethinkDbLoggerService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, RethinkDbStore store, IRethinkDbLoggerService rethinkDbLoggerService)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            rethinkDbLoggerService.ApplySchema();

            loggerFactory.AddRethinkDb(rethinkDbLoggerService, LogLevel.Information);

            // enable RethinkDb logging 
            //loggerFactory.EnableRethinkDbLogging();

            app.UseMvc();

            // connect to RethinkDb cluster
            var rethinkDbCluster = env.IsDevelopment() ? Configuration["RethinkDbDev"] : Configuration["RethinkDbStaging"];
            store.Connect(rethinkDbCluster, Configuration["RethinkDbName"]);

            // create database, tables and indexes if not exists
            store.ApplySchema();

            // register issuer
            store.InsertOrUpdateIssuer(new Issuer
            {
                Name = Environment.MachineName,
                Version = PlatformServices.Default.Application.ApplicationVersion,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
