namespace mvc.Models
{
    public class Reserva
    {
        public int Id { get; set; }

        public int UsuarioCreadorId { get; set; }

        public int? UsuarioTerminadorId { get; set; }

        // Auxiliares para mostrar auditoría en la vista de detalles
        public string? NombreUsuarioCreador { get; set; }

        public string? NombreUsuarioTerminador { get; set; }
    }
}
