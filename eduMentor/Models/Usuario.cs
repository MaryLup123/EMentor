using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace eduMentor.Models
{
    // 🔹 Asegúrate de heredar exactamente de IdentityUser<int>
    public class Usuario : IdentityUser<int>
    {
        public string Nombre { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public DateTime? UltimoLogin { get; set; }

        public virtual ICollection<Curso> Cursos { get; set; } = new List<Curso>();
        public virtual ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
        public virtual ICollection<ProgresoContenido> ProgresosContenidos { get; set; } = new List<ProgresoContenido>();
        public virtual ICollection<ProgresoModulo> Progresos { get; set; } = new List<ProgresoModulo>();
        public virtual ICollection<Certificado> Certificados { get; set; } = new List<Certificado>();
    }
}
