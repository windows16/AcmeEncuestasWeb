namespace AcmeEncuestasWeb.Models
{
    public enum TipoCampoEnum : byte
    {
        Texto = 1,
        Numero = 2,
        Fecha = 3
    }

    /// <summary>
    ///  catalogo de tipos de datos permitidos para campos dinámicos.
    /// </summary>
    public class TipoCampo
    {
        public byte TipoCampoId { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
    }
}
