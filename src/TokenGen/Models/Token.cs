using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenGen.Models
{
    public class Token
    {
        public string Guid { get; set; }
        public DateTime Expires { get; set; }
        public string Issuer { get; set; }
        public string Version { get; set; }
    }
}
