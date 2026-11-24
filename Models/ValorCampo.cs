namespace AcmeEncuestasWeb.Models
{
   
    public class ValorCampo
    {
        
        public int ValorId { get; set; }
        public int RespuestaId { get; set; }
        public int CampoId { get; set; }
        public string Valor { get; set; }
        public string TituloCampo { get; set; }
    }
}
