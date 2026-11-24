using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using AcmeEncuestasWeb.Models;

namespace AcmeEncuestasWeb.Data
{
    
    public class EncuestaRepository
    {
        private readonly string _connectionString;

        public EncuestaRepository()
        {
            var cnn = ConfigurationManager.ConnectionStrings["AcmeEncuestasConnection"];
            _connectionString = cnn != null ? cnn.ConnectionString : string.Empty;
        }

        public EncuestaRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Obtiene todas las encuestas activas de un usuario.
        /// </summary>
        public List<Encuesta> ObtenerPorUsuario(int usuarioId)
        {
            var lista = new List<Encuesta>();

            const string sql = @"
                SELECT 
                    e.EncuestaId,
                    e.UsuarioId,
                    e.Nombre,
                    e.Descripcion,
                    e.LinkUnico,
                    e.EsActiva,
                    e.FechaCreacion,
                    e.FechaModificacion,
                    (SELECT COUNT(1) FROM dbo.CamposEncuesta c WHERE c.EncuestaId = e.EncuestaId) AS TotalCampos,
                    (SELECT COUNT(1) FROM dbo.RespuestasEncuesta r WHERE r.EncuestaId = e.EncuestaId) AS TotalRespuestas
                FROM dbo.Encuestas e WITH (NOLOCK)
                WHERE e.UsuarioId = @UsuarioId AND e.EsActiva = 1
                ORDER BY e.FechaCreacion DESC;";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var encuesta = new Encuesta
                        {
                            EncuestaId = reader.GetInt32(0),
                            UsuarioId = reader.GetInt32(1),
                            Nombre = reader.GetString(2),
                            Descripcion = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                            LinkUnico = reader.GetString(4),
                            EsActiva = reader.GetBoolean(5),
                            FechaCreacion = reader.GetDateTime(6),
                            FechaModificacion = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7)
                        };

