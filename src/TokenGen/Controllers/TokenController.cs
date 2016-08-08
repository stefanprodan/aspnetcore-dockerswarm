using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TokenGen.Models;

namespace TokenGen.Controllers
{
    [Route("api/[controller]")]
    public class TokenController : Controller
    {
        [HttpGet]
        public Token Get()
        {
            return new Token
            {
                Guid = Guid.NewGuid().ToString(),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = Environment.MachineName
            };
        }
    }
}
