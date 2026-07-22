using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using CRMSistema.Controllers.Base;
using CRMSistema.DAL.Dashboard;
using CRMSistema.Models.Dashboard;
using CRMSistema.Models.ViewModels;

namespace CRMSistema.Controllers.Dashboard
{
    [Authorize]
    public class DashboardController : BaseController
    {
        private readonly DashboardDAL _dal = new DashboardDAL();
        private readonly bool _modoDevSinBd;

        public DashboardController()
        {
            bool.TryParse(ConfigurationManager.AppSettings["ModoDesarrolloSinBD"], out _modoDevSinBd);
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Panel de Control";
            ViewBag.ActiveMenu = "Dashboard";

            var model = new DashboardViewModel();

            try
            {
                var ahora = DateTime.Now;
                var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddSeconds(-1);
                var inicioMesAnt = inicioMes.AddMonths(-1);
                var finMesAnt = inicioMes.AddSeconds(-1);

                double calcPct(double act, double ant)
                {
                    if (ant == 0) return act > 0 ? 100 : 0;
                    return Math.Round(((act - ant) / ant) * 1000) / 10.0;
                }

                dynamic kpisData = _dal.ObtenerKPIs(inicioMes, finMes, inicioMesAnt, finMesAnt);
                bool kpiVacio = kpisData == null ||
                    (ToDouble(kpisData?.ingMes, 0.0) == 0 && ToInt(kpisData?.prosMes, 0) == 0 &&
                     ToInt(kpisData?.cotServ, 0) == 0 && ToInt(kpisData?.cotBorr, 0) == 0 &&
                     ToInt(kpisData?.deudores, 0) == 0 && ToInt(kpisData?.alCorriente, 0) == 0);

                double ingMes = kpiVacio ? _dal.IngresosMes(inicioMes, finMes) : ToDouble(kpisData?.ingMes, 0.0);
                double ingAnt = kpiVacio ? _dal.IngresosMes(inicioMesAnt, finMesAnt) : ToDouble(kpisData?.ingAnt, 0.0);
                int prosMes = kpiVacio ? _dal.ContarProspectos(inicioMes, finMes) : ToInt(kpisData?.prosMes, 0);
                int prosAnt = kpiVacio ? _dal.ContarProspectos(inicioMesAnt, finMesAnt) : ToInt(kpisData?.prosAnt, 0);
                int cotActivas = kpiVacio ? _dal.ContarCotizacionesActivas() : (ToInt(kpisData?.cotServ, 0) + ToInt(kpisData?.cotBorr, 0));
                int cotServ = kpiVacio ? cotActivas : ToInt(kpisData?.cotServ, 0);
                int cotBorr = kpiVacio ? 0 : ToInt(kpisData?.cotBorr, 0);
                int totalP = ToInt(kpisData?.totalP, 0);
                int convP = ToInt(kpisData?.convP, 0);
                if (totalP == 0) totalP = _dal.ContarProspectos(DateTime.MinValue, DateTime.MaxValue);
                int deudores = kpiVacio ? _dal.ContarDeudores() : ToInt(kpisData?.deudores, 0);
                int alCorriente = kpiVacio ? _dal.ContarAlCorriente() : ToInt(kpisData?.alCorriente, 0);
                int prosSuc = kpiVacio ? _dal.ContarProspectosConSucursales() : ToInt(kpisData?.prosSuc, 0);
                int totalSuc = ToInt(kpisData?.totalSuc, 0);

                var mesesMap = new Dictionary<string, dynamic>();
                for (int i = 5; i >= 0; i--)
                {
                    var d = ahora.AddMonths(-i);
                    var key = d.ToString("yyyy-MM");
                    mesesMap[key] = new { mes = key, prospectos = 0, ingresos = 0.0, tratos = 0 };
                }

                var tendP = _dal.ObtenerTendenciaProspectos();
                var tendT = _dal.ObtenerTendenciaVentas();

                foreach (var r in tendP)
                {
                    var key = ToString(r.mes, "");
                    if (mesesMap.ContainsKey(key))
                    {
                        var existing = mesesMap[key];
                        mesesMap[key] = new { mes = existing.mes, prospectos = ToInt(r.total, 0), ingresos = existing.ingresos, tratos = existing.tratos };
                    }
                }

                foreach (var r in tendT)
                {
                    var key = ToString(r.mes, "");
                    if (mesesMap.ContainsKey(key))
                    {
                        var existing = mesesMap[key];
                        mesesMap[key] = new { mes = existing.mes, prospectos = existing.prospectos, ingresos = ToDouble(r.ingresos, 0.0), tratos = ToInt(r.tratos, 0) };
                    }
                }

                model.Kpis = new KpiResumen
                {
                    ingresosMes = new KpiValor { valor = ingMes, cambio = calcPct(ingMes, ingAnt) },
                    prospectosMes = new KpiValor { valor = prosMes, cambio = calcPct(prosMes, prosAnt) },
                    cotizaciones = new KpiCotizaciones { servicios = cotServ, borradores = cotBorr, total = cotServ + cotBorr },
                    tasaConversion = new KpiTasaConversion { valor = totalP > 0 ? Math.Round((convP / (double)totalP) * 1000) / 10.0 : 0, convertidos = convP, totalProspectos = totalP },
                    deudores = deudores,
                    alCorriente = alCorriente,
                    sucursales = new KpiSucursales { prospectosConSuc = prosSuc, totalSuc = totalSuc }
                };

                model.Tendencia = mesesMap.Values.Select(x => new TendenciaMesDto
                {
                    mes = x.mes,
                    prospectos = x.prospectos,
                    ingresos = x.ingresos,
                    tratos = x.tratos
                }).ToList();

                model.Origenes = _dal.ObtenerOrigenes()
                    .Select(r => new OrigenDto { nombre = ToString(r.nombre), cantidad = ToInt(r.cantidad, 0) }).ToList();

                model.TiposInmueble = _dal.ObtenerTiposInmueble()
                    .Select(r => new TipoInmuebleDto { tipo = ToString(r.tipo), cantidad = ToInt(r.cantidad, 0) }).ToList();

                // Distribución por estatus: solo del mes actual para que se “limpie” cada mes desde cero
                var distEstatus = _dal.ObtenerEstatusDistribucionPorMes(inicioMes, finMes)
                    .Select(r => new EstatusDistribucionDto { estatus = ToString(r.estatus), cantidad = ToInt(r.cantidad, 0) }).ToList();
                model.EstatusDistribucion = CompletarEstatusDistribucion(distEstatus);

                var pipeline = _dal.ObtenerPipeline()
                    .Select(r => new PipelineDto
                    {
                        empresa = ToString(r.empresa),
                        contacto = ToString(r.contacto),
                        estatus = ToString(r.estatus),
                        tipoInmueble = ToString(r.tipoInmueble),
                        fuente = ToString(r.fuente),
                        trato = ToString(r.trato),
                        monto = ToDecimal(r.monto),
                        fase = ToString(r.fase),
                        tieneSucursales = ToString(r.tieneSucursales),
                        fecha = r.fecha
                    }).ToList();
                if (pipeline.Count == 0)
                    pipeline = CalcularPipelineDesdeProspectos();
                model.Pipeline = pipeline;

                model.CotizacionesDetalle = _dal.ObtenerCotizacionesDetalle()
                    .Select(r => new CotizacionDetalleDto
                    {
                        tipo_residuo = ToString(r.tipo_residuo),
                        frecuencia = ToString(r.frecuencia),
                        periodicidad_pago = ToString(r.periodicidad_pago),
                        volumen_estimado = ToDecimal(r.volumen_estimado),
                        precio_unitario = ToDecimal(r.precio_unitario),
                        trato = ToString(r.trato),
                        empresa = ToString(r.empresa)
                    }).ToList();
            }
            catch (Exception ex)
            {
                if (_modoDevSinBd)
                {
                    CargarDatosDemo(model);
                }
                else
                {
                    model.Error = "No se pudo cargar el dashboard: " + ex.Message;
                }
            }

            return View(model);
        }

