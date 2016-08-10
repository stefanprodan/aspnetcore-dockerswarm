using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;

namespace TokenGen.Controllers
{
    [Route("api/[controller]")]
    public class IssuerController : Controller
    {
        private RethinkDbStore _store;

        public IssuerController(RethinkDbStore store)
        {
            _store = store;
        }

        [HttpGet]
        public dynamic Get()
        {
            return _store.GetIssuerStatus();
        }
    }
}
