using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc.Models
{
  public class Reserva
  {
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un inquilino")]
    public int InquilinoId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un inmueble")]
    public int InmuebleId { get; set; }

    [Required(ErrorMessage = "La fecha desde es obligatoria")]
    [DataType(DataType.Date)]
    public DateTime FechaDesde { get; set; }

    [Required(ErrorMessage = "La fecha hasta es obligatoria")]
    [DataType(DataType.Date)]
    public DateTime FechaHasta { get; set; }

    [Required(ErrorMessage = "El monto diario es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto diario debe ser mayor a cero")]
    public decimal MontoDiario { get; set; }
    [Required(ErrorMessage = "La cantidad de personas es obligatoria")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe ser al menos 1 persona")]
    public int CantidadPersonas { get; set; }

    [DataType(DataType.Date)]
    public DateTime? FechaTerminacion { get; set; }

    public int UsuarioCreadorId { get; set; }
    public int? UsuarioTerminadorId { get; set; }

    [NotMapped]
    public string? NombreUsuarioCreador { get; set; }

    [NotMapped]
    public string? NombreUsuarioTerminador { get; set; }

    [NotMapped]
    public string? NombreInquilino { get; set; }

    [NotMapped]
    public string? DireccionInmueble { get; set; }

    public decimal ImporteTotal => (decimal)(FechaHasta - FechaDesde).TotalDays * MontoDiario;
  }
}