        private List<EstatusDistribucionDto> CalcularEstatusDistribucionDesdeProspectos()
        {
            try
            {
                var pDal = new CRMSistema.DAL.Prospectos.ApiProspectosDAL();
                var prospectos = pDal.ObtenerTodos();
                var datos = prospectos
                    .GroupBy(p => (p.estatus as string) ?? "Sin estatus")
                    .Select(g => new EstatusDistribucionDto { estatus = g.Key, cantidad = g.Count() })
                    .OrderByDescending(x => x.cantidad)
                    .ToList();
                return CompletarEstatusDistribucion(datos);
            }
            catch { return new List<EstatusDistribucionDto>(); }
        }

        private List<EstatusDistribucionDto> CompletarEstatusDistribucion(List<EstatusDistribucionDto> datos)
        {
            var ordenEstatus = new[] { "Nuevo", "En revisión", "En seguimiento", "Cotizado", "Aprobado", "Rechazado", "Adeudo", "Inactivo" };
            var datosLimpios = (datos ?? new List<EstatusDistribucionDto>())
                .Where(x => !string.IsNullOrWhiteSpace(x.estatus))
                .Select(x => new EstatusDistribucionDto { estatus = x.estatus.Trim(), cantidad = x.cantidad })
                .ToList();

            var mapa = datosLimpios
                .ToDictionary(x => x.estatus, x => x.cantidad, StringComparer.OrdinalIgnoreCase);

            // Estatus base siempre visibles (aunque tengan 0)
            var resultado = ordenEstatus
                .Select(e => new EstatusDistribucionDto { estatus = e, cantidad = mapa.ContainsKey(e) ? mapa[e] : 0 })
                .ToList();

            // Agregar estatus extra que vengan de la base de datos y no estén en la lista base
            var extras = datosLimpios
                .Where(x => !ordenEstatus.Contains(x.estatus, StringComparer.OrdinalIgnoreCase))
                .OrderByDescending(x => x.cantidad)
                .ToList();

            resultado.AddRange(extras);
            return resultado;
        }

