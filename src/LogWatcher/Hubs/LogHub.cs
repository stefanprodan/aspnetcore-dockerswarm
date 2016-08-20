using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using RethinkDbLogProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LogWatcher
{
    public class LogHub : Hub
    {
        private static RethinkDB R = RethinkDB.R;
        private readonly IRethinkDbConnectionFactory _rethinkDbFactory;

        public LogHub(IRethinkDbConnectionFactory rethinkDbFactory)
        {
            _rethinkDbFactory = rethinkDbFactory;
        }

        public dynamic Load(int limit)
        {
            var conn = _rethinkDbFactory.CreateConnection();
            var logs = R.Db(_rethinkDbFactory.GetOptions().Database)
                .Table("Logs")
                .OrderBy()[new { index = R.Asc(nameof(LogEntry.Timestamp)) }]
                .Limit(limit)
                .RunCursor<LogEntry>(conn);

            return logs;
        }

        public dynamic Search(string query, int limit)
        {
            var conn = _rethinkDbFactory.CreateConnection();
            var logs = R.Db(_rethinkDbFactory.GetOptions().Database)
                .Table("Logs")
                .OrderBy()[new { index = R.Asc(nameof(LogEntry.Timestamp)) }]
                .Filter(t => t.CoerceTo("string").Match($"(?i){query}"))
                .Limit(limit)
                .RunCursor<LogEntry>(conn);

            return logs;
        }
    }
}
