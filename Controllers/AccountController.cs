using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AcmeEncuestasWeb.Services;
using AcmeEncuestasWeb.ViewModels;

namespace AcmeEncuestasWeb.Controllers
{
    /// <summary>
    /// Controlador responsable del flujo de Autenticación (Login / Logout).
    /// </summary>
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly AuthService _authService;

        public AccountController()
        {
            _authService = new AuthService();
        }

        public AccountController(AuthService authService)
        {
            _authService = authService;
        }

        // GET: /Account/Login
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            // Si el usuario ya está autenticado, redirigir al Home o Dashboard
            if (User.Identity.IsAuthenticated)
            {
                return RedirigirSegunReturnUrl(returnUrl);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var usuario = _authService.ValidarCredenciales(model.Username, model.Password);

                if (usuario == null)
                {
                    // Mensaje genérico para evitar enumeración de usuarios (Buena práctica de seguridad)
                    ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                    return View(model);
                }

                // Generar Cookie de autenticación mediante FormsAuthentication estándar de ASP.NET
                FormsAuthentication.SetAuthCookie(usuario.Username, model.Recordarme);

                // Opcional: Guardar nombre completo en sesión para la interfaz
                Session["UsuarioId"] = usuario.UsuarioId;
                Session["NombreCompleto"] = usuario.NombreCompleto;

                return RedirigirSegunReturnUrl(model.ReturnUrl);
            }
            catch (Exception)
            {
                // En entorno de desarrollo se puede registrar el log
                ModelState.AddModelError("", "Ocurrió un error al intentar iniciar sesión. Por favor, intente más tarde.");
                return View(model);
            }
        }

        // POST o GET: /Account/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Login", "Account");
        }

        #region Helpers

        private ActionResult RedirigirSegunReturnUrl(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        #endregion
    }
}
