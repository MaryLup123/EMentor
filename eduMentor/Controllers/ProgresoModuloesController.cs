using eduMentor.Data;
using eduMentor.Models;
using eduMentor.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace eduMentor.Controllers
{
    public class ProgresoModuloesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProgresoModuloesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProgresoModuloes
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int idInstructor))
                return Unauthorized();

            // 🔹 Cursos impartidos por este instructor
            var cursos = await _context.Curso
                .Include(c => c.Modulos)
                .Where(c => c.IdInstructor == idInstructor)
                .ToListAsync();

            var dashboard = new List<CursoInstructorVM>();

            foreach (var curso in cursos)
            {
                // Estudiantes inscritos a este curso
                var inscripciones = await _context.Inscripcion
                    .Where(i => i.IdCurso == curso.IdCurso)
                    .ToListAsync();

                int totalEstudiantes = inscripciones.Count;
                int completados = 0, enProgreso = 0, sinIniciar = 0;
                double sumaAvances = 0;

                var modulosVM = new List<ModuloInstructorVM>();

                foreach (var modulo in curso.Modulos)
                {
                    var progresos = await _context.ProgresoModulo
                        .Where(pm => pm.IdModulo == modulo.IdModulo)
                        .ToListAsync();

                    double promedioModulo = progresos.Any()
                        ? Math.Round(progresos.Average(p => p.PorcentajeAvance), 2)
                        : 0;

                    modulosVM.Add(new ModuloInstructorVM
                    {
                        IdModulo = modulo.IdModulo,
                        Titulo = modulo.Titulo,
                        PromedioAvance = promedioModulo
                    });
                }

                // 🔹 Por estudiante: promedio de avance entre módulos del curso
                foreach (var inscripcion in inscripciones)
                {
                    var modulosIds = curso.Modulos.Select(m => m.IdModulo).ToList();

                    var progresosEstudiante = await _context.ProgresoModulo
                        .Where(pm => pm.IdEstudiante == inscripcion.IdEstudiante && modulosIds.Contains(pm.IdModulo))
                        .ToListAsync();

                    double promedioEstudiante = progresosEstudiante.Any()
                        ? progresosEstudiante.Average(pm => pm.PorcentajeAvance)
                        : 0;

                    sumaAvances += promedioEstudiante;

                    if (promedioEstudiante >= 100)
                        completados++;
                    else if (promedioEstudiante > 0)
                        enProgreso++;
                    else
                        sinIniciar++;
                }

                double promedioAvanceCurso = totalEstudiantes > 0
                    ? Math.Round(sumaAvances / totalEstudiantes, 2)
                    : 0;

                dashboard.Add(new CursoInstructorVM
                {
                    IdCurso = curso.IdCurso,
                    Titulo = curso.Titulo,
                    Nivel = curso.Nivel,
                    Descripcion = curso.Descripcion,
                    TotalEstudiantes = totalEstudiantes,
                    EstudiantesCompletaron = completados,
                    EstudiantesEnProgreso = enProgreso,
                    EstudiantesSinIniciar = sinIniciar,
                    PromedioAvance = promedioAvanceCurso,
                    Modulos = modulosVM
                });
            }

            return View("~/Views/Pwa/ProgresoModuloes/Index.cshtml", dashboard);
        }

        // GET: ProgresoModuloes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var progresoModulo = await _context.ProgresoModulo
                .Include(p => p.Estudiante)
                .Include(p => p.Modulo)
                .FirstOrDefaultAsync(m => m.IdProgreso == id);
            if (progresoModulo == null)
            {
                return NotFound();
            }

            return View("~/Views/Pwa/ProgresoModuloes/Details.cshtml", progresoModulo);
        }

        // GET: ProgresoModuloes/Create
        public IActionResult Create()
        {
            ViewData["IdEstudiante"] = new SelectList(_context.Usuario, "Id", "Id");
            ViewData["IdModulo"] = new SelectList(_context.Modulo, "IdModulo", "Titulo");
            return View("~/Views/Pwa/ProgresoModuloes/Create.cshtml");
        }

        // POST: ProgresoModuloes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProgreso,IdModulo,IdEstudiante,PorcentajeAvance,TareasCompletadas,TotalTareas,UltimaActualizacion,Completado")] ProgresoModulo progresoModulo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(progresoModulo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdEstudiante"] = new SelectList(_context.Usuario, "Id", "Id", progresoModulo.IdEstudiante);
            ViewData["IdModulo"] = new SelectList(_context.Modulo, "IdModulo", "Titulo", progresoModulo.IdModulo);
            return View("~/Views/Pwa/ProgresoModuloes/Create.cshtml", progresoModulo);
        }

        // GET: ProgresoModuloes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var progresoModulo = await _context.ProgresoModulo.FindAsync(id);
            if (progresoModulo == null)
            {
                return NotFound();
            }
            ViewData["IdEstudiante"] = new SelectList(_context.Usuario, "Id", "Id", progresoModulo.IdEstudiante);
            ViewData["IdModulo"] = new SelectList(_context.Modulo, "IdModulo", "Titulo", progresoModulo.IdModulo);
            return View("~/Views/Pwa/ProgresoModuloes/Edit.cshtml", progresoModulo);
        }

        // POST: ProgresoModuloes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProgreso,IdModulo,IdEstudiante,PorcentajeAvance,TareasCompletadas,TotalTareas,UltimaActualizacion,Completado")] ProgresoModulo progresoModulo)
        {
            if (id != progresoModulo.IdProgreso)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(progresoModulo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProgresoModuloExists(progresoModulo.IdProgreso))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdEstudiante"] = new SelectList(_context.Usuario, "Id", "Id", progresoModulo.IdEstudiante);
            ViewData["IdModulo"] = new SelectList(_context.Modulo, "IdModulo", "Titulo", progresoModulo.IdModulo);
            return View("~/Views/Pwa/ProgresoModuloes/Edit.cshtml", progresoModulo);
        }

        // GET: ProgresoModuloes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var progresoModulo = await _context.ProgresoModulo
                .Include(p => p.Estudiante)
                .Include(p => p.Modulo)
                .FirstOrDefaultAsync(m => m.IdProgreso == id);
            if (progresoModulo == null)
            {
                return NotFound();
            }

            return View("~/Views/Pwa/ProgresoModuloes/Delete.cshtml", progresoModulo);
        }

        // POST: ProgresoModuloes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var progresoModulo = await _context.ProgresoModulo.FindAsync(id);
            if (progresoModulo != null)
            {
                _context.ProgresoModulo.Remove(progresoModulo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProgresoModuloExists(int id)
        {
            return _context.ProgresoModulo.Any(e => e.IdProgreso == id);
        }
    }
}