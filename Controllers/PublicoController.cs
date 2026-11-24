using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using AcmeEncuestasWeb.Services;
using AcmeEncuestasWeb.ViewModels;

namespace AcmeEncuestasWeb.Controllers
{
    [AllowAnonymous]
    public class PublicoController : Controller
    {
        private readonly EncuestaService _encuestaService;

        public PublicoController()
        {
            _encuestaService = new EncuestaService();
        }

        public PublicoController(EncuestaService encuestaService)
        {
            _encuestaService = encuestaService ?? throw new ArgumentNullException(nameof(encuestaService));
        }

        [HttpGet]
        public ActionResult Llenar(string id)
        {
            var encuesta = _encuestaService.ObtenerParaResponder(id);
            return encuesta == null ? (ActionResult)HttpNotFound("La encuesta no existe o ya no está disponible.") : View(encuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Llenar(string id, LlenarEncuestaViewModel model)
        {
            var encuesta = _encuestaService.ObtenerParaResponder(id);
            if (encuesta == null)
            {
                return HttpNotFound("La encuesta no existe o ya no está disponible.");
            }

            var valoresEnviados = model?.Campos ?? new List<CampoRespuestaViewModel>();
            var camposNoNulos = valoresEnviados.Where(c => c != null).ToList();
            if (camposNoNulos.GroupBy(c => c.CampoId).Any(g => g.Count() > 1))
            {
                ModelState.AddModelError("", "La respuesta contiene campos duplicados.");
            }

            var idsConfigurados = new HashSet<int>(encuesta.Campos.Select(c => c.CampoId));
            if (camposNoNulos.Any(c => !idsConfigurados.Contains(c.CampoId)))
            {
                ModelState.AddModelError("", "La respuesta contiene campos que no pertenecen a esta encuesta.");
            }

            var valores = new Dictionary<int, string>();
            foreach (var campoEnviado in camposNoNulos)
            {
                if (!valores.ContainsKey(campoEnviado.CampoId))
                {
                    valores.Add(campoEnviado.CampoId, campoEnviado.Valor);
                }
            }

            for (var i = 0; i < encuesta.Campos.Count; i++)
            {
                var campo = encuesta.Campos[i];
                valores.TryGetValue(campo.CampoId, out var valor);
                campo.Valor = valor?.Trim();

                if (campo.EsRequerido && string.IsNullOrWhiteSpace(campo.Valor))
                {
                    ModelState.AddModelError($"Campos[{i}].Valor", "Este campo es obligatorio.");
                }
                else if (!string.IsNullOrWhiteSpace(campo.Valor) && campo.TipoCampoId == 2 &&
                    !decimal.TryParse(campo.Valor, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture, out _))
                {
                    ModelState.AddModelError($"Campos[{i}].Valor", "Ingrese un número válido.");
                }
                else if (!string.IsNullOrWhiteSpace(campo.Valor) && campo.TipoCampoId == 3 &&
                    !DateTime.TryParseExact(campo.Valor, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out _))
                {
                    ModelState.AddModelError($"Campos[{i}].Valor", "Ingrese una fecha válida.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(encuesta);
            }

            try
            {
                _encuestaService.GuardarRespuesta(encuesta, Request.UserHostAddress);
                return RedirectToAction("Gracias", new { id = encuesta.LinkUnico });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error al guardar respuesta de encuesta: {0}", ex);
                ModelState.AddModelError("", "No fue posible guardar su respuesta. Inténtelo nuevamente.");
                return View(encuesta);
            }
        }

        [HttpGet]
        public ActionResult Gracias(string id)
        {
            var encuesta = _encuestaService.ObtenerParaResponder(id);
            if (encuesta == null)
            {
                return HttpNotFound("La encuesta no existe o ya no está disponible.");
            }

            return View(encuesta);
        }
    }
}
