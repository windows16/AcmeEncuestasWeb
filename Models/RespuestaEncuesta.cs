using System;
using System.Collections.Generic;

namespace AcmeEncuestasWeb.Models
{
    /// <summary>
    /// Encabezado de una respuesta enviada a una encuesta por un usuario publico.
    /// </summary>
    public class RespuestaEncuesta
    {
        
        public int RespuestaId { get; set; }
        public int EncuestaId { get; set; }
        public DateTime FechaRespuesta { get; set; }
        public string IpCliente { get; set; }

        /// <summary>
        /// Valores de cada campo para esta respuesta.
        /// Se popula al cargar resultados (ObtenerPorEncuesta).
        /// </summary>
        public List<ValorCampo> Valores { get; set; }

        public RespuestaEncuesta()
        {
            Valores = new List<ValorCampo>();
        }
    }
}
