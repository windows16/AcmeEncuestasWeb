using System;
using System.Collections.Generic;

namespace AcmeEncuestasWeb.ViewModels
{
    /// <summary>
    /// Modelo de Vista para la consulta tabulada de resultados de una encuesta (Requisito 4).
    /// </summary>
    public class ResultadosViewModel
    {
        public int EncuestaId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string LinkUnico { get; set; }
        public string UrlPublica { get; set; }
        public int TotalRespuestas { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public List<ColumnaResultadoViewModel> Columnas { get; set; }
        public List<FilaResultadoViewModel> Filas { get; set; }

        public ResultadosViewModel()
        {
            Columnas = new List<ColumnaResultadoViewModel>();
            Filas = new List<FilaResultadoViewModel>();
        }
    }

    public class ColumnaResultadoViewModel
    {
        public int CampoId { get; set; }
        public string NombreCampo { get; set; }
        public string TituloCampo { get; set; }
        public byte TipoCampoId { get; set; }
        public int Orden { get; set; }
    }

    public class FilaResultadoViewModel
    {
        public int RespuestaId { get; set; }
        public DateTime FechaRespuesta { get; set; }
        public string IpCliente { get; set; }
        public Dictionary<int, string> ValoresPorCampo { get; set; }

        public FilaResultadoViewModel()
        {
            ValoresPorCampo = new Dictionary<int, string>();
        }
    }
}
