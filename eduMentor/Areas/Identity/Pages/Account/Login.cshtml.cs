// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using eduMentor.Data;
using eduMentor.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace eduMentor.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<Usuario> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<Usuario> _userManager;
        private readonly IServiceProvider _serviceProvider;
        public LoginModel(
            SignInManager<Usuario> signInManager,
            UserManager<Usuario> userManager,
            IServiceProvider serviceProvider,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }


        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }


        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Redirigir por defecto al dashboard
            returnUrl ??= Url.Content("~/Pwa/Index");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Intentar iniciar sesión
                var result = await _signInManager.PasswordSignInAsync(
                    Input.Email,
                    Input.Password,
                    Input.RememberMe,
                    lockoutOnFailure: false
                );

                if (result.Succeeded)
                {
                    _logger.LogInformation($"✅ Usuario {Input.Email} inició sesión correctamente.");

                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user != null)
                    {
                        // Actualizar último login
                        user.UltimoLogin = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);

                        // 🔹 Validar si es Alumno y no tiene inscripción
                        if (await _userManager.IsInRoleAsync(user, "Alumno"))
                        {
                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                bool tieneInscripcion = db.Inscripcion.Any(i => i.IdEstudiante == user.Id);

                                if (!tieneInscripcion)
                                {
                                    TempData["msg"] = "⚠️ Debes completar tu inscripción antes de acceder al panel.";
                                    return LocalRedirect("/Inscripcions/Index");
                                }
                            }
                        }
                    }

                    // 🔹 Redirigir al dashboard si todo está bien
                    return LocalRedirect(returnUrl);
                }

                if (result.RequiresTwoFactor)
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });

                if (result.IsLockedOut)
                {
                    _logger.LogWarning($"⚠️ Usuario {Input.Email} bloqueado temporalmente.");
                    return RedirectToPage("./Lockout");
                }

                // Credenciales incorrectas
                ModelState.AddModelError(string.Empty, "Intento de inicio de sesión no válido.");
            }

            // Si el modelo no es válido
            return Page();
        }

    }
}
