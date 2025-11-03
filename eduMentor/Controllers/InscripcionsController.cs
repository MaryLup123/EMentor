using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using eduMentor.Data;
using eduMentor.Models;

namespace eduMentor.Controllers
{
    [Authorize] // protege todas las acciones
    public class InscripcionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InscripcionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Inscripcions
        // =====================================================
        // GET: Inscripcions (Catálogo principal del alumno)
        // =====================================================
        public async Task<IActionResult> Index()
        {
            // Obtener usuario logueado
            var userEmail = User.Identity?.Name;
            var estudiante = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (estudiante == null)
                return Redirect("/Identity/Account/Login");

            // Obtener IDs de cursos en los que ya está inscrito
            var cursosInscritos = await _context.Inscripcion
                .Where(i => i.IdEstudiante == estudiante.Id)
                .Select(i => i.IdCurso)
                .ToListAsync();

            // Obtener todos los cursos activos (estado = true o "Activo")
            var cursosActivos = await _context.Curso
                .Include(c => c.Instructor)
                .Where(c => c.Estado ==  c.Estado == true)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();

            // Si tiene inscripciones, habilitamos el sidebar
            ViewBag.SidebarEnabled = cursosInscritos.Any();

            // Pasar los cursos ya inscritos a la vista
            ViewBag.CursosInscritos = cursosInscritos;

            // Título
            ViewData["Title"] = "Catálogo de Cursos";

            return View("~/Views/Pwa/Inscripcions/Index.cshtml", cursosActivos);
        }


        // GET: Inscripcions/Create
        public IActionResult Create()
        {
            // Usuario autenticado actual
            var userEmail = User.Identity?.Name;
            var estudiante = _context.Usuario.FirstOrDefault(u => u.Email == userEmail);

            if (estudiante == null)
                return Redirect("/Identity/Account/Login");

            // Lista de cursos disponibles
            ViewData["IdCurso"] = new SelectList(_context.Curso, "IdCurso", "Nivel");
            ViewData["NombreEstudiante"] = estudiante.Nombre ?? estudiante.Email;

            // Creamos modelo prellenado
            var model = new Inscripcion
            {
                IdEstudiante = estudiante.Id,
                FechaInscripcion = DateTime.UtcNow,
                Estado = "Activo"
            };

            return View("~/Views/Pwa/Inscripcions/Create.cshtml", model);
        }

        // POST: Inscripcions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCurso,IdEstudiante,FechaInscripcion,Estado")] Inscripcion inscripcion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inscripcion);
                await _context.SaveChangesAsync();

