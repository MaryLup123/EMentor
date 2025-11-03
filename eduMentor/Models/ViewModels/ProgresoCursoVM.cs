using System.Collections.Generic;

namespace eduMentor.Models.ViewModels
{
    public class ModuloProgresoVM
    {
        public int IdModulo { get; set; }
        public string Titulo { get; set; }
        public double Porcentaje { get; set; }
        public bool Completado { get; set; }
    }

    public class CursoProgresoVM
    {
        public int IdCurso { get; set; }
        public string Titulo { get; set; }
        public string Nivel { get; set; }
        public string Descripcion { get; set; }
        public int TotalModulos { get; set; }
        public int ModulosCompletados { get; set; }
        public double PorcentajeAvance { get; set; }
        public bool Completado { get; set; }
        public List<ModuloProgresoVM> Modulos { get; set; } = new();
    }
}
