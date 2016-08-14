using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RethinkDbLogProvider
{
    public class RethinkDbLogger: ILogger
    {
        private string _appName;
        private string _hostName;
        private string _categoryName;
        private Func<string, LogLevel, bool> _filter;

        {
            _categoryName = categoryName;
            _filter = filter;
            _service = service;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (_filter == null || _filter(_categoryName, logLevel));
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            _service.Log(_appName, _hostName,_categoryName, logLevel.ToString(), eventId.Id, eventId.Name, message, exception);
        }
    }
}
