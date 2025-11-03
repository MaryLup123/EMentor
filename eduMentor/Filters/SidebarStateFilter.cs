using eduMentor.Data;
using eduMentor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace eduMentor.Filters
{
    public class SidebarStateFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public SidebarStateFilter(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controller = context.Controller as Controller;
            if (controller == null)
            {
                await next();
                return;
            }

            var user = await _userManager.GetUserAsync(controller.User);

            bool sidebarEnabled = true;
            string userRole = null;
            string userName = "Usuario";

            if (user != null)
            {
                userName = user.Nombre ?? user.Email;
                var roles = await _userManager.GetRolesAsync(user);
                userRole = roles.FirstOrDefault() ?? "SinRol";

                if (roles.Contains("Alumno", StringComparer.OrdinalIgnoreCase))
                {
                    bool inscrito = _context.Inscripcion.Any(i => i.IdEstudiante == user.Id);
                    sidebarEnabled = inscrito;
                }
            }

            controller.ViewBag.SidebarEnabled = sidebarEnabled;
            controller.ViewBag.UserName = userName;
            controller.ViewBag.UserRole = userRole;

            await next();
        }
    }
}
