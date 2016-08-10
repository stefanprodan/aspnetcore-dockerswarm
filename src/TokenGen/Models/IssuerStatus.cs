using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenGen
{
    public class IssuerStatus
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public DateTime RegisterDate { get; set; }
        public long TotalTokensIssued { get; set; }
    }
}
