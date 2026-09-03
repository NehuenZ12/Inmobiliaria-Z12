using System.ComponentModel.DataAnnotations.Schema;

namespace mvc.Models
{
    public class Reserva
    {
        public int Id { get; set; }

        public int UsuarioCreadorId { get; set; }

        public int? UsuarioTerminadorId { get; set; }

        // Auxiliares para mostrar auditoría en la vista de detalles
        [NotMapped]
        public string? NombreUsuarioCreador { get; set; }

        [NotMapped]
        public string? NombreUsuarioTerminador { get; set; }
    }
}
