using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eduMentor.Models
{
    public class Modulo
    {
        [Key]
        public int IdModulo { get; set; }

        [Required, ForeignKey("Curso")]
        public int IdCurso { get; set; }

        [Required, MaxLength(150)]
        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public int Orden { get; set; }

        public int DuracionMinutos { get; set; } = 10;

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public virtual Curso Curso { get; set; }

        public virtual ICollection<Contenido> Contenidos { get; set; } = new List<Contenido>();
        public virtual ICollection<ProgresoContenido> ProgresosContenidos { get; set; } = new List<ProgresoContenido>();

        public virtual ICollection<ProgresoModulo> Progresos { get; set; } = new List<ProgresoModulo>();
    }
}
