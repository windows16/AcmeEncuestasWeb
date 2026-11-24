using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AcmeEncuestasWeb.ViewModels
{
    /// <summary>
    /// Modelo de Vista para la creación y edición de encuestas junto con sus campos dinámicos.
    /// </summary>
    public class EncuestaFormViewModel
    {
        public int EncuestaId { get; set; }

        [Required(ErrorMessage = "El nombre de la encuesta es obligatorio.")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder los 200 caracteres.")]
        [Display(Name = "Nombre de la Encuesta")]
        public string Nombre { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Enlace Único de Llenado")]
        public string LinkUnico { get; set; }

        public string UrlPublica { get; set; }

        public List<CampoEditorViewModel> Campos { get; set; }

        public EncuestaFormViewModel()
        {
            Campos = new List<CampoEditorViewModel>();
        }
    }
}
