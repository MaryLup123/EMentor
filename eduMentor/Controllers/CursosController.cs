using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eduMentor.Data;
using eduMentor.Models;

namespace eduMentor.Controllers
{
    [Authorize]
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cursos
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!int.TryParse(userId, out int userIntId))
                return BadRequest("ID de usuario inválido");

            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            ViewBag.EsInstructor = userRole == "Instructor";
            ViewBag.EsEstudiante = userRole == "Estudiante";

            List<Curso> cursos;

            if (ViewBag.EsInstructor)
            {
                // Cursos creados por el instructor
                cursos = await _context.Curso
                    .Include(c => c.Instructor)
                    .Where(c => c.IdInstructor == userIntId)
                    .OrderByDescending(c => c.FechaCreacion)
                    .ToListAsync();

                var instructor = await _context.Usuario.FindAsync(userIntId);
                ViewBag.NombreInstructor = instructor?.Nombre ?? "Instructor";
            }
            else
            {
                // Cursos donde el estudiante está inscrito
                cursos = await _context.Inscripcion
                    .Include(i => i.Curso)
                        .ThenInclude(c => c.Instructor)
                    .Where(i => i.IdEstudiante == userIntId)
                    .Select(i => i.Curso)
                    .Distinct()
                    .OrderByDescending(c => c.FechaCreacion)
                    .ToListAsync();

                var estudiante = await _context.Usuario.FindAsync(userIntId);
                ViewBag.NombreInstructor = estudiante?.Nombre ?? estudiante?.Email ?? "Estudiante";

                // ===========================================
                // 🔹 Calcular cursos completados al 100%
                // ===========================================
                var cursosCompletados = new List<int>();

                foreach (var curso in cursos)
                {
                    var modulosCurso = await _context.Modulo
                        .Where(m => m.IdCurso == curso.IdCurso)
                        .Select(m => m.IdModulo)
                        .ToListAsync();

                    if (!modulosCurso.Any())
                        continue;

                    var progreso = await _context.ProgresoModulo
                        .Where(pm => pm.IdEstudiante == userIntId && modulosCurso.Contains(pm.IdModulo))
                        .ToListAsync();

                    bool cursoCompletado = progreso.Count == modulosCurso.Count &&
                                           progreso.All(p => p.PorcentajeAvance >= 100);

                    if (cursoCompletado)
                        cursosCompletados.Add(curso.IdCurso);
                }

                ViewBag.CursosCompletados = cursosCompletados;
            }

            return View("~/Views/Pwa/Cursos/Index.cshtml", cursos);
        }

        // GET: Cursos/GetCurso/5 - Para obtener datos del curso vía AJAX
        [HttpGet]
        public async Task<IActionResult> GetCurso(int id)
        {
            var curso = await _context.Curso
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.IdCurso == id);

            if (curso == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userId, out int instructorId) && curso.IdInstructor != instructorId)
                return Forbid();

            return Json(new
            {
                idCurso = curso.IdCurso,
                idInstructor = curso.IdInstructor,
                titulo = curso.Titulo,
                descripcion = curso.Descripcion,
                nivel = curso.Nivel,
                estado = curso.Estado,
                fechaCreacion = curso.FechaCreacion,
                ultimaActualizacion = curso.UltimaActualizacion
            });
        }

        // POST: Cursos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Titulo,Descripcion,Nivel,Estado")] Curso curso)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine("❌ " + error.ErrorMessage);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userId, out int instructorId))
                {
                    TempData["Error"] = "Error al identificar al instructor.";
                    return RedirectToAction(nameof(Index));
                }

                curso.IdInstructor = instructorId;
                curso.FechaCreacion = DateTime.UtcNow;
                curso.UltimaActualizacion = DateTime.UtcNow;

                _context.Add(curso);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Curso '{curso.Titulo}' creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear el curso: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cursos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCurso,IdInstructor,Titulo,Descripcion,Nivel,Estado,FechaCreacion")] Curso curso)
        {
            if (id != curso.IdCurso)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out int instructorId) || curso.IdInstructor != instructorId)
            {
                TempData["Error"] = "No tienes permiso para editar este curso.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                curso.UltimaActualizacion = DateTime.UtcNow;
                _context.Update(curso);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Curso '{curso.Titulo}' actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CursoExists(curso.IdCurso))
                    return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar el curso: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cursos/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var curso = await _context.Curso.FindAsync(id);
                if (curso == null)
                {
                    TempData["Error"] = "Curso no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userId, out int instructorId) || curso.IdInstructor != instructorId)
                {
                    TempData["Error"] = "No tienes permiso para eliminar este curso.";
                    return RedirectToAction(nameof(Index));
                }

                string tituloCurso = curso.Titulo;
                _context.Curso.Remove(curso);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Curso '{tituloCurso}' eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar el curso: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Cursos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var curso = await _context.Curso
                .Include(c => c.Instructor)
                .Include(c => c.Modulos)
                .FirstOrDefaultAsync(m => m.IdCurso == id);

            if (curso == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out int instructorId) || curso.IdInstructor != instructorId)
                return Forbid();

            return View("~/Views/Pwa/Cursos/Details.cshtml", curso);
        }

        private bool CursoExists(int id) =>
            _context.Curso.Any(e => e.IdCurso == id);

        // GET: Contar módulos de un curso
        [HttpGet]
        public async Task<JsonResult> GetModulosCount(int cursoId)
        {
            var count = await _context.Modulo
                .Where(m => m.IdCurso == cursoId)
                .CountAsync();

            return Json(new { count });
        }

        // GET: Estadísticas del curso
        [HttpGet]
        public async Task<JsonResult> GetCursoStats(int cursoId)
        {
            var curso = await _context.Curso
                .Include(c => c.Modulos)
                .FirstOrDefaultAsync(c => c.IdCurso == cursoId);

            if (curso == null)
                return Json(new { error = "Curso no encontrado" });

            var stats = new
            {
                totalModulos = curso.Modulos?.Count ?? 0,
                duracionTotal = curso.Modulos?.Sum(m => m.DuracionMinutos) ?? 0,
                modulosActivos = curso.Modulos?.Count(m => m.Activo) ?? 0
            };

            return Json(stats);
        }
    }
}
