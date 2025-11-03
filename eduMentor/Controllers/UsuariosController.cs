using eduMentor.Data;
using eduMentor.Models;
using eduMentor.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eduMentor.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public UsuariosController(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuario
                .OrderByDescending(u => u.FechaRegistro)
                .ToListAsync();

            // Cargar roles disponibles
            var roles = await _context.Role
                .Select(r => r.Name)
                .ToListAsync();
            ViewBag.Roles = roles;

            // Obtener el rol de cada usuario
            var userRoles = new Dictionary<int, string>();
            foreach (var usuario in usuarios)
            {
                var userRole = await _userManager.GetRolesAsync(usuario);
                userRoles[usuario.Id] = userRole.FirstOrDefault() ?? "Sin Rol";
            }
            ViewBag.UserRoles = userRoles;

            return View("~/Views/Pwa/Usuarios/Index.cshtml", usuarios);
        }

        // GET: Usuarios/GetUsuario/5 - Para obtener datos completos del usuario (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetUsuario(int id)
        {
            var usuario = await _context.Usuario.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            // Obtener el rol actual del usuario
            var roles = await _userManager.GetRolesAsync(usuario);
            var currentRole = roles.FirstOrDefault();

            // Devolver datos incluyendo el rol
            return Json(new
            {
                id = usuario.Id,
                nombre = usuario.Nombre,
                email = usuario.Email,
                userName = usuario.UserName,
                phoneNumber = usuario.PhoneNumber,
                activo = usuario.Activo,
                role = currentRole,
                fechaRegistro = usuario.FechaRegistro,
                ultimoLogin = usuario.UltimoLogin,
                // Campos de Identity necesarios
                normalizedUserName = usuario.NormalizedUserName,
                normalizedEmail = usuario.NormalizedEmail,
                emailConfirmed = usuario.EmailConfirmed,
                passwordHash = usuario.PasswordHash,
                securityStamp = usuario.SecurityStamp,
                concurrencyStamp = usuario.ConcurrencyStamp,
                phoneNumberConfirmed = usuario.PhoneNumberConfirmed,
                twoFactorEnabled = usuario.TwoFactorEnabled,
                lockoutEnd = usuario.LockoutEnd,
                lockoutEnabled = usuario.LockoutEnabled,
                accessFailedCount = usuario.AccessFailedCount
            });
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor, completa correctamente los campos requeridos.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var user = new Usuario
                {
                    Nombre = model.Nombre,
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Activo = model.Activo,
                    FechaRegistro = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.Role))
                        await _userManager.AddToRoleAsync(user, model.Role);

                    TempData["Success"] = $"Usuario '{model.Nombre}' creado exitosamente.";
                }
                else
                {
                    TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear el usuario: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UsuarioViewModel model)
        {
            var usuario = await _userManager.FindByIdAsync(model.Id.ToString());
            if (usuario == null)
                return NotFound();

            usuario.Nombre = model.Nombre;
            usuario.Email = model.Email;
            usuario.UserName = model.UserName;
            usuario.PhoneNumber = model.PhoneNumber;
            usuario.Activo = model.Activo;
            usuario.NormalizedUserName = usuario.UserName.ToUpperInvariant();
            usuario.NormalizedEmail = usuario.Email.ToUpperInvariant();

            var result = await _userManager.UpdateAsync(usuario);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.Role))
                {
                    var currentRoles = await _userManager.GetRolesAsync(usuario);
                    await _userManager.RemoveFromRolesAsync(usuario, currentRoles);
                    await _userManager.AddToRoleAsync(usuario, model.Role);
                }

                TempData["Success"] = $"Usuario '{model.Nombre}' actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }


        // POST: Usuarios/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var usuario = await _context.Usuario.FindAsync(id);

                if (usuario != null)
                {
                    string nombreUsuario = usuario.Nombre;

                    // Usar UserManager para eliminar (maneja roles automáticamente)
                    var result = await _userManager.DeleteAsync(usuario);

                    if (result.Succeeded)
                    {
                        TempData["Success"] = $"Usuario '{nombreUsuario}' eliminado exitosamente.";
                    }
                    else
                    {
                        TempData["Error"] = "Error al eliminar el usuario.";
                    }
                }
                else
                {
                    TempData["Error"] = "Usuario no encontrado.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar el usuario: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuario.Any(e => e.Id == id);
        }

        // Métodos adicionales útiles

        // GET: API para verificar disponibilidad de username
        [HttpGet]
        public async Task<JsonResult> CheckUserNameAvailability(string username, int? currentUserId)
        {
            var exists = await _context.Usuario
                .AnyAsync(u => u.UserName == username && u.Id != (currentUserId ?? 0));

            return Json(new { available = !exists });
        }

        // GET: API para verificar disponibilidad de email
        [HttpGet]
        public async Task<JsonResult> CheckEmailAvailability(string email, int? currentUserId)
        {
            var exists = await _context.Usuario
                .AnyAsync(u => u.Email == email && u.Id != (currentUserId ?? 0));

            return Json(new { available = !exists });
        }

        // GET: Obtener usuarios por rol
        [HttpGet]
        public async Task<JsonResult> GetUsuariosByRole(string roleName)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);

            var result = usersInRole.Select(u => new
            {
                id = u.Id,
                nombre = u.Nombre,
                email = u.Email,
                userName = u.UserName
            });

            return Json(result);
        }

        // GET: Exportar usuarios a CSV (opcional)
        [HttpGet]
        public async Task<IActionResult> ExportToCSV()
        {
            var usuarios = await _context.Usuario
                .OrderByDescending(u => u.FechaRegistro)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Id,Nombre,Email,Usuario,Telefono,Rol,Estado,FechaRegistro,UltimoLogin");

            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);
                var rol = roles.FirstOrDefault() ?? "Sin Rol";

                csv.AppendLine($"{usuario.Id},{usuario.Nombre},{usuario.Email},{usuario.UserName},{usuario.PhoneNumber},{rol},{(usuario.Activo ? "Activo" : "Inactivo")},{usuario.FechaRegistro:dd/MM/yyyy},{usuario.UltimoLogin:dd/MM/yyyy HH:mm}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "usuarios.csv");
        }
    }
}