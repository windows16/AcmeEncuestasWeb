namespace AcmeEncuestasWeb.Models
{
   
    public class CampoEncuesta
    {
        public int CampoId { get; set; }
        public int EncuestaId { get; set; }
        public string NombreCampo { get; set; }
        public string TituloCampo { get; set; }
        public bool EsRequerido { get; set; }
        public byte TipoCampoId { get; set; }
        public string TipoCampoNombre { get; set; }
        public int Orden { get; set; }
    }
}
