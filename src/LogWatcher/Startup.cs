using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RethinkDbLogProvider;
using RethinkDb.Driver;

namespace LogWatcher
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
            // Add framework services.
            services.AddMvc();

            services.AddSignalR(options =>
            {
                options.Hubs.EnableDetailedErrors = true;
            });

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

            // register keep alive and change feed services
            services.AddSingleton<RethinkDbKeepAlive>();
            services.AddSingleton<LogChangeHandler>();
            services.AddSingleton<BackgroundTaskManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
            ILoggerFactory loggerFactory, 
            IRethinkDbLoggerService rethinkDbLoggerService,
            BackgroundTaskManager backgroundTaskManager)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                // enable RethinkDb driver logging 
                loggerFactory.EnableRethinkDbLogging();
            }

            // enable RethinkDb log provider
            loggerFactory.AddRethinkDb(rethinkDbLoggerService, LogLevel.Warning);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // try to avoid connection being dropped due to inactivity
            backgroundTaskManager.StartKeepAlive();

            // run log watcher on a background thread
            backgroundTaskManager.StartLogChangeFeed();

            app.UseWebSockets();
            app.UseSignalR();
        }
    }
}
