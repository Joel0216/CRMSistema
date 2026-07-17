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

                double ingMes = ToDouble(kpisData?.ingMes, 0.0);
                double ingAnt = ToDouble(kpisData?.ingAnt, 0.0);
                int prosMes = ToInt(kpisData?.prosMes, 0);
                int prosAnt = ToInt(kpisData?.prosAnt, 0);
                int cotServ = ToInt(kpisData?.cotServ, 0);
                int cotBorr = ToInt(kpisData?.cotBorr, 0);
                int totalP = ToInt(kpisData?.totalP, 0);
                int convP = ToInt(kpisData?.convP, 0);
                int deudores = ToInt(kpisData?.deudores, 0);
                int alCorriente = ToInt(kpisData?.alCorriente, 0);
                int prosSuc = ToInt(kpisData?.prosSuc, 0);
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

                model.EstatusDistribucion = _dal.ObtenerEstatusDistribucion()
                    .Select(r => new EstatusDistribucionDto { estatus = ToString(r.estatus), cantidad = ToInt(r.cantidad, 0) }).ToList();

                model.Pipeline = _dal.ObtenerPipeline()
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
                new EstatusDistribucionDto { estatus = "En seguimiento", cantidad = 12 },
                new EstatusDistribucionDto { estatus = "Cotizado", cantidad = 8 },
                new EstatusDistribucionDto { estatus = "Aprobado", cantidad = 3 }
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
