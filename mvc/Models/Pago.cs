using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace mvc.Models
{
    public class Pago
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El concepto debe tener entre 2 y 200 caracteres")]
        public string Concepto { get; set; } = "";

        [Required(ErrorMessage = "El importe es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El importe debe ser mayor a cero")]
        public decimal Importe { get; set; }

        [Required(ErrorMessage = "La reserva es obligatoria")]
        public int ReservaId { get; set; }

        public bool Anulado { get; set; }

        public int UsuarioCreadorId { get; set; }

        public int? UsuarioAnuladorId { get; set; }

        [Required(ErrorMessage = "El método de pago es obligatorio")]
        public MetodoPago Metodo { get; set; }

        [Required(ErrorMessage = "El estado del pago es obligatorio")]
        public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;

        [StringLength(500, ErrorMessage = "La URL del comprobante no puede superar los 500 caracteres")]
        [Url(ErrorMessage = "La URL del comprobante no es válida")]
        public string? ComprobanteUrl { get; set; }

        // Auxiliares para mostrar auditoría en la vista de detalles
        [NotMapped]
        public string? NombreUsuarioCreador { get; set; }

        [NotMapped]
        public string? NombreUsuarioAnulador { get; set; }
    }
}
