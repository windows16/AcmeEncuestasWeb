using System.Collections.Generic;

namespace AcmeEncuestasWeb.ViewModels
{
    /// <summary>
    /// Modelo de Vista para desplegar y responder una encuesta pública mediante su enlace único (Requisito 3).
    /// </summary>
    public class LlenarEncuestaViewModel
    {
        public int EncuestaId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string LinkUnico { get; set; }
        public bool GuardadoExitoso { get; set; }
        public List<CampoRespuestaViewModel> Campos { get; set; }

        public LlenarEncuestaViewModel()
        {
            Campos = new List<CampoRespuestaViewModel>();
        }
    }
}
