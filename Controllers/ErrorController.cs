using System.Web.Mvc;

namespace AcmeEncuestasWeb.Controllers
{
    /// <summary>
    /// Controlador centralizado para manejo de errores HTTP y excepciones no controladas.
    /// Proporciona vistas amigables sin exponer información sensible ni stacktraces.
    /// </summary>
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        // GET: /Error
        public ActionResult Index()
        {
            Response.StatusCode = 500;
            return View("Error");
        }

        // GET: /Error/NotFound
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
        }

        // GET: /Error/ServerError
        public ActionResult ServerError()
        {
            Response.StatusCode = 500;
            return View("Error");
        }
    }
}
