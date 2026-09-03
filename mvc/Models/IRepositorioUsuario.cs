namespace mvc.Models
{
    // Contrato de operaciones sobre usuarios
    public interface IRepositorioUsuario
    {
        // LISTAR
        Task<IList<Usuario>> ObtenerTodos();

        // BUSCAR POR ID
        Task<Usuario?> ObtenerPorId(int id);

        // BUSCAR POR EMAIL (login)
        Task<Usuario?> ObtenerPorEmail(string email);

        // ALTA / EDICIÓN
        Task<int> Guardar(Usuario usuario);

        // BAJA
        Task Eliminar(int id);
    }
}
