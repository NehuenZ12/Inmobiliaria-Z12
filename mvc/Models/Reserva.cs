using System.ComponentModel.DataAnnotations;

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

    // Se completa solo si la reserva se termina antes de la fecha original
    [DataType(DataType.Date)]
    public DateTime? FechaTerminacion { get; set; }

    public int UsuarioCreadorId { get; set; }
    public int? UsuarioTerminadorId { get; set; }

    // Auxiliares para mostrar auditoría en la vista de detalles
    public string? NombreUsuarioCreador { get; set; }
    public string? NombreUsuarioTerminador { get; set; }
  }
}