                // ✅ Después de crear inscripción, redirigir al panel
                return Redirect("/Pwa/Index");
            }

            ViewData["IdCurso"] = new SelectList(_context.Curso, "IdCurso", "Nivel", inscripcion.IdCurso);
            return View("~/Views/Pwa/Inscripcions/Create.cshtml", inscripcion);
        }

        // GET: Inscripcions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var inscripcion = await _context.Inscripcion
                .Include(i => i.Curso)
                .Include(i => i.Estudiante)
                .FirstOrDefaultAsync(m => m.IdInscripcion == id);

            if (inscripcion == null)
                return NotFound();

            return View("~/Views/Pwa/Inscripcions/Details.cshtml", inscripcion);
        }

        // GET: Inscripcions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var inscripcion = await _context.Inscripcion.FindAsync(id);
            if (inscripcion == null)
                return NotFound();

            ViewData["IdCurso"] = new SelectList(_context.Curso, "IdCurso", "Nivel", inscripcion.IdCurso);
            return View("~/Views/Pwa/Inscripcions/Edit.cshtml", inscripcion);
        }

        // POST: Inscripcions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdInscripcion,IdCurso,IdEstudiante,FechaInscripcion,Estado")] Inscripcion inscripcion)
        {
            if (id != inscripcion.IdInscripcion)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inscripcion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InscripcionExists(inscripcion.IdInscripcion))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["IdCurso"] = new SelectList(_context.Curso, "IdCurso", "Nivel", inscripcion.IdCurso);
            return View("~/Views/Pwa/Inscripcions/Edit.cshtml", inscripcion);
        }

        // GET: Inscripcions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var inscripcion = await _context.Inscripcion
                .Include(i => i.Curso)
                .Include(i => i.Estudiante)
                .FirstOrDefaultAsync(m => m.IdInscripcion == id);

            if (inscripcion == null)
                return NotFound();

            return View("~/Views/Pwa/Inscripcions/Delete.cshtml", inscripcion);
        }

        // POST: Inscripcions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inscripcion = await _context.Inscripcion.FindAsync(id);
            if (inscripcion != null)
                _context.Inscripcion.Remove(inscripcion);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InscripcionExists(int id)
        {
            return _context.Inscripcion.Any(e => e.IdInscripcion == id);
        }



        // ===============================================
        // GET: Inscripcions/MisCursos
        // Devuelve los cursos del usuario logueado
        // ===============================================
        [HttpGet]
        public async Task<IActionResult> MisCursos()
        {
            var userEmail = User.Identity?.Name;
            var estudiante = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (estudiante == null)
                return Unauthorized("Usuario no encontrado o no autenticado.");

            var cursos = await _context.Inscripcion
                .Include(i => i.Curso)
                .ThenInclude(c => c.Instructor)
                .Where(i => i.IdEstudiante == estudiante.Id)
                .Select(i => new
                {
                    i.IdInscripcion,
                    Curso = new
                    {
                        i.Curso.IdCurso,
                        i.Curso.Titulo,
                        i.Curso.Nivel,
                        i.Curso.Estado,
                        Instructor = i.Curso.Instructor.Nombre
                    }
                })
                .ToListAsync();

            return Json(cursos);
        }




        // =====================================================
        // POST: Inscripcions/Inscribirse
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int idCurso)
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var estudiante = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == userEmail);

                if (estudiante == null)
                {
                    TempData["Error"] = "Debes iniciar sesión para inscribirte.";
                    return RedirectToAction(nameof(Index));
                }

                // Validar que el curso exista y esté activo
                var curso = await _context.Curso.FirstOrDefaultAsync(c => c.IdCurso == idCurso && ( c.Estado == true));
                if (curso == null)
                {
                    TempData["Error"] = "Curso no encontrado o inactivo.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar si ya está inscrito
                bool yaInscrito = await _context.Inscripcion
                    .AnyAsync(i => i.IdEstudiante == estudiante.Id && i.IdCurso == idCurso);

                if (yaInscrito)
                {
                    TempData["Success"] = $"Ya estás inscrito en '{curso.Titulo}'.";
                    return RedirectToAction(nameof(Index));
                }

                // Crear inscripción
                var inscripcion = new Inscripcion
                {
                    IdCurso = idCurso,
                    IdEstudiante = estudiante.Id,
                    FechaInscripcion = DateTime.UtcNow,
                    Estado = "Activo"
                };

                _context.Add(inscripcion);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Te has inscrito correctamente en '{curso.Titulo}'.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al inscribirte: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================
        // GET: Inscripcions/GetCursoStats?id=1
        // (Usado por las tarjetas en la vista)
        // =====================================================
        [HttpGet]
        public async Task<JsonResult> GetCursoStats(int id)
        {
            var curso = await _context.Curso
                .Include(c => c.Modulos)
                .Include(c => c.Inscripciones)
                .FirstOrDefaultAsync(c => c.IdCurso == id);

            if (curso == null)
                return Json(new { totalModulos = 0, duracionTotal = 0, totalInscritos = 0 });

            return Json(new
            {
                totalModulos = curso.Modulos?.Count ?? 0,
                duracionTotal = curso.Modulos?.Sum(m => m.DuracionMinutos) ?? 0,
                totalInscritos = curso.Inscripciones?.Count ?? 0
            });
        }
    }
}
