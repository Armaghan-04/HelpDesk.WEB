using HelpDesk.WEB.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HelpDesk.WEB.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Convenience alias so the commonly guessed /Home/Create URL reaches the
        // ticket form. Authorisation is still enforced by TicketsController.
        [HttpGet]
        public IActionResult Create() => RedirectToAction("Create", "Tickets");

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
