using System;
using System.Collections.Generic;

namespace AcmeEncuestasWeb.Models
{
    /// <summary>
    /// Encabezado de una encuesta.
    /// </summary>
    public class Encuesta
    {
        public int EncuestaId { get; set; }
        public int UsuarioId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string LinkUnico { get; set; }
        public bool EsActiva { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }

        public List<CampoEncuesta> Campos { get; set; }

        public Encuesta()
        {
            Campos = new List<CampoEncuesta>();
        }
    }
}
