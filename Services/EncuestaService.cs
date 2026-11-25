using System;
using System.Collections.Generic;
using System.Linq;
using AcmeEncuestasWeb.Data;
using AcmeEncuestasWeb.Models;
using AcmeEncuestasWeb.ViewModels;

namespace AcmeEncuestasWeb.Services
{
    
    public class EncuestaService
    {
        private readonly EncuestaRepository _encuestaRepository;

        public EncuestaService()
        {
            _encuestaRepository = new EncuestaRepository();
        }

        public EncuestaService(EncuestaRepository encuestaRepository)
        {
            _encuestaRepository = encuestaRepository;
        }

        public List<EncuestaItemViewModel> ListarPorUsuario(int usuarioId, string baseUrl = "")
        {
            var encuestas = _encuestaRepository.ObtenerPorUsuario(usuarioId);
            return encuestas.Select(e => new EncuestaItemViewModel
            {
                EncuestaId = e.EncuestaId,
                Nombre = e.Nombre,
                Descripcion = e.Descripcion,
                LinkUnico = e.LinkUnico,
                UrlPublica = string.IsNullOrEmpty(baseUrl) ? $"/Publico/Llenar/{e.LinkUnico}" : $"{baseUrl.TrimEnd('/')}/Publico/Llenar/{e.LinkUnico}",
                TotalCampos = e.TotalCampos,
                TotalRespuestas = e.TotalRespuestas,
                FechaCreacion = e.FechaCreacion,
                FechaModificacion = e.FechaModificacion
            }).ToList();
        }

        public EncuestaFormViewModel ObtenerParaEditar(int encuestaId, int usuarioId, string baseUrl = "")
        {
            var encuesta = _encuestaRepository.ObtenerPorId(encuestaId, usuarioId);
            if (encuesta == null) return null;

            return new EncuestaFormViewModel
            {
                EncuestaId = encuesta.EncuestaId,
                Nombre = encuesta.Nombre,
                Descripcion = encuesta.Descripcion,
                LinkUnico = encuesta.LinkUnico,
                UrlPublica = string.IsNullOrEmpty(baseUrl) ? $"/Publico/Llenar/{encuesta.LinkUnico}" : $"{baseUrl.TrimEnd('/')}/Publico/Llenar/{encuesta.LinkUnico}",
                Campos = encuesta.Campos.Select(c => new CampoEditorViewModel
                {
                    CampoId = c.CampoId,
                    NombreCampo = c.NombreCampo,
                    TituloCampo = c.TituloCampo,
                    EsRequerido = c.EsRequerido,
                    TipoCampoId = c.TipoCampoId,
                    Orden = c.Orden
                }).ToList()
            };
        }

        public int Crear(EncuestaFormViewModel model, int usuarioId)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var entidad = new Encuesta
            {
                UsuarioId = usuarioId,
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                LinkUnico = Guid.NewGuid().ToString(),
                Campos = model.Campos != null ? model.Campos
                    .Where(c => !string.IsNullOrWhiteSpace(c.NombreCampo) && !string.IsNullOrWhiteSpace(c.TituloCampo))
                    .Select((c, idx) => new CampoEncuesta
                    {
                        NombreCampo = c.NombreCampo.Trim(),
                        TituloCampo = c.TituloCampo.Trim(),
                        EsRequerido = c.EsRequerido,
                        TipoCampoId = c.TipoCampoId == 0 ? (byte)1 : c.TipoCampoId,
                        Orden = idx + 1
                    }).ToList() : new List<CampoEncuesta>()
            };

            return _encuestaRepository.Crear(entidad);
        }

        public bool Actualizar(EncuestaFormViewModel model, int usuarioId)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var entidad = new Encuesta
            {
                EncuestaId = model.EncuestaId,
                UsuarioId = usuarioId,
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                Campos = model.Campos != null ? model.Campos
                    .Where(c => !string.IsNullOrWhiteSpace(c.NombreCampo) && !string.IsNullOrWhiteSpace(c.TituloCampo))
                    .Select((c, idx) => new CampoEncuesta
                    {
                        NombreCampo = c.NombreCampo.Trim(),
                        TituloCampo = c.TituloCampo.Trim(),
                        EsRequerido = c.EsRequerido,
                        TipoCampoId = c.TipoCampoId == 0 ? (byte)1 : c.TipoCampoId,
                        Orden = idx + 1
                    }).ToList() : new List<CampoEncuesta>()
            };

