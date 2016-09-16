using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LogWatcher.Controllers
{
    public class HomeController : Controller
    {
        private readonly BackgroundTaskManager _backgroundTaskManager;
        public HomeController(BackgroundTaskManager backgroundTaskManager)
        {
            _backgroundTaskManager = backgroundTaskManager;
            backgroundTaskManager.StartKeepAlive();
            backgroundTaskManager.StartLogChangeFeed();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Search()
        {
            return View();
        }

        public IActionResult Stats()
        {
            return View();
        }

        public IActionResult Healthcheck()
        {
            if (_backgroundTaskManager.IsConnected())
            {
                return new EmptyResult();
            }

            return StatusCode(500); 
        }
    }
}
