using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Intrinsics.X86;



namespace eduMentor.Models
{
    public class Curso
    {
        [Key]
        public int IdCurso { get; set; }

        [Required]
        [ForeignKey("Instructor")]
        public int IdInstructor { get; set; }

        [Required, MaxLength(150)]
        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        [Required, MaxLength(50)]
        public string Nivel { get; set; } // Básico, Intermedio, Avanzado

        public bool Estado { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public DateTime UltimaActualizacion { get; set; } = DateTime.Now;

        // Relaciones
        public virtual Usuario Instructor { get; set; }
        public virtual ICollection<Modulo> Modulos { get; set; } = new List<Modulo>();
        public virtual ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
        public virtual ICollection<Certificado> Certificados { get; set; } = new List<Certificado>();
    }
}

