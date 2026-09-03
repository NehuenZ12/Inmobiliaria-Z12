using Npgsql;
using System.Data;

namespace mvc.Models
{
    public class RepositorioUsuario : IRepositorioUsuario
    {
        private readonly string _connectionString;

        // Columnas que siempre traemos de la tabla usuario
        private const string Columnas = "id_usuario, nombre, apellido, email, clave, avatar, rol";

        // Leemos la cadena de conexión desde appsettings
        public RepositorioUsuario(IConfiguration configuration)
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

        // LISTAR TODOS LOS USUARIOS
        public async Task<IList<Usuario>> ObtenerTodos()
        {
            var lista = new List<Usuario>();

            // Usamos using para cerrar la conexión automáticamente
            using var connection = await CrearConexionAsync();
            using var command = new NpgsqlCommand(
                $"SELECT {Columnas} FROM usuario ORDER BY apellido, nombre",
                connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(Mapear(reader));
            }

            return lista;
        }

        // BUSCAR UN USUARIO POR ID
        public async Task<Usuario?> ObtenerPorId(int id)
        {
            using var connection = await CrearConexionAsync();
            using var command = new NpgsqlCommand(
                $"SELECT {Columnas} FROM usuario WHERE id_usuario = @id",
                connection);
            command.Parameters.AddWithValue("@id", id);

            return await ObtenerUnoAsync(command);
        }

        // BUSCAR UN USUARIO POR EMAIL (login)
        public async Task<Usuario?> ObtenerPorEmail(string email)
        {
            using var connection = await CrearConexionAsync();
            using var command = new NpgsqlCommand(
                $"SELECT {Columnas} FROM usuario WHERE email = @email",
                connection);
            command.Parameters.AddWithValue("@email", email);

            return await ObtenerUnoAsync(command);
        }

        // ALTA O EDICIÓN DE USUARIO
        public async Task<int> Guardar(Usuario usuario)
        {
            using var connection = await CrearConexionAsync();

            if (usuario.Id == 0)
            {
                // ALTA: insertamos todos los campos, incluyendo la clave ya hasheada
                using var command = new NpgsqlCommand(
                    $"INSERT INTO usuario (nombre, apellido, email, clave, avatar, rol) " +
                    $"VALUES (@nombre, @apellido, @email, @clave, @avatar, @rol) " +
                    $"RETURNING id_usuario",
                    connection);

                AgregarParametrosComunes(command, usuario);
                command.Parameters.AddWithValue("@clave", usuario.Clave);

                var result = await command.ExecuteScalarAsync();
                usuario.Id = Convert.ToInt32(result);
            }
            else
            {
                // EDICIÓN: construimos la lista de campos a actualizar
                var campos = new List<string>
                {
                    "nombre = @nombre",
                    "apellido = @apellido",
                    "email = @email",
                    "avatar = @avatar",
                    "rol = @rol"
                };

                using var command = new NpgsqlCommand(
                    $"UPDATE usuario SET {string.Join(", ", campos)} WHERE id_usuario = @id",
                    connection);

                AgregarParametrosComunes(command, usuario);

                // Si la clave viene vacía no la tocamos (el usuario no la cambió)
                if (!string.IsNullOrWhiteSpace(usuario.Clave))
                {
                    command.CommandText =
                        $"UPDATE usuario SET {string.Join(", ", campos)}, clave = @clave WHERE id_usuario = @id";
                    command.Parameters.AddWithValue("@clave", usuario.Clave);
                }

                command.Parameters.AddWithValue("@id", usuario.Id);
                await command.ExecuteNonQueryAsync();
            }

            return usuario.Id;
        }

        // BAJA FÍSICA DE USUARIO
        public async Task Eliminar(int id)
        {
            using var connection = await CrearConexionAsync();
            using var command = new NpgsqlCommand(
                "DELETE FROM usuario WHERE id_usuario = @id",
                connection);
            command.Parameters.AddWithValue("@id", id);

            await command.ExecuteNonQueryAsync();
        }

        // EJECUTAR UNA CONSULTA QUE DEVUELVE COMO MUCHO UN USUARIO
        private static async Task<Usuario?> ObtenerUnoAsync(NpgsqlCommand command)
        {
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return Mapear(reader);
            }

            return null;
        }

        // PARAMETRIZAR LOS CAMPOS COMUNES DE INSERT/UPDATE
        private static void AgregarParametrosComunes(NpgsqlCommand command, Usuario usuario)
        {
            command.Parameters.AddWithValue("@nombre", usuario.Nombre);
            command.Parameters.AddWithValue("@apellido", usuario.Apellido);
            command.Parameters.AddWithValue("@email", usuario.Email);
            command.Parameters.AddWithValue("@avatar", (object?)usuario.Avatar ?? DBNull.Value);
            command.Parameters.AddWithValue("@rol", usuario.Rol);
        }

        // MAPEO LECTOR → OBJETO
        private static Usuario Mapear(NpgsqlDataReader reader)
        {
            return new Usuario
            {
                Id = (int)reader["id_usuario"],
                Nombre = (string)reader["nombre"],
                Apellido = (string)reader["apellido"],
                Email = (string)reader["email"],
                Clave = (string)reader["clave"],
                Avatar = reader.IsDBNull(reader.GetOrdinal("avatar")) ? null : (string)reader["avatar"],
                Rol = (string)reader["rol"]
            };
        }
    }
}
