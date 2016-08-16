using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Logging;

namespace TokenGen.Controllers
{
    [Route("api/[controller]")]
    public class RdbController : Controller
    {
        private IRethinkDbStore _store;
        private ILogger<RdbController> _logger;

        public RdbController(IRethinkDbStore store, ILogger<RdbController> logger)
        {
            _store = store;
            _logger = logger;
        }

        [Route("[action]/{shards:int}/{replicas:int}")]
        public void Reconfigure(int shards, int replicas)
        {
            _logger.LogInformation($"Reconfigure database tables: set shards to {shards} and replicas to {replicas}");
            _store.Reconfigure(shards, replicas);

        }
    }
}
