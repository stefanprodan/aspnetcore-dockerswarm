using RethinkDb.Driver;
using RethinkDbLogProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Microsoft.AspNetCore.SignalR;

namespace LogWatcher
{
    public class LogChangeHandler
    {
        private readonly IRethinkDbConnectionFactory _rethinkDbFactory;
        private static RethinkDB R = RethinkDB.R;
        private readonly IConnectionManager _signalManager;

        public LogChangeHandler(IRethinkDbConnectionFactory rethinkDbFactory, IConnectionManager signalManager)
        {
            _rethinkDbFactory = rethinkDbFactory;
            _signalManager = signalManager;
        }

        public void HandleUpdates()
        {
            var hubContext = _signalManager.GetHubContext<LogHub>();

            var conn = _rethinkDbFactory.CreateConnection();
            var feed = R.Db(_rethinkDbFactory.GetOptions().Database).Table("Logs").Changes().RunChanges<LogEntry>(conn);

            foreach (var log in feed)
            {
                //hubContext.Clients.All.OnLog(
                //    log.NewValue.Timestamp.ToString(),
                //    log.NewValue.Level,
                //    log.NewValue.Host,
                //    log.NewValue.Application,
                //    log.NewValue.Category,
                //    log.NewValue.Message,
                //    log.NewValue.EventId);

                hubContext.Clients.All.OnLog(log.NewValue);
            }
        }
    }
}
