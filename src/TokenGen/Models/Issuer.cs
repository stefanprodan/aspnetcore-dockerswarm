using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenGen
{
    public class Issuer
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
