using eduMentor.Data;
using eduMentor.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace eduMentor.Controllers
{
    public class ModuloesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModuloesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================
        // INDEX
        // ==========================
        public async Task<IActionResult> Index(int? idCurso)
        {
            if (idCurso == null)
                return RedirectToAction("Index", "Cursos");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (!int.TryParse(userId, out int userIntId))
                return Unauthorized();

            var curso = await _context.Curso
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.IdCurso == idCurso);

            if (curso == null)
            {
                TempData["Error"] = "Curso no encontrado.";
                return RedirectToAction("Index", "Cursos");
            }

            // === Validación de acceso ===
            if (userRole == "Estudiante")
            {
                bool inscrito = await _context.Inscripcion
                    .AnyAsync(i => i.IdEstudiante == userIntId && i.IdCurso == idCurso);

                if (!inscrito)
                {
                    TempData["Error"] = "No estás inscrito en este curso.";
                    return RedirectToAction("Index", "Cursos");
                }
            }
            else if (userRole == "Instructor" && curso.IdInstructor != userIntId)
            {
                TempData["Error"] = "No tienes permiso para acceder a los módulos de este curso.";
                return RedirectToAction("Index", "Cursos");
            }

            // === Datos para la vista ===
            ViewBag.EsInstructor = userRole == "Instructor";
            ViewBag.EsEstudiante = userRole == "Estudiante";

            ViewBag.IdCurso = idCurso;
            ViewBag.TituloCurso = curso.Titulo;
            ViewBag.DescripcionCurso = curso.Descripcion;
            ViewBag.NivelCurso = curso.Nivel;
            ViewBag.NombreInstructor = curso.Instructor?.Nombre ?? "Instructor";

            IQueryable<Modulo> query = _context.Modulo
                .Where(m => m.IdCurso == idCurso)
                .OrderBy(m => m.Orden);

            if (userRole == "Estudiante")
                query = query.Where(m => m.Activo);

            var modulos = await query.ToListAsync();

            return View("~/Views/Pwa/Moduloes/Index.cshtml", modulos);
        }

        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetModulo(int id)
        {
            var modulo = await _context.Modulo.FindAsync(id);
            if (modulo == null) return NotFound();

            return Json(new
            {
                idModulo = modulo.IdModulo,
                idCurso = modulo.IdCurso,
                titulo = modulo.Titulo,
                descripcion = modulo.Descripcion,
                orden = modulo.Orden,
                duracionMinutos = modulo.DuracionMinutos,
                activo = modulo.Activo,
                fechaCreacion = modulo.FechaCreacion
            });
        }


        // ==========================
        // CREATE (desde modal)
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCurso,Titulo,Descripcion,Orden,DuracionMinutos,Activo,FechaCreacion")] Modulo modulo)
        {
            // Evitar validaciones por navegación o campos calculados
            ModelState.Remove(nameof(Modulo.Curso));

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" | ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Index), new { idCurso = modulo.IdCurso });
            }

            try
            {
                // Asegurar que haya orden consecutivo si no se envía
                if (modulo.Orden <= 0)
                {
                    var maxOrden = await _context.Modulo
                        .Where(m => m.IdCurso == modulo.IdCurso)
                        .Select(m => (int?)m.Orden)
                        .MaxAsync() ?? 0;
                    modulo.Orden = maxOrden + 1;
                }

                modulo.FechaCreacion = DateTime.UtcNow;

                _context.Add(modulo);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Módulo '{modulo.Titulo}' creado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear módulo: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { idCurso = modulo.IdCurso });
        }

        // ==========================
        // EDIT (desde modal)
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdModulo,IdCurso,Titulo,Descripcion,Orden,DuracionMinutos,Activo,FechaCreacion")] Modulo modulo)
        {
            if (id != modulo.IdModulo)
                return NotFound();

            ModelState.Remove(nameof(Modulo.Curso));

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" | ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Index), new { idCurso = modulo.IdCurso });
            }

            try
            {
                _context.Update(modulo);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Módulo '{modulo.Titulo}' actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ModuloExists(modulo.IdModulo))
                    return NotFound();
                else
                    TempData["Error"] = "Conflicto de concurrencia al guardar cambios.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar módulo: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { idCurso = modulo.IdCurso });
        }

        // ==========================
        // DELETE (confirmación desde modal)
        // ==========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var modulo = await _context.Modulo.FindAsync(id);
            if (modulo == null)
            {
                TempData["Error"] = "Módulo no encontrado.";
                return RedirectToAction("Index", "Cursos");
            }

            try
            {
                int idCurso = modulo.IdCurso;
                _context.Modulo.Remove(modulo);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Módulo '{modulo.Titulo}' eliminado correctamente.";
                return RedirectToAction(nameof(Index), new { idCurso });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar módulo: {ex.Message}";
                return RedirectToAction(nameof(Index), new { idCurso = modulo?.IdCurso });
            }
        }

        private bool ModuloExists(int id)
        {
            return _context.Modulo.Any(e => e.IdModulo == id);
        }
    }
}
