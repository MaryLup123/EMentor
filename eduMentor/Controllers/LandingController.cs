using Microsoft.AspNetCore.Mvc;

namespace eduMentor.Controllers
{
    public class LandingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Cursos()
        {
            return View(); 
        }
    }
}
