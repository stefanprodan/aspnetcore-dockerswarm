using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RethinkDbLogProvider
{
    public class LogEntry
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
        public string Application { get; set; }
        public string Host { get; set; }
        public string Category { get; set; }
        public string Level { get; set; }
        public int EventId { get; set; }
        public string Event { get; set; }
        public string Message { get; set; }
        public string ExceptionId { get; set; }
    }
}
