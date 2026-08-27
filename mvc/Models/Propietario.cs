using System.ComponentModel.DataAnnotations;
namespace mvc.Models
{
  public class Propietario
  {
    public int Id { get; set; }
    [Required(ErrorMessage = " El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", ErrorMessage = "El nombre solo puede contener letras")]
    public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ\s]+$", ErrorMessage = "El apellido solo puede contener letras")]

    public string Apellido { get; set; } = "";
    [Required(ErrorMessage = "El DNI es obligatorio")]
    [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe tener 7 u 8 digitos, sin puntos ni letras")]

    public string Dni { get; set; } = "";
    [RegularExpression(@"^[\d\s()+-]{6,30}$", ErrorMessage = "El telefono solo admite digitos, espacios y los signos + - ( )")]
    public string? Telefono { get; set; }
    [EmailAddress(ErrorMessage = "El email no tiene un formato valido")]
    [StringLength(150)]

    public string? Email { get; set; }
  }
}