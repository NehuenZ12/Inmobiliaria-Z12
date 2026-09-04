using System.ComponentModel.DataAnnotations;

namespace mvc.Models
{
    // Datos que se permiten editar en un pago: solo el concepto
    public class EditarConceptoPagoViewModel
    {
        public int Id { get; set; }

        public int ReservaId { get; set; }

        [Required(ErrorMessage = "El concepto es obligatorio")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El concepto debe tener entre 2 y 200 caracteres")]
        public string Concepto { get; set; } = "";
    }
}
