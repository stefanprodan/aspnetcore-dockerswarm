using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;

namespace TokenGen.Controllers
{
    [Route("api/[controller]")]
    public class TokenController : Controller
    {
        private RethinkDbStore _store;

        public TokenController(RethinkDbStore store)
        {
            _store = store;
        }

        [HttpGet]
        public Token Get()
        {
            var token = new Token
            {
                Id = Guid.NewGuid().ToString(),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = Environment.MachineName
            };

            _store.InserToken(token);

            return token;
        }

        [HttpGet("{id}")]
        public TokenStatus Get(string id)
        {
            return _store.GetTokenStatus(id);
        }
    }
}
