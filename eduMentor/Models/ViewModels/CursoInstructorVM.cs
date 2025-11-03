using System;
using System.Collections.Generic;

namespace eduMentor.Models.ViewModels
{
    public class CursoInstructorVM
    {
        public int IdCurso { get; set; }
        public string Titulo { get; set; }
        public string Nivel { get; set; }
        public string Descripcion { get; set; }

        public int TotalEstudiantes { get; set; }
        public int EstudiantesCompletaron { get; set; }
        public int EstudiantesEnProgreso { get; set; }
        public int EstudiantesSinIniciar { get; set; }
        public double PromedioAvance { get; set; }

        public List<ModuloInstructorVM> Modulos { get; set; } = new();
    }

    public class ModuloInstructorVM
    {
        public int IdModulo { get; set; }
        public string Titulo { get; set; }
        public double PromedioAvance { get; set; }
    }
}
