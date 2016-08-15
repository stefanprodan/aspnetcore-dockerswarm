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
    public class TokenController : Controller
    {
        private IRethinkDbStore _store;
        private ILogger<TokenController> _logger;

        public TokenController(IRethinkDbStore store, ILogger<TokenController> logger)
        {
            _store = store;
            _logger = logger;
        }

        [HttpGet]
        public Token Get()
        {
            _logger.LogInformation("Info");
            _logger.LogWarning(new EventId(1, "TestEvent"), new Exception("Some ex"), "some message");

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
