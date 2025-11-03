using System.Diagnostics;
using eduMentor.Models;
using Microsoft.AspNetCore.Mvc;

namespace eduMentor.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

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



/*
 using Microsoft.AspNetCore.Mvc;

namespace eduMentor.Controllers
{
    public class HomeController : Controller
    {
        // Página principal - Landing
        public IActionResult Index()
        {
            // Si está autenticado, redirigir a la PWA
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            
            return View("~/Views/Landing/Index.cshtml");
        }

        // Otras páginas del Landing
        public IActionResult About()
        {
            return View("~/Views/Landing/About.cshtml");
        }

        public IActionResult Contact()
        {
            return View("~/Views/Landing/Contact.cshtml");
        }
    }
}
 
 
 */
