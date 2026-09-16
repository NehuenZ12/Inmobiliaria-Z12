namespace mvc.Models.ViewModels
{
    public class InmuebleReservaViewModel
    {
        public int Id { get; set; }

        public string Direccion { get; set; } = "";

        public string Propietario { get; set; } = "";

        public int CantidadReservas { get; set; }
    }
}