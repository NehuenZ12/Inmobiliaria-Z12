namespace mvc.Models
{
    // Contrato de operaciones sobre pagos
    public interface IRepositorioPago
    {
        // LISTAR TODOS LOS PAGOS
        Task<IList<Pago>> ObtenerTodos();

        // BUSCAR PAGO POR ID
        Task<Pago?> ObtenerPorId(int id);

        // LISTAR PAGOS DE UNA RESERVA
        Task<IList<Pago>> ListarPorReserva(int reservaId);

        // CREAR UN PAGO
        Task<int> Crear(Pago pago);

        // EDITAR SOLO EL CONCEPTO DE UN PAGO
        Task EditarConcepto(int id, string concepto);

        // ANULAR UN PAGO (BAJA LÓGICA)
        Task Anular(int id, int usuarioAnuladorId);
    }
}
