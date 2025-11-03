using eduMentor.Data;
using eduMentor.Models;
using eduMentor.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace eduMentor.Controllers
{
    public class ContenidosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContenidosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===============================
        // LISTADO DE CONTENIDOS POR MÓDULO
        // ===============================
        public async Task<IActionResult> Index(int? idModulo)
        {
            if (idModulo == null)
            {
                TempData["Error"] = "Debe seleccionar un módulo para ver su contenido.";
                return RedirectToAction("Index", "Cursos");
            }

            var modulo = await _context.Modulo
                .Include(m => m.Curso)
                    .ThenInclude(c => c.Instructor)
                .FirstOrDefaultAsync(m => m.IdModulo == idModulo);

            if (modulo == null)
            {
                TempData["Error"] = "Módulo no encontrado.";
                return RedirectToAction("Index", "Cursos");
            }

            // Usuario actual
            var userEmail = User.Identity?.Name;
            var usuarioActual = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == userEmail);

            bool esInstructor = usuarioActual != null && modulo.Curso != null &&
                                modulo.Curso.IdInstructor == usuarioActual.Id;

            ViewBag.IdModulo = modulo.IdModulo;
            ViewBag.TituloModulo = modulo.Titulo;
            ViewBag.IdCurso = modulo.IdCurso;
            ViewBag.TituloCurso = modulo.Curso?.Titulo ?? "Curso";
            ViewBag.EsInstructor = esInstructor;

            // ===============================
            // 📘 CONTENIDOS + PROGRESO (si existe)
            // ===============================
            var contenidos = await _context.Contenido
                .Where(c => c.IdModulo == idModulo)
                .OrderBy(c => c.Orden)
                .Select(c => new ContenidoConProgresoViewModel
                {
                    IdContenido = c.IdContenido,
                    IdModulo = c.IdModulo,
                    Titulo = c.Titulo,
                    Tipo = c.Tipo,
                    JsonContenido = c.JsonContenido,
                    EsTarea = c.EsTarea,
                    Peso = c.Peso,
                    Orden = c.Orden,
                    FechaCreacion = c.FechaCreacion,
                    FechaProgreso = _context.ProgresoContenido
                        .Where(p => p.IdContenido == c.IdContenido && p.IdEstudiante == usuarioActual.Id)
                        .Select(p => (DateTime?)p.FechaCompletado)
                        .FirstOrDefault()
                })
                .ToListAsync();


            return View("~/Views/Pwa/Contenidos/Index.cshtml", contenidos);
        }



        // ===============================
        // CREAR CONTENIDO (LECTURA NORMAL)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdModulo,Titulo,Tipo,JsonContenido,EsTarea,Peso,Orden,FechaCreacion")] Contenido contenido)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    Console.WriteLine("❌ " + error.ErrorMessage);
            }
            contenido.FechaCreacion = DateTime.UtcNow;
            _context.Add(contenido);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Contenido creado correctamente.";
            return RedirectToAction(nameof(Index), new { idModulo = contenido.IdModulo });
        }

        // ===============================
        // CREAR TAREA / FORMULARIO (SurveyJS)
        // ===============================
        [HttpGet]
        public async Task<IActionResult> CreateTarea(int idModulo)
        {
            ViewBag.IdModulo = idModulo;

            // Verificar permisos
            var modulo = await _context.Modulo
                .Include(m => m.Curso)
                .FirstOrDefaultAsync(m => m.IdModulo == idModulo);

            if (modulo == null)
            {
                TempData["Error"] = "Módulo no encontrado.";
                return RedirectToAction("Index", "Cursos");
            }

            var userEmail = User.Identity?.Name;
            var usuarioActual = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == userEmail);

            if (usuarioActual == null || modulo.Curso.IdInstructor != usuarioActual.Id)
            {
                TempData["Error"] = "No tienes permisos para crear contenido en este módulo.";
                return RedirectToAction(nameof(Index), new { idModulo });
            }

            return View("~/Views/Pwa/Contenidos/CreateTarea.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTarea([Bind("IdModulo,Titulo,Tipo,JsonContenido,EsTarea,Peso,Orden,FechaCreacion")] Contenido contenido)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Error al crear la tarea. Verifique los campos.";
                ViewBag.IdModulo = contenido.IdModulo;
                return View("~/Views/Pwa/Contenidos/CreateTarea.cshtml", contenido);
            }

            contenido.FechaCreacion = DateTime.UtcNow;
            _context.Add(contenido);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Tarea / formulario creado correctamente.";
            return RedirectToAction(nameof(Index), new { idModulo = contenido.IdModulo });
        }

        // ===============================
        // EDITAR CONTENIDO
        // ===============================
        // ===============================
        // EDITAR CONTENIDO (Lectura o Tarea)
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var contenido = await _context.Contenido.FindAsync(id);
            if (contenido == null)
            {
                TempData["Error"] = "Contenido no encontrado.";
                return RedirectToAction("Index", "Cursos");
            }

            ViewBag.IdModulo = contenido.IdModulo;
            ViewBag.EsTarea = contenido.EsTarea;

            if (contenido.EsTarea)
                return View("~/Views/Pwa/Contenidos/CreateTarea.cshtml", contenido);
            else
                return View("~/Views/Pwa/Contenidos/_CreateContenidoLecturaModal.cshtml", contenido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdContenido,IdModulo,Titulo,Tipo,JsonContenido,EsTarea,Peso,Orden,FechaCreacion")] Contenido contenido)
        {
            if (id != contenido.IdContenido)
            {
                TempData["Error"] = "Identificador inválido.";
                return RedirectToAction("Index", new { idModulo = contenido.IdModulo });
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Error al actualizar el contenido. Verifique los datos ingresados.";
                return contenido.EsTarea
                    ? View("~/Views/Pwa/Contenidos/CreateTarea.cshtml", contenido)
                    : View("~/Views/Pwa/Contenidos/_CreateContenidoLecturaModal.cshtml", contenido);
            }

            try
            {
                var contenidoExistente = await _context.Contenido.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.IdContenido == id);

                if (contenidoExistente == null)
                {
                    TempData["Error"] = "Contenido no encontrado en base de datos.";
                    return RedirectToAction(nameof(Index), new { idModulo = contenido.IdModulo });
                }

                contenido.FechaCreacion = contenidoExistente.FechaCreacion;

                _context.Update(contenido);
                await _context.SaveChangesAsync();

                TempData["Success"] = contenido.EsTarea
                    ? "Tarea / formulario actualizado correctamente."
                    : "Contenido de lectura actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Contenido.Any(e => e.IdContenido == contenido.IdContenido))
                {
                    TempData["Error"] = "El contenido ya no existe.";
                    return RedirectToAction(nameof(Index), new { idModulo = contenido.IdModulo });
                }
                throw;
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar contenido: {ex.Message}";
            }

            return RedirectToAction(nameof(Index), new { idModulo = contenido.IdModulo });
        }


        // ===============================
        // ELIMINAR CONTENIDO
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var contenido = await _context.Contenido.FindAsync(id);
            if (contenido == null)
            {
                TempData["Error"] = "Contenido no encontrado.";
                return RedirectToAction("Index", "Cursos");
            }

            try
            {
                var idModulo = contenido.IdModulo;
                _context.Contenido.Remove(contenido);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Contenido eliminado correctamente.";
                return RedirectToAction(nameof(Index), new { idModulo });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar contenido: {ex.Message}";
                return RedirectToAction(nameof(Index), new { idModulo = contenido.IdModulo });
            }
        }

        private bool ContenidoExists(int id)
        {
            return _context.Contenido.Any(e => e.IdContenido == id);
        }

        // ===============================
        // API: Obtener un contenido (JSON)
        // ===============================
        [HttpGet]
        public async Task<IActionResult> GetContenido(int id)
        {
            var contenido = await _context.Contenido.FindAsync(id);
            if (contenido == null)
                return NotFound();

            return Json(new
            {
                idContenido = contenido.IdContenido,
                idModulo = contenido.IdModulo,
                titulo = contenido.Titulo,
                tipo = contenido.Tipo,
                jsonContenido = contenido.JsonContenido,
                esTarea = contenido.EsTarea,
                peso = contenido.Peso,
                orden = contenido.Orden,
                fechaCreacion = contenido.FechaCreacion
            });
        }

        // ===============================
        // VISTA DE CONTENIDO / TAREA
        // ===============================
        public async Task<IActionResult> Vista(int id)
        {
            var contenido = await _context.Contenido
                .Include(c => c.Modulo)
                    .ThenInclude(m => m.Curso)
                .FirstOrDefaultAsync(c => c.IdContenido == id);

            if (contenido == null)
                return NotFound();

            ViewBag.TituloModulo = contenido.Modulo?.Titulo ?? "Módulo";
            ViewBag.IdModulo = contenido.IdModulo;
            ViewBag.IdCurso = contenido.Modulo?.IdCurso;

            return View("~/Views/Pwa/Contenidos/Vista.cshtml", contenido);
        }

        public async Task<IActionResult> Formulario(int id)
        {
            var contenido = await _context.Contenido
                .Include(c => c.Modulo)
                    .ThenInclude(m => m.Curso)
                .FirstOrDefaultAsync(c => c.IdContenido == id);

            if (contenido == null || !contenido.EsTarea)
                return NotFound();

            ViewBag.TituloModulo = contenido.Modulo?.Titulo ?? "Módulo";
            ViewBag.IdModulo = contenido.IdModulo;
            ViewBag.IdCurso = contenido.Modulo?.IdCurso;

            return View("~/Views/Pwa/Contenidos/Formulario.cshtml", contenido);
        }
    }
}