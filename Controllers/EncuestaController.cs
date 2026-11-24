using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AcmeEncuestasWeb.Data;
using AcmeEncuestasWeb.Services;
using AcmeEncuestasWeb.ViewModels;

namespace AcmeEncuestasWeb.Controllers
{
    
    [Authorize]
    public class EncuestaController : Controller
    {
        private readonly EncuestaService _encuestaService;
        private readonly UsuarioRepository _usuarioRepository;

        public EncuestaController()
        {
            _encuestaService = new EncuestaService();
            _usuarioRepository = new UsuarioRepository();
        }

        public EncuestaController(EncuestaService encuestaService, UsuarioRepository usuarioRepository)
        {
            _encuestaService = encuestaService;
            _usuarioRepository = usuarioRepository;
        }

        [HttpGet]
        public ActionResult Index()
        {
            int usuarioId = ObtenerUsuarioIdActual();
            string baseUrl = ObtenerBaseUrl();
            var listado = _encuestaService.ListarPorUsuario(usuarioId, baseUrl);
            return View(listado);
        }

        [HttpGet]
        public ActionResult Crear()
        {
            var model = new EncuestaFormViewModel
            {
                Campos = new List<CampoEditorViewModel>
                {
                    new CampoEditorViewModel
                    {
                        NombreCampo = "NombreCompleto",
                        TituloCampo = "Nombre Completo",
                        EsRequerido = true,
                        TipoCampoId = 1, // Texto
                        Orden = 1
                    }
                }
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(EncuestaFormViewModel model)
        {
            ValidarCamposDinamicos(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                int usuarioId = ObtenerUsuarioIdActual();
                int nuevoId = _encuestaService.Crear(model, usuarioId);

                TempData["MensajeExito"] = "La encuesta ha sido creada exitosamente con su enlace único de llenado.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error al crear encuesta: {0}", ex);
                ModelState.AddModelError("", "Ocurrió un error al intentar guardar la encuesta. Por favor, verifique los datos.");
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Editar(int id)
        {
            int usuarioId = ObtenerUsuarioIdActual();
            string baseUrl = ObtenerBaseUrl();
            var model = _encuestaService.ObtenerParaEditar(id, usuarioId, baseUrl);

            if (model == null)
            {
                return HttpNotFound("La encuesta solicitada no existe o no tiene permisos para editarla.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(EncuestaFormViewModel model)
        {
            ValidarCamposDinamicos(model);

            if (!ModelState.IsValid)
            {
                model.UrlPublica = $"{ObtenerBaseUrl().TrimEnd('/')}/Publico/Llenar/{model.LinkUnico}";
                return View(model);
            }

            try
            {
                int usuarioId = ObtenerUsuarioIdActual();
                bool actualizado = _encuestaService.Actualizar(model, usuarioId);

                if (!actualizado)
                {
                    return HttpNotFound("No fue posible actualizar la encuesta.");
                }

                TempData["MensajeExito"] = "La encuesta ha sido actualizada correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error al actualizar encuesta: {0}", ex);
                ModelState.AddModelError("", "Ocurrió un error al intentar actualizar la encuesta.");
                model.UrlPublica = $"{ObtenerBaseUrl().TrimEnd('/')}/Publico/Llenar/{model.LinkUnico}";
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult Eliminar(int id)
        {
            int usuarioId = ObtenerUsuarioIdActual();
            string baseUrl = ObtenerBaseUrl();
            var model = _encuestaService.ObtenerParaEditar(id, usuarioId, baseUrl);

            if (model == null)
            {
                return HttpNotFound("La encuesta solicitada no existe o no tiene permisos para eliminarla.");
            }

            return View(model);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarConfirmado(int id)
        {
            try
            {
                int usuarioId = ObtenerUsuarioIdActual();
                bool eliminado = _encuestaService.Eliminar(id, usuarioId);

                if (!eliminado)
                {
                    TempData["MensajeError"] = "No fue posible eliminar la encuesta solicitada.";
                }
                else
                {
                    TempData["MensajeExito"] = "La encuesta y sus campos han sido eliminados correctamente.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error al eliminar encuesta: {0}", ex);
                TempData["MensajeError"] = "Ocurrió un error al intentar eliminar la encuesta.";
                return RedirectToAction("Index");
            }
        }

        #region Helpers

        private void ValidarCamposDinamicos(EncuestaFormViewModel model)
        {
            if (model.Campos == null || !model.Campos.Any(c => !string.IsNullOrWhiteSpace(c.NombreCampo) && !string.IsNullOrWhiteSpace(c.TituloCampo)))
            {
                ModelState.AddModelError("", "Debe configurar al menos un campo dinámico para la encuesta.");
                return;
            }

            var nombresDuplicados = model.Campos
                .Where(c => !string.IsNullOrWhiteSpace(c.NombreCampo))
                .GroupBy(c => c.NombreCampo.Trim().ToLowerInvariant())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (nombresDuplicados.Any())
            {
                ModelState.AddModelError("", $"Los siguientes nombres de campo están repetidos: {string.Join(", ", nombresDuplicados)}. Cada campo debe tener un nombre único.");
            }
        }

        private int ObtenerUsuarioIdActual()
        {
            if (Session["UsuarioId"] != null && int.TryParse(Session["UsuarioId"].ToString(), out int id))
            {
                return id;
            }

            // Recuperar de base de datos si la sesión expiró pero la cookie sigue activa
            var usuario = _usuarioRepository.ObtenerPorUsername(User.Identity.Name);
            if (usuario != null)
            {
                Session["UsuarioId"] = usuario.UsuarioId;
                Session["NombreCompleto"] = usuario.NombreCompleto;
                return usuario.UsuarioId;
            }

            throw new InvalidOperationException("No se encontró el usuario actual en sesión.");
        }

        private string ObtenerBaseUrl()
        {
            if (Request != null && Request.Url != null)
            {
                return $"{Request.Url.Scheme}://{Request.Url.Authority}{Url.Content("~")}";
            }
            return "/";
        }

        #endregion
    }
}
