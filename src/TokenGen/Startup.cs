using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RethinkDb.Driver;
using Microsoft.Extensions.PlatformAbstractions;

namespace TokenGen
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
            services.AddOptions();
            services.AddMemoryCache();

            // Add framework services.
            services.AddMvc();

            // RethinkDb connection is thread safe
            services.AddSingleton(new RethinkDbStore());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, RethinkDbStore store)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // enable RethinkDb logging 
            loggerFactory.EnableRethinkDbLogging();

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
