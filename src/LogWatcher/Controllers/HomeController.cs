using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace LogWatcher.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(BackgroundTaskManager backgroundTaskManager)
        {
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
    }
}