        private List<PipelineDto> CalcularPipelineDesdeProspectos()
        {
            try
            {
                // Intentar primero el fallback SQL con JOIN a tratos para traer montos reales
                var sqlRows = _dal.PipelineDesdeProspectos();
                if (sqlRows.Count > 0)
                {
                    return sqlRows.Select(r => new PipelineDto
                    {
                        empresa = ToString(r.empresa),
                        contacto = !string.IsNullOrWhiteSpace(ToString(r.contactos))
                            ? ToString(r.contactos)
                            : ToString(r.contacto_principal),
                        estatus = ToString(r.estatus) ?? "Nuevo",
                        tipoInmueble = ToString(r.tipoInmueble),
                        fuente = "",
                        trato = "",
                        monto = ToDecimal(r.monto),
                        fase = "",
                        tieneSucursales = !string.IsNullOrWhiteSpace(ToString(r.sucursales))
                            ? ToString(r.sucursales)
                            : (((ToString(r.tieneSucursales) ?? "").ToLower().StartsWith("s") ? "Sí" : "No")),
                        fecha = r.fecha
                    }).Take(20).ToList();
                }
            }
            catch { }

            // Fallback final a la API generica de prospectos
            try
            {
                var pDal = new CRMSistema.DAL.Prospectos.ApiProspectosDAL();
                var prospectos = pDal.ObtenerTodos();
                return prospectos.Select(p => new PipelineDto
                {
                    empresa = p.nombre as string ?? "",
                    contacto = p.contacto as string ?? "",
                    estatus = p.estatus as string ?? "Nuevo",
                    tipoInmueble = p.tipoInmueble as string ?? "",
                    fuente = "",
                    trato = "",
                    monto = 0,
                    fase = "",
                    tieneSucursales = ((p.tieneSucursales as string) ?? "").ToLower().StartsWith("s") ? "Sí" : "No",
                    fecha = p.GetType().GetProperty("Fecha_Creacion")?.GetValue(p)
                }).Take(20).ToList();
            }
            catch { return new List<PipelineDto>(); }
        }

