#nullable disable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using eduMentor.Models;
using System.Linq;
using System.Threading.Tasks;

namespace eduMentor.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Usuario> _signInManager;
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserStore<Usuario> _userStore;
        private readonly IUserEmailStore<Usuario> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<Usuario> userManager,
            IUserStore<Usuario> userStore,
            SignInManager<Usuario> signInManager,
            RoleManager<Role> roleManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required, EmailAddress]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; }

            [Required, StringLength(100, ErrorMessage = "La contraseña debe tener al menos {2} caracteres y máximo {1}.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "Registrarme como")]
            public string Rol { get; set; } // 👈 Nuevo campo
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Redirección base: alumnos van a Inscripción, instructores al PWA
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = new Usuario
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    Nombre = Input.Email.Split('@')[0],
                    Activo = true,
                    FechaRegistro = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Nuevo usuario registrado.");

                    // 🔹 Crear rol si no existe
                    if (!await _roleManager.RoleExistsAsync(Input.Rol))
                        await _roleManager.CreateAsync(new Role { Name = Input.Rol, Descripcion = $"Rol {Input.Rol}" });

                    // 🔹 Asignar rol
                    await _userManager.AddToRoleAsync(user, Input.Rol);

                    // 🔹 Autologin
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // 🔹 Redirección condicional según rol
                    if (Input.Rol == "Alumno")
                        return LocalRedirect("~/Inscripcions/Index");
                    else
                        return LocalRedirect("~/Pwa/Index");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        private Usuario CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Usuario>();
            }
            catch
            {
                throw new InvalidOperationException($"No se pudo crear una instancia de '{nameof(Usuario)}'. " +
                    $"Asegúrate de que '{nameof(Usuario)}' tenga un constructor sin parámetros.");
            }
        }

        private IUserEmailStore<Usuario> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("El sistema de Identity requiere soporte de email.");
            return (IUserEmailStore<Usuario>)_userStore;
        }
    }
}
