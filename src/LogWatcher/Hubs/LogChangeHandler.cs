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
        private static IRethinkDbConnectionFactory _connectionFactory;
        private static RethinkDB R = RethinkDB.R;
        private readonly IConnectionManager _connectionManager;

        public LogChangeHandler(IRethinkDbConnectionFactory connectionFactory, IConnectionManager connectionManager)
        {
            _connectionFactory = connectionFactory;
            _connectionManager = connectionManager;
        }

        public void HandleUpdates()
        {
            IHubContext context = _connectionManager.GetHubContext<LogHub>();
            IConnection connection = _connectionManager.GetConnectionContext<PersistentConnection>().Connection;

            var conn = _connectionFactory.CreateConnection();
            var feed = R.Db(_connectionFactory.GetOptions().Database).Table("Logs").Changes().RunChanges<LogEntry>(conn);

            foreach (var log in feed)
            {
                context.Clients.All.OnLog(
                    log.NewValue.Timestamp.ToString(),
                    log.NewValue.Level,
                    log.NewValue.Host,
                    log.NewValue.Application,
                    log.NewValue.Category,
                    log.NewValue.Message);
            }
        }
    }
}
