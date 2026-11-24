using System.ComponentModel.DataAnnotations;

namespace AcmeEncuestasWeb.ViewModels
{
    
    public class CampoEditorViewModel
    {
        public int CampoId { get; set; }

        [Required(ErrorMessage = "El nombre del campo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del campo no puede exceder 100 caracteres.")]
        [Display(Name = "Nombre del Campo")]
        public string NombreCampo { get; set; }

        [Required(ErrorMessage = "El título del campo es obligatorio.")]
        [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres.")]
        [Display(Name = "Título en Pantalla")]
        public string TituloCampo { get; set; }

        [Display(Name = "¿Es Requerido?")]
        public bool EsRequerido { get; set; }

        [Display(Name = "Tipo de Campo")]
        public byte TipoCampoId { get; set; }

        public int Orden { get; set; }
    }
}
