using eduMentor.Data;
using eduMentor.Models;
using eduMentor.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace eduMentor.Controllers
{
    public class ProgresoContenidosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProgresoContenidosController(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int idEstudiante))
                return Unauthorized();

            // 🔹 Obtener cursos en los que está inscrito el estudiante
            var cursosInscritos = await _context.Inscripcion
                .Include(i => i.Curso)
                .ThenInclude(c => c.Modulos)
                .Where(i => i.IdEstudiante == idEstudiante)
                .Select(i => i.Curso)
                .ToListAsync();

            var progresoCursos = new List<CursoProgresoVM>();

            foreach (var curso in cursosInscritos)
            {
                var modulos = await _context.Modulo
                    .Where(m => m.IdCurso == curso.IdCurso)
                    .ToListAsync();

                int totalModulos = modulos.Count;
                int modulosCompletados = 0;
                double sumaPorcentajes = 0;

                var modulosData = new List<ModuloProgresoVM>();

                foreach (var modulo in modulos)
                {
                    var progresoModulo = await _context.ProgresoModulo
                        .FirstOrDefaultAsync(pm => pm.IdModulo == modulo.IdModulo && pm.IdEstudiante == idEstudiante);

                    double porcentajeModulo = progresoModulo?.PorcentajeAvance ?? 0;
                    bool completado = progresoModulo?.Completado ?? false;

                    sumaPorcentajes += porcentajeModulo;
                    if (completado) modulosCompletados++;

                    modulosData.Add(new ModuloProgresoVM
                    {
                        IdModulo = modulo.IdModulo,
                        Titulo = modulo.Titulo,
                        Porcentaje = porcentajeModulo,
                        Completado = completado
                    });
                }

                double progresoCurso = totalModulos > 0 ? Math.Round(sumaPorcentajes / totalModulos, 2) : 0;

                progresoCursos.Add(new CursoProgresoVM
                {
                    IdCurso = curso.IdCurso,
                    Titulo = curso.Titulo,
                    Nivel = curso.Nivel,
                    Descripcion = curso.Descripcion,
                    TotalModulos = totalModulos,
                    ModulosCompletados = modulosCompletados,
                    PorcentajeAvance = progresoCurso,
                    Completado = progresoCurso >= 100,
                    Modulos = modulosData
                });
            }

            ViewBag.IdEstudiante = idEstudiante;
            return View("~/Views/Pwa/ProgresoContenidos/Index.cshtml", progresoCursos);
        }



        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var progreso = await _context.ProgresoContenido
                .Include(p => p.Contenido)
                .Include(p => p.Estudiante)
                .FirstOrDefaultAsync(m => m.IdProgresoContenido == id);

            if (progreso == null)
                return NotFound();

            return View(progreso);
        }

        public IActionResult Create()
        {
            ViewData["IdContenido"] = new SelectList(_context.Contenido, "IdContenido", "Titulo");
            ViewData["IdEstudiante"] = new SelectList(_context.Usuario, "Id", "Email");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProgresoContenido,IdContenido,IdEstudiante,Completado,FechaCompletado,Peso")] ProgresoContenido progresoContenido)
        {
            if (ModelState.IsValid)
            {
                _context.Add(progresoContenido);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdContenido"] = new SelectList(_context.Contenido, "IdContenido", "Titulo", progresoContenido.IdContenido);
            ViewData["IdEstudiante"] = new SelectList(_context.Usuario, "Id", "Email", progresoContenido.IdEstudiante);
            return View(progresoContenido);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var progresoContenido = await _context.ProgresoContenido.FindAsync(id);
            if (progresoContenido == null)
                return NotFound();

            ViewData["IdContenido"] = new SelectList(_context.Contenido, "IdContenido", "Titulo", progresoContenido.IdContenido);
            ViewData["IdEstudiante"] = new SelectList(_context.Usuario, "Id", "Email", progresoContenido.IdEstudiante);
            return View(progresoContenido);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProgresoContenido,IdContenido,IdEstudiante,Completado,FechaCompletado,Peso")] ProgresoContenido progresoContenido)
        {
            if (id != progresoContenido.IdProgresoContenido)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(progresoContenido);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProgresoContenidoExists(progresoContenido.IdProgresoContenido))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(progresoContenido);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var progresoContenido = await _context.ProgresoContenido
                .Include(p => p.Contenido)
                .Include(p => p.Estudiante)
                .FirstOrDefaultAsync(m => m.IdProgresoContenido == id);

            if (progresoContenido == null)
                return NotFound();

            return View(progresoContenido);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var progresoContenido = await _context.ProgresoContenido.FindAsync(id);
            if (progresoContenido != null)
                _context.ProgresoContenido.Remove(progresoContenido);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProgresoContenidoExists(int id)
        {
            return _context.ProgresoContenido.Any(e => e.IdProgresoContenido == id);
        }

        // ============================================================
        // 🟢 FUNCIONALIDAD DE PROGRESO REAL (PWA)
        // ============================================================

        // ✅ Marca un contenido como completado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarCompletado(int idContenido)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!int.TryParse(userId, out int idEstudiante))
                return BadRequest("ID de usuario inválido");

            // Verificar si ya está completado
            var progreso = await _context.ProgresoContenido
                .FirstOrDefaultAsync(p => p.IdContenido == idContenido && p.IdEstudiante == idEstudiante);

            if (progreso != null)
            {
                return Json(new
                {
                    success = false,
                    message = "Este contenido ya fue marcado como completado anteriormente.",
                    completado = true,
                    fecha = progreso.FechaCompletado.ToString("yyyy-MM-dd HH:mm")
                });
            }

            // Crear nuevo progreso de contenido
            progreso = new ProgresoContenido
            {
                IdContenido = idContenido,
                IdEstudiante = idEstudiante,
                Completado = true,
                FechaCompletado = DateTime.UtcNow,
                Peso = 1.0
            };

            _context.ProgresoContenido.Add(progreso);
            await _context.SaveChangesAsync();

            // 🔁 Actualizar progreso del módulo
            var contenido = await _context.Contenido.FindAsync(idContenido);
            if (contenido != null)
            {
                await ActualizarProgresoModuloAsync(contenido.IdModulo, idEstudiante);
            }

            return Json(new
            {
                success = true,
                message = "Contenido marcado como completado.",
                completado = true,
                fecha = progreso.FechaCompletado.ToString("yyyy-MM-dd HH:mm")
            });
        }

        // ✅ Guardar respuestas de formularios o tareas (SurveyJS)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarRespuestas(int idContenido, [FromBody] JsonElement respuestas)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int idEstudiante))
                    return Unauthorized();

                var contenido = await _context.Contenido.FindAsync(idContenido);
                if (contenido == null)
                    return NotFound("Contenido no encontrado.");

                // Procesar JSON existente
                dynamic? jsonData = null;
                try
                {
                    if (!string.IsNullOrEmpty(contenido.JsonContenido))
                        jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(contenido.JsonContenido);
                }
                catch { }

                if (jsonData == null)
                    jsonData = new Newtonsoft.Json.Linq.JObject();

                string jsonRespuestas = respuestas.ToString();
                var jObj = jsonData is Newtonsoft.Json.Linq.JObject obj ? obj : new Newtonsoft.Json.Linq.JObject();
                jObj["respuestas"] = Newtonsoft.Json.Linq.JArray.Parse(jsonRespuestas);

                contenido.JsonContenido = jObj.ToString();
                _context.Update(contenido);

                // Registrar progreso
                var progreso = await _context.ProgresoContenido
                    .FirstOrDefaultAsync(p => p.IdContenido == idContenido && p.IdEstudiante == idEstudiante);

                if (progreso == null)
                {
                    progreso = new ProgresoContenido
                    {
                        IdContenido = idContenido,
                        IdEstudiante = idEstudiante,
                        Completado = true,
                        FechaCompletado = DateTime.UtcNow,
                        Peso = 1.0
                    };
                    _context.ProgresoContenido.Add(progreso);
                }

                await _context.SaveChangesAsync();

                // 🔁 Actualizar progreso de módulo
                await ActualizarProgresoModuloAsync(contenido.IdModulo, idEstudiante);

                return Json(new { success = true, message = "Respuestas guardadas y progreso actualizado." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error interno: {ex.Message}" });
            }
        }
        // ============================================================
        // 🔁 ACTUALIZAR PROGRESO DEL MÓDULO (todo contenido cuenta)
        // ============================================================
        private async Task ActualizarProgresoModuloAsync(int idModulo, int idEstudiante)
        {
            // 1️⃣ Total de contenidos del módulo (tareas + lecturas + evaluaciones)
            int totalContenidos = await _context.Contenido
                .CountAsync(c => c.IdModulo == idModulo);

            // 2️⃣ Total de contenidos completados por el estudiante
            int contenidosCompletados = await _context.ProgresoContenido
                .CountAsync(p => p.Contenido.IdModulo == idModulo && p.IdEstudiante == idEstudiante);

            // 3️⃣ Calcular porcentaje de avance (cada contenido vale lo mismo)
            double porcentaje = totalContenidos > 0
                ? Math.Round((contenidosCompletados * 100.0) / totalContenidos, 2)
                : 0;

            // 4️⃣ Buscar o crear registro de progreso del módulo
            var progresoModulo = await _context.ProgresoModulo
                .FirstOrDefaultAsync(pm => pm.IdModulo == idModulo && pm.IdEstudiante == idEstudiante);

            if (progresoModulo == null)
            {
                progresoModulo = new ProgresoModulo
                {
                    IdModulo = idModulo,
                    IdEstudiante = idEstudiante,
                    PorcentajeAvance = porcentaje,
                    TareasCompletadas = contenidosCompletados, 
                    TotalTareas = totalContenidos,
                    UltimaActualizacion = DateTime.UtcNow,
                    Completado = porcentaje >= 100
                };
                _context.ProgresoModulo.Add(progresoModulo);
            }
            else
            {
                progresoModulo.PorcentajeAvance = porcentaje;
                progresoModulo.TareasCompletadas = contenidosCompletados;
                progresoModulo.TotalTareas = totalContenidos;
                progresoModulo.UltimaActualizacion = DateTime.UtcNow;
                progresoModulo.Completado = porcentaje >= 100;

                _context.ProgresoModulo.Update(progresoModulo);
            }

            await _context.SaveChangesAsync();
        }


        // ============================================================
        // 📊 ENDPOINT PARA DASHBOARD DE PROGRESO POR USUARIO
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetProgresoModulos(int idEstudiante)
        {
            var modulos = await _context.ProgresoModulo
                .Include(pm => pm.Modulo)
                .ThenInclude(m => m.Curso)
                .Where(pm => pm.IdEstudiante == idEstudiante)
                .Select(pm => new
                {
                    pm.IdModulo,
                    Modulo = pm.Modulo.Titulo,
                    Curso = pm.Modulo.Curso.Titulo,
                    pm.PorcentajeAvance,
                    pm.TareasCompletadas,
                    pm.TotalTareas,
                    pm.Completado,
                    pm.UltimaActualizacion
                })
                .ToListAsync();

            return Json(modulos);
        }

        // ============================================================
        // 📊 DASHBOARD DE PROGRESO DE CURSOS (para el estudiante)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetProgresoCursos(int idEstudiante)
        {
            // 🔹 Obtener todos los cursos en los que el estudiante está inscrito
            var cursosInscritos = await _context.Inscripcion
                .Include(i => i.Curso)
                .Where(i => i.IdEstudiante == idEstudiante)
                .Select(i => i.Curso)
                .ToListAsync();

            var progresoCursos = new List<object>();

            foreach (var curso in cursosInscritos)
            {
                // 🔹 Obtener módulos del curso
                var modulos = await _context.Modulo
                    .Where(m => m.IdCurso == curso.IdCurso)
                    .ToListAsync();

                int totalModulos = modulos.Count;
                int modulosCompletados = 0;
                double sumaPorcentajes = 0;

                foreach (var modulo in modulos)
                {
                    var progresoModulo = await _context.ProgresoModulo
                        .FirstOrDefaultAsync(pm => pm.IdModulo == modulo.IdModulo && pm.IdEstudiante == idEstudiante);

                    if (progresoModulo != null)
                    {
                        sumaPorcentajes += progresoModulo.PorcentajeAvance;
                        if (progresoModulo.Completado)
                            modulosCompletados++;
                    }
                }

                // 🔹 Promedio del progreso total del curso
                double progresoCurso = totalModulos > 0 ? Math.Round(sumaPorcentajes / totalModulos, 2) : 0;

                progresoCursos.Add(new
                {
                    curso.IdCurso,
                    Curso = curso.Titulo,
                    curso.Nivel,
                    curso.Descripcion,
                    TotalModulos = totalModulos,
                    ModulosCompletados = modulosCompletados,
                    PorcentajeAvance = progresoCurso,
                    Completado = progresoCurso >= 100
                });
            }

            return Json(progresoCursos);
        }


    }
}
