using eduMentor.Data;
using eduMentor.Models;
using eduMentor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace eduMentor.Controllers
{
    public class CertificadoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CertificadoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Certificadoes
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int idEstudiante))
                return Unauthorized();

            // 🔹 Obtener todos los certificados del estudiante logueado
            var certificados = await _context.Certificado
                .Include(c => c.Curso)
                .Include(c => c.Estudiante)
                .Where(c => c.IdEstudiante == idEstudiante)
                .OrderByDescending(c => c.FechaGeneracion)
                .ToListAsync();

            var estudiante = await _context.Usuario.FindAsync(idEstudiante);
            ViewBag.NombreEstudiante = estudiante?.Nombre ?? "Estudiante";

            return View("~/Views/Pwa/Certificadoes/Index.cshtml", certificados);
        }

        // GET: Certificadoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificado = await _context.Certificado
                .Include(c => c.Curso)
                .Include(c => c.Estudiante)
                .FirstOrDefaultAsync(m => m.IdCertificado == id);
            if (certificado == null)
            {
                return NotFound();
            }

            return View("~/Views/Pwa/Certificadoes/Details.cshtml", certificado);
        }

        // GET: Certificadoes/Create
        [HttpGet]
        public async Task<IActionResult> Create(int idCurso)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int idEstudiante))
                return Unauthorized();

            // Verificar si ya tiene certificado emitido
            var yaTieneCertificado = await _context.Certificado
                .AnyAsync(c => c.IdCurso == idCurso && c.IdEstudiante == idEstudiante);

            if (yaTieneCertificado)
            {
                TempData["Info"] = "Ya se ha generado un certificado para este curso.";
                return RedirectToAction("Index", "Cursos");
            }

            var curso = await _context.Curso.FirstOrDefaultAsync(c => c.IdCurso == idCurso);
            if (curso == null)
                return NotFound();

            var certificado = new Certificado
            {
                IdCurso = idCurso,
                IdEstudiante = idEstudiante,
                FechaGeneracion = DateTime.UtcNow,
                UrlPDF = $"/certificados/{Guid.NewGuid()}.pdf" 
            };

            _context.Add(certificado);
            await _context.SaveChangesAsync();

            ViewBag.NombreCurso = curso.Titulo;
            ViewBag.NombreEstudiante = (await _context.Usuario.FindAsync(idEstudiante))?.Nombre ?? "Estudiante";

            return View("~/Views/Pwa/Certificadoes/Create.cshtml", certificado);
        }


        // GET: Certificadoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificado = await _context.Certificado.FindAsync(id);
            if (certificado == null)
            {
                return NotFound();
            }
            ViewData["IdCurso"] = new SelectList(_context.Curso, "IdCurso", "Nivel", certificado.IdCurso);
            ViewData["IdEstudiante"] = new SelectList(_context.Usuario, "Id", "Id", certificado.IdEstudiante);
            return View("~/Views/Pwa/Certificadoes/Edit.cshtml", certificado);
        }

        // POST: Certificadoes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCertificado,IdCurso,IdEstudiante,FechaGeneracion,UrlPDF,CodigoVerificacion")] Certificado certificado)
        {
            if (id != certificado.IdCertificado)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(certificado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CertificadoExists(certificado.IdCertificado))
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
            ViewData["IdCurso"] = new SelectList(_context.Curso, "IdCurso", "Nivel", certificado.IdCurso);
            ViewData["IdEstudiante"] = new SelectList(_context.Usuario, "Id", "Id", certificado.IdEstudiante);
            return View("~/Views/Pwa/Certificadoes/Edit.cshtml", certificado);
        }

        // GET: Certificadoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificado = await _context.Certificado
                .Include(c => c.Curso)
                .Include(c => c.Estudiante)
                .FirstOrDefaultAsync(m => m.IdCertificado == id);
            if (certificado == null)
            {
                return NotFound();
            }

            return View("~/Views/Pwa/Certificadoes/Delete.cshtml", certificado);
        }

        // POST: Certificadoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var certificado = await _context.Certificado.FindAsync(id);
            if (certificado != null)
            {
                _context.Certificado.Remove(certificado);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CertificadoExists(int id)
        {
            return _context.Certificado.Any(e => e.IdCertificado == id);
        }



        // GET: Certificadoes/DescargarPdf/5
        [HttpGet]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            // Buscar certificado (o el curso/estudiante si no tienes certificado aún)
            var certificado = await _context.Certificado
                .Include(c => c.Curso)
                .Include(c => c.Estudiante)
                .FirstOrDefaultAsync(c => c.IdCertificado == id);

            if (certificado == null)
            {
                // Si no existe registro de certificado, podemos generar el PDF usando datos del curso + estudiante
                // Intentamos buscar curso/inscripcion si el id recibido fuese IdCurso en vez de IdCertificado
                var curso = await _context.Curso.FindAsync(id);
                if (curso == null) return NotFound();

                // intentar obtener al estudiante actual (si aplica)
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userId, out int estudianteId)) return Forbid();

                var estudiante = await _context.Usuario.FindAsync(estudianteId);
                if (estudiante == null) return NotFound();

                var docAlt = new CertificateDocument(estudiante.Nombre ?? estudiante.Email, curso.Titulo, DateTime.UtcNow, "eduMentor (Demo)");
                using var msAlt = new MemoryStream();
                docAlt.GeneratePdf(msAlt);
                msAlt.Position = 0;
                var filenameAlt = $"Certificado_{curso.Titulo.Replace(' ', '_')}_{estudiante.Nombre ?? estudiante.Id.ToString()}.pdf";
                return File(msAlt.ToArray(), "application/pdf", filenameAlt);
            }

            // Si existe registro de certificado, usarlo
            var nombreAlumno = certificado.Estudiante?.Nombre ?? certificado.Estudiante?.Email ?? "Estudiante";
            var nombreCurso = certificado.Curso?.Titulo ?? "Curso";
            var fecha = certificado.FechaGeneracion == default ? DateTime.UtcNow : certificado.FechaGeneracion;

            var document = new CertificateDocument(nombreAlumno, nombreCurso, fecha, "eduMentor (Demo)");

            using var ms = new MemoryStream();
            document.GeneratePdf(ms);
            ms.Position = 0;

            var filename = $"Certificado_{nombreCurso.Replace(' ', '_')}_{nombreAlumno.Replace(' ', '_')}.pdf";
            return File(ms.ToArray(), "application/pdf", filename);
        }


    }
}