using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RethinkDbLogProvider
{
    public interface IRethinkDbLoggerService
    {
        void ApplySchema();
        void CloseConnection();
        void Log(string categoryName, string logLevel, int eventId, string eventName, string message, Exception exception);
    }
}
