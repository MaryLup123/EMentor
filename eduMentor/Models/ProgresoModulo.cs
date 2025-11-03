using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eduMentor.Models
{
    public class ProgresoModulo
    {
        [Key]
        public int IdProgreso { get; set; }

        [Required, ForeignKey("Modulo")]
        public int IdModulo { get; set; }

        [Required, ForeignKey("Estudiante")]
        public int IdEstudiante { get; set; }

        public double PorcentajeAvance { get; set; } = 0;

        public int TareasCompletadas { get; set; } = 0;

        public int TotalTareas { get; set; } = 0;

        public DateTime UltimaActualizacion { get; set; } = DateTime.Now;

        public bool Completado { get; set; } = false;

        public virtual Modulo Modulo { get; set; }
        public virtual Usuario Estudiante { get; set; }
    }
}
