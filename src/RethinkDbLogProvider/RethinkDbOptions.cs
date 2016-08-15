using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RethinkDbLogProvider
{
    public class RethinkDbOptions
    {
        public string Application { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public int Timeout { get; set; }
    }
}
