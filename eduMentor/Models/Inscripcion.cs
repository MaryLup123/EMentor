using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace eduMentor.Models
{
    public class Inscripcion
    {
        [Key]
        public int IdInscripcion { get; set; }

        [Required, ForeignKey("Curso")]
        public int IdCurso { get; set; }

        [Required, ForeignKey("Estudiante")]
        public int IdEstudiante { get; set; }

        public DateTime FechaInscripcion { get; set; } = DateTime.Now;

        [MaxLength(20)]
        public string Estado { get; set; } = "Activa"; // Activa, Completada, Cancelada

        // Relaciones
        public virtual Curso Curso { get; set; }
        public virtual Usuario Estudiante { get; set; }

    }
}
