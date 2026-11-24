using System.ComponentModel.DataAnnotations;

namespace AcmeEncuestasWeb.ViewModels
{
    /// <summary>
    /// Modelo de Vista para el Formulario de Login.
    /// Incorpora validaciones declarativas (Data Annotations) para garantizar integridad
    /// antes de que la petición llegue a la base de datos.
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [Display(Name = "Usuario")]
        [StringLength(50, ErrorMessage = "El usuario no puede superar 50 caracteres.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name = "¿Recordar sesión?")]
        public bool Recordarme { get; set; }

        public string ReturnUrl { get; set; }
    }
}
