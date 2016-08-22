using RethinkDb.Driver;
using RethinkDbLogProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RethinkDb.Driver.Net;

namespace LogWatcher
{
    public class LogChangeHandler
    {
        private static RethinkDB R = RethinkDB.R;
        private readonly Microsoft.AspNetCore.SignalR.Infrastructure.IConnectionManager _signalManager;
        private readonly IRethinkDbConnectionFactory _rethinkDbFactory;
        private readonly ILogger<LogChangeHandler> _logger;
        private RethinkDbOptions _options;
        private Connection _conn;
        private DateTime _lastLogTimestamp = DateTime.UtcNow.AddSeconds(-1);
        private int _retyCount = 0;

        public LogChangeHandler(IRethinkDbConnectionFactory rethinkDbFactory, 
            IOptions<RethinkDbOptions> options,
            ILogger<LogChangeHandler> logger,
            Microsoft.AspNetCore.SignalR.Infrastructure.IConnectionManager signalManager)
        {
            _rethinkDbFactory = rethinkDbFactory;
            _options = options.Value;
            _signalManager = signalManager;
            _logger = logger;

            //_conn = R.Connection()
            //    .Hostname(_options.Host)
            //    .Port(_options.Port)
            //    .Timeout(_options.Timeout)
            //    .Connect();

            _logger.LogDebug(900, $"Changefeed watcher started.");
        }

        public void Start()
        {
            HandleUpdates();

            _logger.LogCritical($"Changefeed exited, connection is open {_conn.Open}");
        }

        public void HandleUpdates()
        {
            try
            {
                _logger.LogDebug(901, $"Changefeed HandleUpdates started. Retry count {_retyCount}");

                _conn = _rethinkDbFactory.CreateConnection();
                RunChangefeed();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(1001, ex, $"Changefeed error {ex.Message}. Connection open {_conn.Open}.");

                _conn.Close();
                _conn.Reconnect();

                _retyCount++;

                //TODO: retry limit
                HandleUpdates();
            }
        }

        private void RunChangefeed()
        {
            var hubContext = _signalManager.GetHubContext<LogHub>();
            var feed = R.Db(_options.Database).Table("Logs")
                .Between(_lastLogTimestamp, R.Maxval())[new { index = nameof(LogEntry.Timestamp) }]
                .Changes().RunChanges<LogEntry>(_conn);

            foreach (var log in feed)
            {
                _logger.LogDebug(902, $"SignalR push event {log.NewValue.Id}");

                // push new value to SignalR hub
                hubContext.Clients.All.OnLog(log.NewValue);

                // start point on reconnect
                _lastLogTimestamp = log.NewValue.Timestamp;
            }
        }
    }
}
