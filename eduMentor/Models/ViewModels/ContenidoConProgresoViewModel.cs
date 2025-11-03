namespace eduMentor.Models.ViewModels
{
    public class ContenidoConProgresoViewModel
    {
        public int IdContenido { get; set; }
        public int IdModulo { get; set; }
        public string Titulo { get; set; }
        public string Tipo { get; set; }
        public string JsonContenido { get; set; }
        public bool EsTarea { get; set; }
        public double Peso { get; set; }
        public int Orden { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaProgreso { get; set; } // <- nuevo campo
    }
}
