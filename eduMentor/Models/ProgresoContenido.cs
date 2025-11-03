using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eduMentor.Models
{
    public class ProgresoContenido
    {
        [Key]
        public int IdProgresoContenido { get; set; }

        [Required, ForeignKey("Contenido")]
        public int IdContenido { get; set; }

        [Required, ForeignKey("Estudiante")]
        public int IdEstudiante { get; set; }

        public bool Completado { get; set; } = false;

        public DateTime FechaCompletado { get; set; } = DateTime.UtcNow;

        public double Peso { get; set; } = 1.0;

        public virtual Contenido Contenido { get; set; }
        public virtual Usuario Estudiante { get; set; }
    }
}