            return _encuestaRepository.Actualizar(entidad);
        }

        public bool Eliminar(int encuestaId, int usuarioId)
        {
            return _encuestaRepository.Eliminar(encuestaId, usuarioId);
        }

        public LlenarEncuestaViewModel ObtenerParaResponder(string linkUnico)
        {
            var encuesta = _encuestaRepository.ObtenerPorLink(linkUnico);
            if (encuesta == null) return null;

            return new LlenarEncuestaViewModel
            {
                EncuestaId = encuesta.EncuestaId,
                Nombre = encuesta.Nombre,
                Descripcion = encuesta.Descripcion,
                LinkUnico = encuesta.LinkUnico,
                Campos = encuesta.Campos.Select(c => new CampoRespuestaViewModel
                {
                    CampoId = c.CampoId,
                    NombreCampo = c.NombreCampo,
                    TituloCampo = c.TituloCampo,
                    EsRequerido = c.EsRequerido,
                    TipoCampoId = c.TipoCampoId
                }).ToList()
            };
        }

        public int GuardarRespuesta(LlenarEncuestaViewModel model, string ipCliente)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var valores = model.Campos != null ? model.Campos.Select(c =>
            {
                var valor = c.Valor;
                return new ValorCampo
                {
                    CampoId = c.CampoId,
                    Valor = SurveyValueProtector.Protect(valor),
                    EsVacio = string.IsNullOrWhiteSpace(valor)
                };
            }).ToList() : new List<ValorCampo>();

            return _encuestaRepository.GuardarRespuesta(model.LinkUnico, ipCliente, valores);
        }

        public ResultadosViewModel ObtenerResultados(int encuestaId, int usuarioId, int pagina, string terminoBusqueda = null)
        {
            terminoBusqueda = terminoBusqueda?.Trim();
            if (string.IsNullOrEmpty(terminoBusqueda))
            {
                var resultados = _encuestaRepository.ObtenerResultados(encuestaId, usuarioId, pagina);
                if (resultados != null)
                {
                    resultados.TerminoBusqueda = string.Empty;
                }

                return resultados;
            }

            var primeraPagina = _encuestaRepository.ObtenerResultados(encuestaId, usuarioId, 1);
            if (primeraPagina == null)
            {
                return null;
            }

            int paginaSolicitada = Math.Max(1, pagina);
            long inicioPaginaSolicitada = (long)(paginaSolicitada - 1) * ResultadosViewModel.TamanoPagina;
            int totalCoincidencias = 0;
            var filasPaginaSolicitada = new List<FilaResultadoViewModel>();
            var filasUltimaPagina = new Queue<FilaResultadoViewModel>();

            for (int numeroPagina = 1; numeroPagina <= primeraPagina.TotalPaginas; numeroPagina++)
            {
                var paginaResultados = numeroPagina == 1
                    ? primeraPagina
                    : _encuestaRepository.ObtenerResultados(encuestaId, usuarioId, numeroPagina);

                if (paginaResultados == null)
                {
                    return null;
                }

                foreach (var fila in paginaResultados.Filas)
                {
                    if (!fila.ValoresPorCampo.Values.Any(valor =>
                        valor.IndexOf(terminoBusqueda, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        continue;
                    }

                    if (totalCoincidencias >= inicioPaginaSolicitada
                        && totalCoincidencias < inicioPaginaSolicitada + ResultadosViewModel.TamanoPagina)
                    {
                        filasPaginaSolicitada.Add(fila);
                    }

                    filasUltimaPagina.Enqueue(fila);
                    if (filasUltimaPagina.Count > ResultadosViewModel.TamanoPagina)
                    {
                        filasUltimaPagina.Dequeue();
                    }

                    totalCoincidencias++;
                }
            }

            int totalPaginas = Math.Max(1, (int)Math.Ceiling(totalCoincidencias / (double)ResultadosViewModel.TamanoPagina));
            int paginaActual = Math.Min(paginaSolicitada, totalPaginas);

            primeraPagina.TerminoBusqueda = terminoBusqueda;
            primeraPagina.TotalRespuestas = totalCoincidencias;
            primeraPagina.TotalPaginas = totalPaginas;
            primeraPagina.PaginaActual = paginaActual;
            primeraPagina.Filas = paginaActual == paginaSolicitada
                ? filasPaginaSolicitada
                : filasUltimaPagina.ToList();

            return primeraPagina;
        }
    }
}
