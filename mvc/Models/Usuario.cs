using System.ComponentModel.DataAnnotations;

namespace mvc.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", ErrorMessage = "El nombre solo puede contener letras")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", ErrorMessage = "El apellido solo puede contener letras")]
        public string Apellido { get; set; } = "";

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato valido")]
        [StringLength(150)]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La clave es obligatoria")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "La clave debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        public string Clave { get; set; } = "";

        [StringLength(255)]
        public string? Avatar { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        [RegularExpression(@"^(Administrador|Empleado)$", ErrorMessage = "El rol debe ser Administrador o Empleado")]
        public string Rol { get; set; } = "Empleado";

        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
    }
}
