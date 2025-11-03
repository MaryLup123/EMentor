using eduMentor.Data;
using eduMentor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class PwaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public PwaController(ApplicationDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Redirect("/Identity/Account/Login");
        }

        var roles = await _userManager.GetRolesAsync(user);
        bool esAlumno = roles.Contains("Alumno", StringComparer.OrdinalIgnoreCase);

        if (esAlumno)
        {
            bool inscrito = _context.Inscripcion.Any(i => i.IdEstudiante == user.Id);
            if (!inscrito)
            {
                TempData["msg"] = "⚠️ Debes completar tu inscripción antes de acceder al panel.";
                return Redirect("/Inscripcions/Index");
            }
        }

        return View("~/Views/Pwa/Index.cshtml");
    }
}
