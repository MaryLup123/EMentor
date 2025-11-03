using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eduMentor.Models
{
    public class Contenido
    {
        [Key]
        public int IdContenido { get; set; }

        [Required, ForeignKey("Modulo")]
        public int IdModulo { get; set; }

        [Required, MaxLength(200)]
        public string Titulo { get; set; }

        [MaxLength(50)]
        public string Tipo { get; set; } = "Lectura";

        [Column(TypeName = "jsonb")] 
        public string JsonContenido { get; set; }

        public bool EsTarea { get; set; } = false;

        public double Peso { get; set; } = 1.0;
        public int Orden { get; set; } = 0;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public virtual ICollection<ProgresoContenido> Progresos { get; set; } = new List<ProgresoContenido>();

        public virtual Modulo Modulo { get; set; }
    }
}