        [HttpGet]
        public ActionResult GetEstatusDistribucionPorMes(string mes)
        {
            try
            {
                DateTime inicio, fin;
                if (string.IsNullOrEmpty(mes) || !DateTime.TryParse(mes + "-01", out inicio))
                {
                    inicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }
                fin = inicio.AddMonths(1).AddSeconds(-1);

                var datos = _dal.ObtenerEstatusDistribucionPorMes(inicio, fin)
                    .Select(r => new EstatusDistribucionDto { estatus = ToString(r.estatus), cantidad = ToInt(r.cantidad, 0) }).ToList();

                // Solo si se solicita el mes actual y no hay datos historicos,
                // mostramos la distribucion actual como referencia.
                var ahora = DateTime.Now;
                bool esMesActual = inicio.Year == ahora.Year && inicio.Month == ahora.Month;
                if (datos.Count == 0 && esMesActual)
                    datos = CalcularEstatusDistribucionDesdeProspectos();

                datos = CompletarEstatusDistribucion(datos);

                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { success = true, data = datos }), "application/json");
            }
            catch (Exception ex)
            {
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { success = false, error = ex.Message }), "application/json");
            }
        }

        private void CargarDatosDemo(DashboardViewModel model)
        {
            model.Kpis = new KpiResumen
            {
                ingresosMes = new KpiValor { valor = 125000, cambio = 12.5 },
                prospectosMes = new KpiValor { valor = 34, cambio = -5.2 },
                cotizaciones = new KpiCotizaciones { servicios = 18, borradores = 7, total = 25 },
                tasaConversion = new KpiTasaConversion { valor = 42.5, convertidos = 14, totalProspectos = 33 },
                deudores = 3,
                alCorriente = 28,
                sucursales = new KpiSucursales { prospectosConSuc = 12, totalSuc = 19 }
            };

            model.Tendencia = new List<TendenciaMesDto>
            {
                new TendenciaMesDto { mes = "2026-01", prospectos = 10, ingresos = 80000, tratos = 2 },
                new TendenciaMesDto { mes = "2026-02", prospectos = 15, ingresos = 95000, tratos = 3 },
                new TendenciaMesDto { mes = "2026-03", prospectos = 12, ingresos = 110000, tratos = 4 },
                new TendenciaMesDto { mes = "2026-04", prospectos = 20, ingresos = 105000, tratos = 5 },
                new TendenciaMesDto { mes = "2026-05", prospectos = 28, ingresos = 120000, tratos = 6 },
                new TendenciaMesDto { mes = "2026-06", prospectos = 34, ingresos = 125000, tratos = 7 }
            };

            model.Origenes = new List<OrigenDto>
            {
                new OrigenDto { nombre = "Referido", cantidad = 12 },
                new OrigenDto { nombre = "Sitio web", cantidad = 8 },
                new OrigenDto { nombre = "Redes sociales", cantidad = 5 },
                new OrigenDto { nombre = "Llamada", cantidad = 9 }
            };

            model.TiposInmueble = new List<TipoInmuebleDto>
            {
                new TipoInmuebleDto { tipo = "Comercio", cantidad = 15 },
                new TipoInmuebleDto { tipo = "Industria", cantidad = 10 },
                new TipoInmuebleDto { tipo = "Oficinas", cantidad = 7 },
                new TipoInmuebleDto { tipo = "Residencial", cantidad = 2 }
            };

            model.EstatusDistribucion = new List<EstatusDistribucionDto>
            {
                new EstatusDistribucionDto { estatus = "Nuevo", cantidad = 10 },
                new EstatusDistribucionDto { estatus = "En revisión", cantidad = 5 },
                new EstatusDistribucionDto { estatus = "En seguimiento", cantidad = 12 },
                new EstatusDistribucionDto { estatus = "Cotizado", cantidad = 8 },
                new EstatusDistribucionDto { estatus = "Aprobado", cantidad = 3 },
                new EstatusDistribucionDto { estatus = "Rechazado", cantidad = 2 },
                new EstatusDistribucionDto { estatus = "Adeudo", cantidad = 1 },
                new EstatusDistribucionDto { estatus = "Inactivo", cantidad = 4 }
            };

            model.Pipeline = new List<PipelineDto>
            {
                new PipelineDto { empresa = "Empresa Demo SA", contacto = "Juan Pérez", estatus = "En seguimiento", monto = 15000, tieneSucursales = "Sí" },
                new PipelineDto { empresa = "Constructora Yucatán", contacto = "Ana Ruiz", estatus = "Cotizado", monto = 28000, tieneSucursales = "No" },
                new PipelineDto { empresa = "Hotel Mérida", contacto = "Luis Torres", estatus = "Nuevo", monto = 12000, tieneSucursales = "No" }
            };
        }
    }
}
