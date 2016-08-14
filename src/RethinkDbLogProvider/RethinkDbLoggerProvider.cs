using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RethinkDbLogProvider
{
    public class RethinkDbLoggerProvider : ILoggerProvider
    {
        private readonly Func<string, LogLevel, bool> _filter;

        {
            _service = service;
            _filter = filter;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new RethinkDbLogger(categoryName, _filter, _service);
        }

        public void Dispose()
        {
        }
    }
}
