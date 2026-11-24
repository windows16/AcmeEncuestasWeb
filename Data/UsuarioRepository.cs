using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using AcmeEncuestasWeb.Models;

namespace AcmeEncuestasWeb.Data
{
    /// <summary>
    /// Repositorio de Acceso a Datos para Usuarios.
    /// Utiliza ADO.NET optimizado con consultas parametrizadas para máxima velocidad
    /// y protección total contra inyecciones SQL (SQL Injection).
    /// </summary>
    public class UsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository()
        {
            var cnn = ConfigurationManager.ConnectionStrings["AcmeEncuestasConnection"];
            _connectionString = cnn != null ? cnn.ConnectionString : string.Empty;
        }

        public UsuarioRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Obtiene un usuario activo por su Username.
        /// Retorna null si el usuario no existe o está inactivo.
        /// </summary>
        public Usuario ObtenerPorUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            const string sql = @"
                SELECT UsuarioId, NombreCompleto, Username, Email, PasswordHash, PasswordSalt, EsActivo, FechaCreacion, UltimoLogin
                FROM dbo.Usuarios WITH (NOLOCK)
                WHERE Username = @Username AND EsActivo = 1;";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = username.Trim() });

                connection.Open();
                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        return new Usuario
                        {
                            UsuarioId = reader.GetInt32(0),
                            NombreCompleto = reader.GetString(1),
                            Username = reader.GetString(2),
                            Email = reader.GetString(3),
                            PasswordHash = reader.GetString(4),
                            PasswordSalt = reader.GetString(5),
                            EsActivo = reader.GetBoolean(6),
                            FechaCreacion = reader.GetDateTime(7),
                            UltimoLogin = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8)
                        };
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Actualiza la fecha y hora del último inicio de sesión exitoso.
        /// </summary>
        public void RegistrarUltimoLogin(int usuarioId)
        {
            const string sql = @"
                UPDATE dbo.Usuarios
                SET UltimoLogin = SYSDATETIME()
                WHERE UsuarioId = @UsuarioId;";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Registra un nuevo usuario con su Salt y Hash calculados.
        /// </summary>
        public bool CrearUsuario(Usuario usuario)
        {
            const string sql = @"
                INSERT INTO dbo.Usuarios (NombreCompleto, Username, Email, PasswordHash, PasswordSalt, EsActivo)
                VALUES (@NombreCompleto, @Username, @Email, @PasswordHash, @PasswordSalt, 1);";

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.Add(new SqlParameter("@NombreCompleto", SqlDbType.NVarChar, 150) { Value = usuario.NombreCompleto });
                command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = usuario.Username.Trim() });
                command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = usuario.Email.Trim() });
                command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.VarChar, 128) { Value = usuario.PasswordHash });
                command.Parameters.Add(new SqlParameter("@PasswordSalt", SqlDbType.VarChar, 128) { Value = usuario.PasswordSalt });

                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
        }
    }
}
