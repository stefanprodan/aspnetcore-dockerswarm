using Microsoft.Extensions.Logging;
using System;

namespace RethinkDbLogProvider
{
    public static class LoggerFactoryExtensions
    {
        public static ILoggerFactory AddRethinkDb(this ILoggerFactory factory, 
            IRethinkDbLoggerService service, 
            Func<string, LogLevel, bool> filter = null)
        {
            factory.AddProvider(new RethinkDbLoggerProvider(filter, service));
            return factory;
        }

        public static ILoggerFactory AddRethinkDb(this ILoggerFactory factory, 
            IRethinkDbLoggerService service, 
            LogLevel minLevel)
        {
            return AddRethinkDb(
                factory,
                service,
                (_, logLevel) => logLevel >= minLevel);
        }
    }
}
