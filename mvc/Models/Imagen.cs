namespace mvc.Models
{
    public class Imagen
    {
        public int Id { get; set; }

        public string Url { get; set; } = "";

        public string? Descripcion { get; set; }

        public bool EsPrincipal { get; set; }

        // Clave foranea
        public int InmuebleId { get; set; }

        // Inmueble asociado
        public Inmueble? Inmueble { get; set; }
    }
}