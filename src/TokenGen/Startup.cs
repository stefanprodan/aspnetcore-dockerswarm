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

            services.AddMvc();

            // add RethinkDB logger service
            if (_env.IsDevelopment())
            {
                services.Configure<RethinkDbOptions>(Configuration.GetSection("RethinkDbDev"));
            }
            else
            {
                services.Configure<RethinkDbOptions>(Configuration.GetSection("RethinkDbStaging"));
            }
            services.AddSingleton<IRethinkDbConnectionFactory, RethinkDbConnectionFactory>();
            services.AddSingleton<IRethinkDbLoggerService, RethinkDbLoggerService>();

            // add RethinkDB store service  
            services.AddSingleton<IRethinkDbStore, RethinkDbStore>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, 
            IRethinkDbLoggerService rethinkDbLoggerService, 
            IRethinkDbStore store)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // create log database, tables and indexes if not exists
            rethinkDbLoggerService.ApplySchema();

            // enable RethinkDb logging 
            loggerFactory.AddRethinkDb(rethinkDbLoggerService, LogLevel.Information);

            app.UseMvc();

            // create TokenStore database, tables and indexes if not exists
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
