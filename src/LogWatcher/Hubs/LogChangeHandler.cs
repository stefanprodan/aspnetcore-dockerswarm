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
        private RethinkDbOptions _options;
        private Connection _conn;
        private DateTime _lastLogTimestamp = DateTime.UtcNow.AddSeconds(-1);
        private int _retyCount = 0;

        public LogChangeHandler(IRethinkDbConnectionFactory rethinkDbFactory, 
            IOptions<RethinkDbOptions> options,
            Microsoft.AspNetCore.SignalR.Infrastructure.IConnectionManager signalManager)
        {
            _rethinkDbFactory = rethinkDbFactory;
            _options = options.Value;
            _signalManager = signalManager;
        }

        public void HandleUpdates()
        {
            try
            {
                _conn = _rethinkDbFactory.CreateConnection();
                RunChangefeed();
            }
            catch (Exception)
            {
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
                // push new value to SignalR hub
                hubContext.Clients.All.OnLog(log.NewValue);

                // start point on reconnect
                _lastLogTimestamp = log.NewValue.Timestamp;
            }
        }
    }
}
