namespace AcmeEncuestasWeb.ViewModels
{
    /// <summary>
    /// Representa un campo dinámico dentro de una encuesta para su captura por un participante público.
    /// </summary>
    public class CampoRespuestaViewModel
    {
        public int CampoId { get; set; }
        public string NombreCampo { get; set; }
        public string TituloCampo { get; set; }
        public bool EsRequerido { get; set; }
        public byte TipoCampoId { get; set; } // 1: Texto, 2: Número, 3: Fecha
        public string Valor { get; set; }
    }
}
