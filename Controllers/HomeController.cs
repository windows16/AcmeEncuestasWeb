using System.Web.Mvc;

namespace AcmeEncuestasWeb.Controllers
{
    /// <summary>
    /// Controlador principal para usuarios autenticados.
    /// Protegido con [Authorize]: si el usuario no tiene sesión activa,
    /// es redirigido automáticamente al Login.
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        // GET: /Home/Index
        public ActionResult Index()
        {
            return View();
        }
    }
}
