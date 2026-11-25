using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using AcmeEncuestasWeb.Models;
using AcmeEncuestasWeb.ViewModels;

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

       
        public List<Encuesta> ObtenerPorUsuario(int usuarioId)
        {
            var lista = new List<Encuesta>();

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("dbo.sp_ObtenerEncuestasPorUsuario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
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
                            FechaModificacion = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                            TotalCampos = reader.GetInt32(8),
                            TotalRespuestas = reader.GetInt32(9)
                        };

                        lista.Add(encuesta);
                    }
                }
            }

            return lista;
        }

    
        public Encuesta ObtenerPorId(int encuestaId, int usuarioId)
        {
            Encuesta encuesta = null;

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("dbo.sp_ObtenerEncuestaPorId", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@EncuestaId", SqlDbType.Int) { Value = encuestaId });
                cmd.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    // Encabezado
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

                    //Campos dinámicos
                    if (encuesta != null && reader.NextResult())
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

            return encuesta;
        }

        
        public int Crear(Encuesta encuesta)
        {
            if (encuesta == null) throw new ArgumentNullException(nameof(encuesta));

            encuesta.EncuestaId = 0; 
            return EjecutarSpGuardarEncuesta(encuesta);
        }

        
        public bool Actualizar(Encuesta encuesta)
        {
            if (encuesta == null) throw new ArgumentNullException(nameof(encuesta));
            if (encuesta.EncuestaId <= 0) throw new ArgumentException("El EncuestaId debe ser mayor a 0 para actualizar.", nameof(encuesta));

            int id = EjecutarSpGuardarEncuesta(encuesta);
            return id > 0;
        }


        private int EjecutarSpGuardarEncuesta(Encuesta encuesta)
        {

            var tablaCampos = new DataTable();
            tablaCampos.Columns.Add("NombreCampo", typeof(string));
            tablaCampos.Columns.Add("TituloCampo", typeof(string));
            tablaCampos.Columns.Add("EsRequerido", typeof(bool));
            tablaCampos.Columns.Add("TipoCampoId", typeof(byte));
            tablaCampos.Columns.Add("Orden", typeof(int));

            if (encuesta.Campos != null && encuesta.Campos.Count > 0)
            {
                int ordenIndex = 1;
                foreach (var campo in encuesta.Campos)
                {
                    tablaCampos.Rows.Add(
                        campo.NombreCampo.Trim(),
                        campo.TituloCampo.Trim(),
                        campo.EsRequerido,
                        campo.TipoCampoId,
                        ordenIndex++
                    );
                }
            }

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("dbo.sp_GuardarEncuesta", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@EncuestaId", SqlDbType.Int) { Value = encuesta.EncuestaId });
                cmd.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = encuesta.UsuarioId });
                cmd.Parameters.Add(new SqlParameter("@Nombre", SqlDbType.NVarChar, 200) { Value = encuesta.Nombre.Trim() });
                cmd.Parameters.Add(new SqlParameter("@Descripcion", SqlDbType.NVarChar, 500)
                {
                    Value = (object)encuesta.Descripcion?.Trim() ?? DBNull.Value
                });

                var paramLink = new SqlParameter("@LinkUnico", SqlDbType.VarChar, 36)
                {
                    Direction = ParameterDirection.InputOutput,
                    Value = string.IsNullOrWhiteSpace(encuesta.LinkUnico) ? (object)DBNull.Value : encuesta.LinkUnico.Trim()
                };
                cmd.Parameters.Add(paramLink);

                var paramCampos = new SqlParameter("@Campos", SqlDbType.Structured)
                {
                    TypeName = "dbo.CampoEncuestaType",
                    Value = tablaCampos
                };
                cmd.Parameters.Add(paramCampos);

                conn.Open();

                using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        encuesta.EncuestaId = reader.GetInt32(reader.GetOrdinal("EncuestaId"));
                        if (!reader.IsDBNull(reader.GetOrdinal("LinkUnico")))
                        {
                            encuesta.LinkUnico = reader.GetString(reader.GetOrdinal("LinkUnico"));
                        }
                    }
                }

                return encuesta.EncuestaId;
            }
        }

        public bool Eliminar(int encuestaId, int usuarioId)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("dbo.sp_EliminarEncuesta", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@EncuestaId", SqlDbType.Int) { Value = encuestaId });
                cmd.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

                conn.Open();
                var resultado = cmd.ExecuteScalar();
                return resultado != null && Convert.ToInt32(resultado) > 0;
            }
        }

        /// <summary>
        /// Obtiene una encuesta activa y sus campos dinámicos mediante su Link.
        /// </summary>
        public Encuesta ObtenerPorLink(string linkUnico)
        {
            if (!Guid.TryParseExact(linkUnico?.Trim(), "D", out _)) return null;

            Encuesta encuesta = null;

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("dbo.sp_ObtenerEncuestaPorLink", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@LinkUnico", SqlDbType.VarChar, 36) { Value = linkUnico.Trim() });

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    // Encabezado
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
                            FechaCreacion = reader.GetDateTime(6)
                        };
                    }

                    // Campos Dinámicos
                    if (encuesta != null && reader.NextResult())
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

            return encuesta;
        }

       
        public int GuardarRespuesta(string linkUnico, string ipCliente, List<ValorCampo> valores)
        {
            if (string.IsNullOrWhiteSpace(linkUnico)) throw new ArgumentNullException(nameof(linkUnico));

            var tablaValores = new DataTable();
            tablaValores.Columns.Add("CampoId", typeof(int));
            tablaValores.Columns.Add("Valor", typeof(string));

            if (valores != null)
            {
                foreach (var v in valores)
                {
                    tablaValores.Rows.Add(v.CampoId, (object)v.Valor ?? DBNull.Value);
                }
            }

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("dbo.sp_GuardarRespuestaEncuesta", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@LinkUnico", SqlDbType.VarChar, 36) { Value = linkUnico.Trim() });
                cmd.Parameters.Add(new SqlParameter("@IpCliente", SqlDbType.VarChar, 45) { Value = (object)ipCliente ?? DBNull.Value });

                var paramValores = new SqlParameter("@Valores", SqlDbType.Structured)
                {
                    TypeName = "dbo.ValorCampoRespuestaType",
                    Value = tablaValores
                };
                cmd.Parameters.Add(paramValores);

                conn.Open();
                var res = cmd.ExecuteScalar();
                return res != null ? Convert.ToInt32(res) : 0;
            }
        }

        
        public ResultadosViewModel ObtenerResultados(int encuestaId, int usuarioId)
        {
            var modelo = new ResultadosViewModel();

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("dbo.sp_ObtenerResultadosEncuesta", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@EncuestaId", SqlDbType.Int) { Value = encuestaId });
                cmd.Parameters.Add(new SqlParameter("@UsuarioId", SqlDbType.Int) { Value = usuarioId });

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    // Resumen de Encuesta
                    if (reader.Read())
                    {
                        modelo.EncuestaId = reader.GetInt32(0);
                        modelo.Nombre = reader.GetString(1);
                        modelo.Descripcion = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                        modelo.LinkUnico = reader.GetString(3);
                        modelo.TotalRespuestas = reader.GetInt32(5);
                    }
                    else
                    {
                        return null; // Encuesta no encontrada o no pertenece al usuario
                    }

                    // Columnas de Campos configurados
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            modelo.Columnas.Add(new ColumnaResultadoViewModel
                            {
                                CampoId = reader.GetInt32(0),
                                NombreCampo = reader.GetString(1),
                                TituloCampo = reader.GetString(2),
                                TipoCampoId = reader.GetByte(3),
                                Orden = reader.GetInt32(4)
                            });
                        }
                    }

                    // Respuestas individuales y sus valores
                    if (reader.NextResult())
                    {
                        var mapaFilas = new Dictionary<int, FilaResultadoViewModel>();

                        while (reader.Read())
                        {
                            int respuestaId = reader.GetInt32(0);
                            DateTime fechaRespuesta = reader.GetDateTime(1);
                            string ipCliente = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);

                            if (!mapaFilas.TryGetValue(respuestaId, out var fila))
                            {
                                fila = new FilaResultadoViewModel
                                {
                                    RespuestaId = respuestaId,
                                    FechaRespuesta = fechaRespuesta,
                                    IpCliente = ipCliente
                                };
                                mapaFilas[respuestaId] = fila;
                                modelo.Filas.Add(fila);
                            }

                            if (!reader.IsDBNull(3))
                            {
                                int campoId = reader.GetInt32(3);
                                string valor = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                                fila.ValoresPorCampo[campoId] = valor;
                            }
                        }
                    }
                }
            }

            return modelo;
        }
    }
}
