using System.ComponentModel.DataAnnotations;

namespace eduMentor.Models.ViewModels
{
    public class UsuarioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Debe tener al menos 6 caracteres")]
        public string Password { get; set; }

        [Phone(ErrorMessage = "Formato de teléfono no válido")]
        public string? PhoneNumber { get; set; }

        public string? Role { get; set; }
        public bool Activo { get; set; } = true;
    }
}
