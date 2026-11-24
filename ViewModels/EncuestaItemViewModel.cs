using System;

namespace AcmeEncuestasWeb.ViewModels
{
    /// <summary>
    /// Modelo de Vista para cada elemento en el listado de encuestas creadas por el usuario.
    /// </summary>
    public class EncuestaItemViewModel
    {
        public int EncuestaId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string LinkUnico { get; set; }
        public string UrlPublica { get; set; }
        public int TotalCampos { get; set; }
        public int TotalRespuestas { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
    }
}
