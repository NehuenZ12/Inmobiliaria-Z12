using System.ComponentModel.DataAnnotations;

namespace mvc.Models
{
    public class TipoInmueble
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = "";
    }
}