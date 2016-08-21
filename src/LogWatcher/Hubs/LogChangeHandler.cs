using RethinkDb.Driver;
using RethinkDbLogProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace LogWatcher
{
    public class LogChangeHandler
    {
        private readonly IRethinkDbConnectionFactory _rethinkDbFactory;
        private static RethinkDB R = RethinkDB.R;
        private readonly IConnectionManager _signalManager;
        private readonly ILogger<LogChangeHandler> _logger;

        public LogChangeHandler(IRethinkDbConnectionFactory rethinkDbFactory, 
            IConnectionManager signalManager, 
            ILogger<LogChangeHandler> logger)
        {
            _rethinkDbFactory = rethinkDbFactory;
            _signalManager = signalManager;
            _logger = logger;
        }

        public void HandleUpdates()
        {
            var hubContext = _signalManager.GetHubContext<LogHub>();

            var conn = _rethinkDbFactory.CreateConnection();
            var feed = R.Db(_rethinkDbFactory.GetOptions().Database).Table("Logs").Changes().RunChanges<LogEntry>(conn);
            try
            {
                foreach (var log in feed)
                {
                    hubContext.Clients.All.OnLog(log.NewValue);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(1001, ex, $"Changefeed error {ex.Message}");

                //TODO: retry limit
                HandleUpdates();
            }


            _logger.LogCritical($"Changefeed exited, connection is open {conn.Open}");
        }
    }
}
