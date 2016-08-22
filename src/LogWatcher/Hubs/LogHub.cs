using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<LogHub> _logger;

        public LogHub(IRethinkDbConnectionFactory rethinkDbFactory,
            ILogger<LogHub> logger)
        {
            _rethinkDbFactory = rethinkDbFactory;
            _logger = logger;
        }

        public dynamic Load(int limit)
        {
            var conn = _rethinkDbFactory.CreateConnection();
            var logs = R.Db(_rethinkDbFactory.GetOptions().Database)
                .Table("Logs")
                .OrderBy()[new { index = R.Desc(nameof(LogEntry.Timestamp)) }]
                .Limit(limit)
                .RunCursor<LogEntry>(conn);

            var result = logs.ToList();
            logs.Close();

            return result;
        }

        public dynamic Search(string query, int limit)
        {
            var conn = _rethinkDbFactory.CreateConnection();

            try
            {
                return DoSearch(query, limit, conn);
            }
            catch (Exception ex)
            {
                _logger.LogError(1001, ex, $"DoSearch error {ex.Message}. Connection open {conn.Open}.");

                conn.Close();
                conn.Reconnect();

                return DoSearch(query, limit, conn);
            }
        }

        private List<LogEntry> DoSearch(string query, int limit, RethinkDb.Driver.Net.Connection conn)
        {
            var date = DateTime.UtcNow.AddDays(-1);
            var logs = R.Db(_rethinkDbFactory.GetOptions().Database)
                .Table("Logs")
                .OrderBy()[new { index = R.Desc(nameof(LogEntry.Timestamp)) }]
                .Filter(t => t.CoerceTo("string").Match($"(?i){query}"))
                .Limit(limit)
                .RunCursor<LogEntry>(conn);

            var result = logs.ToList();
            logs.Close();

            return result;
        }

        public dynamic HostLogStats()
        {
            var conn = _rethinkDbFactory.CreateConnection();
            var result = R.Db(_rethinkDbFactory.GetOptions().Database)
                .Table("Logs")
                .Group("Host", "Level")
                .Count()
                .RunGrouping<List<string>, long>(conn);

            var stats = new List<LevelStats>();

            foreach (var item in result.ToList())
            {
                var host = item.Key[0];
                var level = item.Key[1];

                if(!stats.Exists(s => s.Name == host))
                {
                    stats.Add(new LevelStats { Name = host });
                }

                var entry = stats.Find(s => s.Name == host);

                switch (level)
                {
                    case "Trace":
                        entry.Trace = item.Items.First();
                        break;
                    case "Debug":
                        entry.Debug = item.Items.First();
                        break;
                    case "Information":
                        entry.Information = item.Items.First();
                        break;
                    case "Warning":
                        entry.Warning = item.Items.First();
                        break;
                    case "Error":
                        entry.Error = item.Items.First();
                        break;
                    case "Critical":
                        entry.Critical = item.Items.First();
                        break;
                    default:
                        break;
                }
            }

            return stats;
        }

        public dynamic AppLogStats()
        {
            var conn = _rethinkDbFactory.CreateConnection();
            var result = R.Db(_rethinkDbFactory.GetOptions().Database)
                .Table("Logs")
                .Group("Application", "Level")
                .Count()
                .RunGrouping<List<string>, long>(conn);

            var stats = new List<LevelStats>();

            foreach (var item in result.ToList())
            {
                var host = item.Key[0];
                var level = item.Key[1];

                if (!stats.Exists(s => s.Name == host))
                {
                    stats.Add(new LevelStats { Name = host });
                }

                var entry = stats.Find(s => s.Name == host);

                switch (level)
                {
                    case "Trace":
                        entry.Trace = item.Items.First();
                        break;
                    case "Debug":
                        entry.Debug = item.Items.First();
                        break;
                    case "Information":
                        entry.Information = item.Items.First();
                        break;
                    case "Warning":
                        entry.Warning = item.Items.First();
                        break;
                    case "Error":
                        entry.Error = item.Items.First();
                        break;
                    case "Critical":
                        entry.Critical = item.Items.First();
                        break;
                    default:
                        break;
                }
            }

            return stats;
        }
    }

    public class LevelStats
    {
        public string Name { get; set; }
        public long Trace { get; set; }
        public long Debug { get; set; }
        public long Information { get; set; }
        public long Warning { get; set; }
        public long Error { get; set; }
        public long Critical { get; set; }
    }
}
