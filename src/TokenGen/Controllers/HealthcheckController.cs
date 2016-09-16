using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;

namespace TokenGen.Controllers
{
    [Route("api/[controller]")]
    public class Healthcheck : Controller
    {

        [HttpGet]
        public dynamic Get()
        {
            return StatusCode(200);
        }
    }
}
