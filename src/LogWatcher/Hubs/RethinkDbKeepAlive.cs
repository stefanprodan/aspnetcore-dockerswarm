using Microsoft.Extensions.Logging;
using RethinkDb.Driver;
using RethinkDbLogProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LogWatcher
{
    public class RethinkDbKeepAlive
    {
        private static RethinkDB R = RethinkDB.R;
        private readonly IRethinkDbConnectionFactory _rethinkDbFactory;
        private readonly ILogger<RethinkDbKeepAlive> _logger;

        public RethinkDbKeepAlive(IRethinkDbConnectionFactory rethinkDbFactory,
            ILogger<RethinkDbKeepAlive> logger)
        {
            _rethinkDbFactory = rethinkDbFactory;
            _logger = logger;
        }

        public void Start()
        {
            bool firstCall = true;
            while (true)
            {
                var conn = _rethinkDbFactory.CreateConnection();

                try
                {
                    //var srv = conn.Server();
                    //_logger.LogDebug(902, $"Connected to RethinkDB server {srv.Name}");

                    var result = R.Db(_rethinkDbFactory.GetOptions().Database).TableList().RunAtom<List<string>>(conn);
                    if (firstCall)
                    {
                        _logger.LogDebug(902, $"Connected to RethinkDB server {_rethinkDbFactory.GetOptions().Host}");
                    }
                    firstCall = false;
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(1001, ex, $"RethinkDbKeepAlive error {ex.Message}. Connection open {conn.Open}.");

                    conn.Reconnect();
                }

                Thread.Sleep(TimeSpan.FromSeconds(60));
            }
        }
    }
}
