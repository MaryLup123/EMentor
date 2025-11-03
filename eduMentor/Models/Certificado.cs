
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace eduMentor.Models
{
    public class Certificado
    {
        [Key]
        public int IdCertificado { get; set; }

        [Required, ForeignKey("Curso")]
        public int IdCurso { get; set; }

        [Required, ForeignKey("Estudiante")]
        public int IdEstudiante { get; set; }

        public DateTime FechaGeneracion { get; set; } = DateTime.Now;

        [MaxLength(255)]
        public string UrlPDF { get; set; }

        public Guid CodigoVerificacion { get; set; } = Guid.NewGuid();

        // Relaciones
        public virtual Curso Curso { get; set; }
        public virtual Usuario Estudiante { get; set; }
    }
}
