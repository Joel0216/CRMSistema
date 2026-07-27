using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using CRMSistema.Controllers.Base;
using CRMSistema.DAL.Dashboard;
using CRMSistema.Models.Dashboard;
using CRMSistema.Models.Usuarios;
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
                    (ValDouble(kpisData, "ingMes") == 0 && ValInt(kpisData, "prosMes") == 0 &&
                     ValInt(kpisData, "cotServ") == 0 && ValInt(kpisData, "cotBorr") == 0 &&
                     ValInt(kpisData, "deudores") == 0 && ValInt(kpisData, "alCorriente") == 0);

                double ingMes = ToDouble(Val(kpisData, "ingMes"), 0.0);
                double ingAnt = ToDouble(Val(kpisData, "ingAnt"), 0.0);
                int prosMes = ValInt(kpisData, "prosMes");
                int prosAnt = ValInt(kpisData, "prosAnt");
                int cotServ = ValInt(kpisData, "cotServ");
                int cotBorr = ValInt(kpisData, "cotBorr");
                int cotActivas = cotServ + cotBorr;
                int totalP = ValInt(kpisData, "totalP");
                int convP = ValInt(kpisData, "convP");
                int deudores = ValInt(kpisData, "deudores");
                int alCorriente = ValInt(kpisData, "alCorriente");
                int prosSuc = ValInt(kpisData, "prosSuc");
                int totalSuc = ValInt(kpisData, "totalSuc");

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
                    var key = ValString(r, "mes");
                    if (mesesMap.ContainsKey(key))
                    {
                        var existing = mesesMap[key];
                        mesesMap[key] = new { mes = existing.mes, prospectos = ValInt(r, "total"), ingresos = existing.ingresos, tratos = existing.tratos };
                    }
                }

                foreach (var r in tendT)
                {
                    var key = ValString(r, "mes");
                    if (mesesMap.ContainsKey(key))
                    {
                        var existing = mesesMap[key];
                        mesesMap[key] = new { mes = existing.mes, prospectos = existing.prospectos, ingresos = ValDouble(r, "ingresos"), tratos = ValInt(r, "tratos") };
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
                    .Select(r => new OrigenDto { nombre = ValString(r, "nombre"), cantidad = ValInt(r, "cantidad") }).ToList();

                model.TiposInmueble = _dal.ObtenerTiposInmueble()
                    .Select(r => new TipoInmuebleDto { tipo = ValString(r, "tipo"), cantidad = ValInt(r, "cantidad") }).ToList();

                // Distribución por estatus: solo del mes actual para que se “limpie” cada mes desde cero
                var distEstatus = _dal.ObtenerEstatusDistribucionPorMes(inicioMes, finMes)
                    .Select(r => new EstatusDistribucionDto { estatus = ValString(r, "estatus"), cantidad = ValInt(r, "cantidad") }).ToList();
                model.EstatusDistribucion = CompletarEstatusDistribucion(distEstatus);

                var pipeline = _dal.ObtenerPipeline()
                    .Select(r => new PipelineDto
                    {
                        empresa = ValString(r, "empresa"),
                        contacto = ValString(r, "contacto"),
                        estatus = ValString(r, "estatus"),
                        tipoInmueble = ValString(r, "tipoInmueble"),
                        fuente = ValString(r, "fuente"),
                        trato = ValString(r, "trato"),
                        monto = ValDecimal(r, "monto"),
                        fase = ValString(r, "fase"),
                        tieneSucursales = ValString(r, "tieneSucursales"),
                        vendedorNombre = ValString(r, "vendedorNombre"),
                        fecha = Val(r, "fecha")
                    }).ToList();

                pipeline = FiltrarPorRol(pipeline);
                model.Pipeline = pipeline;

                model.CotizacionesDetalle = _dal.ObtenerCotizacionesDetalle()
                    .Select(r => new CotizacionDetalleDto
                    {
                        tipo_residuo = ValString(r, "tipo_residuo"),
                        frecuencia = ValString(r, "frecuencia"),
                        periodicidad_pago = ValString(r, "periodicidad_pago"),
                        volumen_estimado = ValDecimal(r, "volumen_estimado"),
                        precio_unitario = ValDecimal(r, "precio_unitario"),
                        trato = ValString(r, "trato"),
                        empresa = ValString(r, "empresa")
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

        private List<PipelineDto> FiltrarPorRol(List<PipelineDto> pipeline)
        {
            var rol = Session["Rol"]?.ToString() ?? "";
            if (AppRoles.EsSupervisorOAdmin(rol))
                return pipeline;

            // Vendedor: solo registros asignados a él
            var usuarioNombre = Session["UsuarioNombre"]?.ToString() ?? "";
            return pipeline
                .Where(p => !string.IsNullOrWhiteSpace(p.vendedorNombre)
                    && p.vendedorNombre.Equals(usuarioNombre, StringComparison.OrdinalIgnoreCase))
                .ToList();
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
                    .Select(r => new EstatusDistribucionDto { estatus = ValString(r, "estatus"), cantidad = ValInt(r, "cantidad") }).ToList();

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
