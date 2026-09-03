using Npgsql;
using System.Data;

namespace mvc.Models
{
    public class RepositorioPago : IRepositorioPago
    {
        private readonly string _connectionString;

        // Columnas de la tabla pago
        private const string ColumnasPago =
            "p.id, p.fecha, p.concepto, p.importe, p.reserva_id, p.anulado, " +
            "p.usuario_creador_id, p.usuario_anulador_id, p.metodo, p.estado, p.comprobante_url";

        // Leemos la cadena de conexión desde appsettings
        public RepositorioPago(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        // CREAR Y ABRIR CONEXIÓN
        private async Task<NpgsqlConnection> CrearConexionAsync()
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        // LISTAR TODOS LOS PAGOS
        public async Task<IList<Pago>> ObtenerTodos()
        {
            var lista = new List<Pago>();

            using var connection = await CrearConexionAsync();
            using var command = new NpgsqlCommand(
                $@"SELECT {ColumnasPago},
                    u1.nombre || ' ' || u1.apellido AS creador_nombre,
                    u2.nombre || ' ' || u2.apellido AS anulador_nombre
                    FROM pago p
                    LEFT JOIN usuario u1 ON u1.id_usuario = p.usuario_creador_id
                    LEFT JOIN usuario u2 ON u2.id_usuario = p.usuario_anulador_id
                    ORDER BY p.fecha DESC, p.id DESC",
                connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(Mapear(reader));
            }

            return lista;
        }

        // BUSCAR PAGO POR ID
        public async Task<Pago?> ObtenerPorId(int id)
        {
            using var connection = await CrearConexionAsync();
            using var command = new NpgsqlCommand(
                $@"SELECT {ColumnasPago},
                    u1.nombre || ' ' || u1.apellido AS creador_nombre,
                    u2.nombre || ' ' || u2.apellido AS anulador_nombre
                    FROM pago p
                    LEFT JOIN usuario u1 ON u1.id_usuario = p.usuario_creador_id
                    LEFT JOIN usuario u2 ON u2.id_usuario = p.usuario_anulador_id
                    WHERE p.id = @id",
                connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return Mapear(reader);
            }

            return null;
        }

        // LISTAR PAGOS DE UNA RESERVA
        public async Task<IList<Pago>> ListarPorReserva(int reservaId)
        {
            var lista = new List<Pago>();

            using var connection = await CrearConexionAsync();
            using var command = new NpgsqlCommand(
                $@"SELECT {ColumnasPago},
                    u1.nombre || ' ' || u1.apellido AS creador_nombre,
                    u2.nombre || ' ' || u2.apellido AS anulador_nombre
                    FROM pago p
                    LEFT JOIN usuario u1 ON u1.id_usuario = p.usuario_creador_id
                    LEFT JOIN usuario u2 ON u2.id_usuario = p.usuario_anulador_id
                    WHERE p.reserva_id = @reserva_id
                    ORDER BY p.fecha DESC, p.id DESC",
                connection);
            command.Parameters.AddWithValue("@reserva_id", reservaId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(Mapear(reader));
            }

            return lista;
        }

        // CREAR UN PAGO
        public async Task<int> Crear(Pago pago)
        {
            using var connection = await CrearConexionAsync();
            using var command = new NpgsqlCommand(
                @"INSERT INTO pago (fecha, concepto, importe, reserva_id, usuario_creador_id, metodo, estado, comprobante_url)
                  VALUES (@fecha, @concepto, @importe, @reserva_id, @usuario_creador_id, @metodo, @estado, @comprobante_url)
                  RETURNING id",
                connection);

            command.Parameters.AddWithValue("@fecha", pago.Fecha);
            command.Parameters.AddWithValue("@concepto", pago.Concepto);
            command.Parameters.AddWithValue("@importe", pago.Importe);
            command.Parameters.AddWithValue("@reserva_id", pago.ReservaId);
            command.Parameters.AddWithValue("@usuario_creador_id", pago.UsuarioCreadorId);
            command.Parameters.AddWithValue("@metodo", pago.Metodo.ToString());
            command.Parameters.AddWithValue("@estado", pago.Estado.ToString());
            command.Parameters.AddWithValue("@comprobante_url", (object?)pago.ComprobanteUrl ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            pago.Id = Convert.ToInt32(result);

            return pago.Id;
        }

        // EDITAR SOLO EL CONCEPTO (no se tocan importe ni fecha)
        public async Task EditarConcepto(int id, string concepto)
        {
            using var connection = await CrearConexionAsync();
            using var command = new NpgsqlCommand(
                "UPDATE pago SET concepto = @concepto WHERE id = @id",
                connection);

            command.Parameters.AddWithValue("@concepto", concepto);
            command.Parameters.AddWithValue("@id", id);

            await command.ExecuteNonQueryAsync();
        }

        // ANULAR PAGO: baja lógica + auditoría
        public async Task Anular(int id, int usuarioAnuladorId)
        {
            using var connection = await CrearConexionAsync();
            using var command = new NpgsqlCommand(
                @"UPDATE pago
                  SET anulado = true,
                      usuario_anulador_id = @usuario_anulador_id
                  WHERE id = @id",
                connection);

            command.Parameters.AddWithValue("@usuario_anulador_id", usuarioAnuladorId);
            command.Parameters.AddWithValue("@id", id);

            await command.ExecuteNonQueryAsync();
        }

        // MAPEO LECTOR → OBJETO
        private static Pago Mapear(NpgsqlDataReader reader)
        {
            return new Pago
            {
                Id = (int)reader["id"],
                Fecha = (DateTime)reader["fecha"],
                Concepto = (string)reader["concepto"],
                Importe = (decimal)reader["importe"],
                ReservaId = (int)reader["reserva_id"],
                Anulado = (bool)reader["anulado"],
                UsuarioCreadorId = (int)reader["usuario_creador_id"],
                UsuarioAnuladorId = reader.IsDBNull(reader.GetOrdinal("usuario_anulador_id"))
                    ? null
                    : (int)reader["usuario_anulador_id"],
                Metodo = Enum.Parse<MetodoPago>((string)reader["metodo"]),
                Estado = Enum.Parse<EstadoPago>((string)reader["estado"]),
                ComprobanteUrl = reader.IsDBNull(reader.GetOrdinal("comprobante_url"))
                    ? null
                    : (string)reader["comprobante_url"],
                NombreUsuarioCreador = reader.IsDBNull(reader.GetOrdinal("creador_nombre"))
                    ? null
                    : (string)reader["creador_nombre"],
                NombreUsuarioAnulador = reader.IsDBNull(reader.GetOrdinal("anulador_nombre"))
                    ? null
                    : (string)reader["anulador_nombre"]
            };
        }
    }
}
