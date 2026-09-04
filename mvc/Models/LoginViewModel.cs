using System.ComponentModel.DataAnnotations;

namespace mvc.Models
{
    // Datos que pide la pantalla de login
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La clave es obligatoria")]
        [DataType(DataType.Password)]
        public string Clave { get; set; } = "";
    }
}