                        lista.Add(encuesta);
                    }
                }
            }

            return lista;
        }

        /// <summary>
        /// Obtiene el detalle completo de una encuesta por su id y el id de usuario.
        /// </summary>
        public Encuesta ObtenerPorId(int encuestaId, int usuarioId)
        {
            Encuesta encuesta = null;

            const string sqlEncuesta = @"
                SELECT EncuestaId, UsuarioId, Nombre, Descripcion, LinkUnico, EsActiva, FechaCreacion, FechaModificacion
                FROM dbo.Encuestas WITH (NOLOCK)
                WHERE EncuestaId = @EncuestaId AND UsuarioId = @UsuarioId AND EsActiva = 1;";

            const string sqlCampos = @"
                SELECT c.CampoId, c.EncuestaId, c.NombreCampo, c.TituloCampo, c.EsRequerido, c.TipoCampoId, t.Nombre AS TipoCampoNombre, c.Orden
                FROM dbo.CamposEncuesta c WITH (NOLOCK)
                INNER JOIN dbo.TiposCampo t WITH (NOLOCK) ON c.TipoCampoId = t.TipoCampoId
                WHERE c.EncuestaId = @EncuestaId
                ORDER BY c.Orden ASC, c.CampoId ASC;";

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand(sqlEncuesta, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@EncuestaId", SqlDbType.Int) { Value = encuestaId });
                    cmd.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

                    using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.Read())
                        {
                            encuesta = new Encuesta
                            {
                                EncuestaId = reader.GetInt32(0),
                                UsuarioId = reader.GetInt32(1),
                                Nombre = reader.GetString(2),
                                Descripcion = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                LinkUnico = reader.GetString(4),
                                EsActiva = reader.GetBoolean(5),
                                FechaCreacion = reader.GetDateTime(6),
                                FechaModificacion = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7)
                            };
                        }
                    }
                }

                if (encuesta != null)
                {
                    using (var cmdCampos = new SqlCommand(sqlCampos, conn))
                    {
                        cmdCampos.Parameters.Add(new SqlParameter("@EncuestaId", SqlDbType.Int) { Value = encuestaId });

                        using (var reader = cmdCampos.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                encuesta.Campos.Add(new CampoEncuesta
                                {
                                    CampoId = reader.GetInt32(0),
                                    EncuestaId = reader.GetInt32(1),
                                    NombreCampo = reader.GetString(2),
                                    TituloCampo = reader.GetString(3),
                                    EsRequerido = reader.GetBoolean(4),
                                    TipoCampoId = reader.GetByte(5),
                                    TipoCampoNombre = reader.GetString(6),
                                    Orden = reader.GetInt32(7)
                                });
                            }
                        }
                    }
                }
            }

            return encuesta;
        }

        public int Crear(Encuesta encuesta)
        {
            if (string.IsNullOrWhiteSpace(encuesta.LinkUnico))
            {
                encuesta.LinkUnico = Guid.NewGuid().ToString();
            }

            const string sqlInsertEncuesta = @"
                INSERT INTO dbo.Encuestas (UsuarioId, Nombre, Descripcion, LinkUnico, EsActiva, FechaCreacion)
                VALUES (@UsuarioId, @Nombre, @Descripcion, @LinkUnico, 1, SYSDATETIME());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            const string sqlInsertCampo = @"
                INSERT INTO dbo.CamposEncuesta (EncuestaId, NombreCampo, TituloCampo, EsRequerido, TipoCampoId, Orden)
                VALUES (@EncuestaId, @NombreCampo, @TituloCampo, @EsRequerido, @TipoCampoId, @Orden);";

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        int nuevoId;

                        using (var cmd = new SqlCommand(sqlInsertEncuesta, conn, trans))
                        {
                            cmd.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = encuesta.UsuarioId });
                            cmd.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.NVarChar, 200) { Value = encuesta.Nombre.Trim() });
                            cmd.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.NVarChar, 500) { Value = (object)encuesta.Descripcion?.Trim() ?? DBNull.Value });
                            cmd.Parameters.Add(new SqlParameter("@LinkUnico", SqlDbType.VarChar, 36) { Value = encuesta.LinkUnico });

                            nuevoId = (int)cmd.ExecuteScalar();
                        }

                        if (encuesta.Campos != null && encuesta.Campos.Count > 0)
                        {
                            int ordenIndex = 1;
                            foreach (var campo in encuesta.Campos)
                            {
                                using (var cmdCampo = new SqlCommand(sqlInsertCampo, conn, trans))
                                {
                                    cmdCampo.Parameters.Add(new SqlParameter("@EncuestaId", SqlDbType.Int) { Value = nuevoId });
                                    cmdCampo.Parameters.Add(new SqlParameter("@NombreCampo", SqlDbType.NVarChar, 100) { Value = campo.NombreCampo.Trim() });
                                    cmdCampo.Parameters.Add(new SqlParameter("@TituloCampo", SqlDbType.NVarChar, 200) { Value = campo.TituloCampo.Trim() });
                                    cmdCampo.Parameters.Add(new SqlParameter("@EsRequerido", SqlDbType.Bit) { Value = campo.EsRequerido });
                                    cmdCampo.Parameters.Add(new SqlParameter("@TipoCampoId", SqlDbType.TinyInt) { Value = campo.TipoCampoId });
                                    cmdCampo.Parameters.Add(new SqlParameter("@Orden", SqlDbType.Int) { Value = ordenIndex++ });

                                    cmdCampo.ExecuteNonQuery();
                                }
                            }
                        }

                        trans.Commit();
                        return nuevoId;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool Actualizar(Encuesta encuesta)
        {
            const string sqlUpdateEncuesta = @"
                UPDATE dbo.Encuestas
                SET Nombre = @Nombre,
                    Descripcion = @Descripcion,
                    FechaModificacion = SYSDATETIME()
                WHERE EncuestaId = @EncuestaId AND UsuarioId = @UsuarioId;";

            const string sqlDeleteCampos = @"
                DELETE FROM dbo.CamposEncuesta
                WHERE EncuestaId = @EncuestaId;";

            const string sqlInsertCampo = @"
                INSERT INTO dbo.CamposEncuesta (EncuestaId, NombreCampo, TituloCampo, EsRequerido, TipoCampoId, Orden)
                VALUES (@EncuestaId, @NombreCampo, @TituloCampo, @EsRequerido, @TipoCampoId, @Orden);";

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        int filasAfectadas;

                        using (var cmd = new SqlCommand(sqlUpdateEncuesta, conn, trans))
                        {
                            cmd.Parameters.Add(new SqlParameter("@EncuestaId", SqlDbType.Int) { Value = encuesta.EncuestaId });
                            cmd.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = encuesta.UsuarioId });
                            cmd.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.NVarChar, 200) { Value = encuesta.Nombre.Trim() });
                            cmd.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.NVarChar, 500) { Value = (object)encuesta.Descripcion?.Trim() ?? DBNull.Value });

                            filasAfectadas = cmd.ExecuteNonQuery();
                        }

                        if (filasAfectadas == 0)
                        {
                            trans.Rollback();
                            return false;
                        }

                        // Reemplazar campos dinámicos
                        using (var cmdDel = new SqlCommand(sqlDeleteCampos, conn, trans))
                        {
                            cmdDel.Parameters.Add(new SqlParameter("@EncuestaId", SqlDbType.Int) { Value = encuesta.EncuestaId });
                            cmdDel.ExecuteNonQuery();
                        }

                        if (encuesta.Campos != null && encuesta.Campos.Count > 0)
                        {
                            int ordenIndex = 1;
                            foreach (var campo in encuesta.Campos)
                            {
                                using (var cmdCampo = new SqlCommand(sqlInsertCampo, conn, trans))
                                {
                                    cmdCampo.Parameters.Add(new SqlParameter("@EncuestaId", SqlDbType.Int) { Value = encuesta.EncuestaId });
                                    cmdCampo.Parameters.Add(new SqlParameter("@NombreCampo", SqlDbType.NVarChar, 100) { Value = campo.NombreCampo.Trim() });
                                    cmdCampo.Parameters.Add(new SqlParameter("@TituloCampo", SqlDbType.NVarChar, 200) { Value = campo.TituloCampo.Trim() });
                                    cmdCampo.Parameters.Add(new SqlParameter("@EsRequerido", SqlDbType.Bit) { Value = campo.EsRequerido });
                                    cmdCampo.Parameters.Add(new SqlParameter("@TipoCampoId", SqlDbType.TinyInt) { Value = campo.TipoCampoId });
                                    cmdCampo.Parameters.Add(new SqlParameter("@Orden", SqlDbType.Int) { Value = ordenIndex++ });

                                    cmdCampo.ExecuteNonQuery();
                                }
                            }
                        }

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool Eliminar(int encuestaId, int usuarioId)
        {
            const string sql = @"
                DELETE FROM dbo.Encuestas
                WHERE EncuestaId = @EncuestaId AND UsuarioId = @UsuarioId;";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@EncuestaId", SqlDbType.Int) { Value = encuestaId });
                cmd.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
