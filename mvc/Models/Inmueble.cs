using System.ComponentModel.DataAnnotations;

namespace mvc.Models
{
    public class Inmueble
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La direccion es obligatoria")]
        public string Direccion { get; set; } = "";

        [Range(1, int.MaxValue, ErrorMessage = "El cupo debe ser mayor a 0")]
        public int Cupo { get; set; }

        [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90")]
        public decimal? Latitud { get; set; }

        [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180")]
        public decimal? Longitud { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PrecioPorDia { get; set; }

        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
        public decimal PorcentajeReserva { get; set; }

        public bool Disponible { get; set; } = true;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un propietario")]
        public int PropietarioId { get; set; }

        public Propietario? Propietario { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un tipo")]
        public int TipoId { get; set; }

        public TipoInmueble? TipoInmueble { get; set; }
    }
}
