using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CRMSistema.DAL.Cotizador;
using CRMSistema.Models.Cotizador;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CRMSistema.Controllers.Cotizador
{
    [Authorize]
    public class CotizadorController : Controller
    {
        private readonly CotizacionesDAL _dal = new CotizacionesDAL();
        private readonly TratosDAL _tratosDal = new TratosDAL();

        public ActionResult Index()
        {
            ViewBag.Title = "Cotizador";
            ViewBag.ActiveMenu = "Cotizador";
            return View();
        }

        public ActionResult Generar(int? prospectoId)
        {
            ViewBag.Title = "Generar Cotización";
            ViewBag.ActiveMenu = "Cotizador";
            ViewBag.ProspectoId = prospectoId;
            return View();
        }

        [HttpGet]
        public ActionResult GetServiciosResiduos()
        {
            try { return Content(JsonConvert.SerializeObject(new { success = true, data = _dal.ObtenerServiciosResiduos() }), "application/json"); }
            catch (Exception ex) { return Content(JsonConvert.SerializeObject(new { success = false, error = ex.Message }), "application/json"); }
        }

        [HttpGet]
        public ActionResult GetConfiguradorUnidades()
        {
            try { return Content(JsonConvert.SerializeObject(new { success = true, data = _dal.ObtenerUnidadesRme() }), "application/json"); }
            catch (Exception ex) { return Content(JsonConvert.SerializeObject(new { success = false, error = ex.Message }), "application/json"); }
        }

        [HttpGet]
        public ActionResult GetBorradores(int id)
        {
            try { return Content(JsonConvert.SerializeObject(new { success = true, data = _dal.ObtenerBorradores(id) }), "application/json"); }
            catch (Exception ex) { return Content(JsonConvert.SerializeObject(new { success = false, error = ex.Message }), "application/json"); }
        }

        [HttpPost]
        public ActionResult CreateBorrador(int id, string datos)
        {
            try
            {
                int insertId = _dal.CrearBorrador(id, datos);
                return Json(new { success = true, id = insertId });
            }
            catch (Exception ex) { return Json(new { success = false, error = ex.Message }); }
        }

        [HttpPost]
        public ActionResult DeleteBorrador(int id)
        {
            try
            {
                _dal.EliminarBorrador(id);
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, error = ex.Message }); }
        }

        [HttpPost]
        public ActionResult EnviarCotizacion(EnviarCotizacionRequest req)
        {
            try
            {
                var rand = new Random();
                string password_temporal = rand.Next(10000000, 99999999).ToString() + "x!";
                _dal.EnviarCotizacion(req.prospecto_id, req.email, req.nombre, password_temporal);
                return Json(new { success = true, message = "Cotización enviada y estatus actualizado.", password_temporal });
            }
            catch (Exception ex) { return Json(new { success = false, error = ex.Message }); }
        }

        [HttpPost]
        public ActionResult CreateTrato(CrearTratoRequest req)
        {
            try
            {
                var model = new TratoModel
                {
                    prospecto_id = req.prospecto_id,
                    nombre_trato = req.nombre_trato,
                    importe = req.importe,
                    fase_id = req.fase_id,
                    fecha_limite_cotizacion = req.fecha_limite_cotizacion
                };
                int insertId = _tratosDal.Crear(model);
                return Json(new { success = true, id = insertId });
            }
            catch (Exception ex) { return Json(new { success = false, error = ex.Message }); }
        }

        [HttpPost]
        public ActionResult CreateServicioCotizado(ServicioCotizadoModel req)
        {
            try
            {
                long insertId = _dal.CrearServicioCotizado(req);
                return Json(new { success = true, id = insertId });
            }
            catch (Exception ex) { return Json(new { success = false, error = ex.Message }); }
        }

        [HttpGet]
        public ActionResult GetProspectos()
        {
            try
            {
                var pDal = new CRMSistema.DAL.Prospectos.ApiProspectosDAL();
                var data = pDal.ObtenerTodos();
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(data), "application/json");
            }
            catch (Exception ex) { return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { error = ex.Message }), "application/json"); }
        }

        [HttpGet]
        public ActionResult GetSucursales(int id)
        {
            try
            {
                var pDal = new CRMSistema.DAL.Prospectos.ApiProspectosDAL();
                var data = pDal.ObtenerSucursales(id);
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(data), "application/json");
            }
            catch (Exception ex) { return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new { error = ex.Message }), "application/json"); }
        }

        [HttpPost]
        public ActionResult SolicitarValidacion(CotizacionValidacionRequest req)
        {
            try
            {
                int borradorId = req.borrador_id ?? 0;
                if (borradorId <= 0)
                {
                    if (string.IsNullOrWhiteSpace(req.datos))
                        return Json(new { success = false, error = "No hay datos de cotización para enviar a validación." });

                    borradorId = _dal.CrearBorrador(req.prospecto_id, req.datos);
                    if (borradorId <= 0)
                        return Json(new { success = false, error = "No se pudo guardar el borrador." });
                }

                var datos = req.datos;
                if (string.IsNullOrWhiteSpace(datos))
                {
                    var borradores = _dal.ObtenerBorradores(req.prospecto_id);
                    var borrador = borradores.FirstOrDefault(b => b.Borrador_ID == borradorId);
                    datos = borrador?.Datos_Borrador ?? "{}";
                }

                var validacion = new CotizacionValidacionModel
                {
                    Prospecto_ID = req.prospecto_id,
                    Borrador_ID = borradorId,
                    Datos_Cotizacion = datos,
                    Usuario_Creacion = User.Identity.Name
                };

                int validacionId = _dal.CrearValidacion(validacion);

                try
                {
                    GenerarArchivoResumenHtml(validacionId, req.prospecto_id, datos);
                }
                catch (Exception exFile)
                {
                    // No detenemos el flujo si falla la escritura del archivo.
                    System.Diagnostics.Debug.WriteLine("Error generando archivo resumen: " + exFile.Message);
                }

                return Json(new { success = true, validacion_id = validacionId, borrador_id = borradorId, message = "Cotización enviada a validación." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private static string ObtenerValorString(dynamic obj, params string[] claves)
        {
            if (obj == null) return null;
            var dic = obj as IDictionary<string, object>;
            foreach (var clave in claves)
            {
                if (dic != null)
                {
                    if (dic.TryGetValue(clave, out var val) && val != null)
                        return val.ToString();
                }
                else
                {
                    try
                    {
                        var val = ((object)obj).GetType().GetProperty(clave)?.GetValue(obj, null);
                        if (val != null) return val.ToString();
                    }
                    catch { }
                }
            }
            return null;
        }

        private static int ObtenerValorInt(dynamic obj, params string[] claves)
        {
            if (obj == null) return 0;
            var s = ObtenerValorString(obj, claves);
            int n;
            if (int.TryParse(s, out n)) return n;
            return 0;
        }

        private void GenerarArchivoResumenHtml(int validacionId, int prospectoId, string datosJson)
        {
            string rutaBase = ConfigurationManager.AppSettings["RutaArchivosValidacion"]
                ?? @"C:\Users\Joel Pool\Downloads\PROCESO MOCKUPS\01-PROCESO\";

            if (!Directory.Exists(rutaBase))
                Directory.CreateDirectory(rutaBase);

            string fileName = $"Validacion_{validacionId}.html";
            string filePath = Path.Combine(rutaBase, fileName);

            var prospectos = new CRMSistema.DAL.Prospectos.ApiProspectosDAL().ObtenerTodos();
            var prospecto = prospectos.FirstOrDefault(p =>
            {
                int idProspecto = ObtenerValorInt(p, "Prospecto_ID", "id", "Id");
                return idProspecto == prospectoId;
            });

            string razonSocial = ObtenerValorString(prospecto, "Nombre", "nombre", "Nombre_Empresa", "nombreEmpresa") ?? "Prospecto";
            string rfc = ObtenerValorString(prospecto, "RFC", "rfc") ?? "—";
            string vendedor = ObtenerValorString(prospecto, "Vendedor_Nombre", "vendedorNombre", "Nombre", "nombre") ?? "Sin asignar";

            var datos = JObject.Parse(datosJson ?? "{}");
            string fechaCreacion = DateTime.Now.ToString("dd/MM/yyyy");
            string vigenciaInicio = datos["fechaInicio"]?.ToString() ?? DateTime.Now.ToString("yyyy-MM-dd");
            string vigenciaFin = datos["fechaLimite"]?.ToString() ?? DateTime.Now.AddDays(30).ToString("yyyy-MM-dd");
            string folio = $"COT-{DateTime.Now:yyyy}-{validacionId.ToString().PadLeft(4, '0')}";

            string html = $@"<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <title>Resumen de Cotización - {razonSocial}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 24px; color: #333; }}
        .header {{ background: #4E342E; color: white; padding: 16px 24px; display: flex; justify-content: space-between; align-items: center; border-radius: 8px 8px 0 0; }}
        .header h1 {{ margin: 0; font-size: 22px; }}
        .badge {{ background: #FFE0B2; color: #E65100; padding: 6px 14px; border-radius: 20px; font-weight: bold; font-size: 13px; }}
        .info {{ display: grid; grid-template-columns: repeat(5, 1fr); gap: 16px; padding: 16px 0; border-bottom: 2px solid #C4A574; margin-bottom: 24px; }}
        .info-item div {{ font-size: 11px; color: #666; text-transform: uppercase; margin-bottom: 4px; }}
        .info-item strong {{ font-size: 14px; color: #333; }}
        .section-title {{ color: #A16A34; font-size: 14px; font-weight: bold; text-transform: uppercase; border-bottom: 2px solid #C4A574; padding-bottom: 8px; margin-bottom: 16px; }}
        .card {{ border: 1px solid #E0E0E0; border-radius: 8px; padding: 16px; background: #FAFAFA; min-width: 200px; }}
        .cards {{ display: flex; gap: 16px; margin-bottom: 24px; }}
        table {{ width: 100%; border-collapse: collapse; font-size: 12px; border: 1px solid #4E342E; }}
        th {{ background: #8D6E63; color: white; padding: 10px; border: 1px solid #4E342E; text-align: left; }}
        td {{ padding: 10px; border: 1px solid #ccc; }}
        .summary {{ width: 400px; margin-left: auto; border: 1px solid #C4A574; border-radius: 8px; padding: 16px; background: #FFFDE7; }}
        .summary-row {{ display: flex; justify-content: space-between; margin-bottom: 8px; }}
        .total {{ background: #3E2723; color: white; padding: 12px; border-radius: 4px; font-weight: bold; font-size: 15px; display: flex; justify-content: space-between; }}
        .badge-tipo {{ color: white; padding: 3px 8px; border-radius: 4px; font-weight: bold; font-size: 11px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>{folio} | {razonSocial}</h1>
        <span class='badge'>PENDIENTE DE APROBACIÓN</span>
    </div>
    <div class='info'>
        <div class='info-item'><div>Razón Social</div><strong>{razonSocial}</strong></div>
        <div class='info-item'><div>RFC</div><strong>{rfc}</strong></div>
        <div class='info-item'><div>Vendedor</div><strong>{vendedor}</strong></div>
        <div class='info-item'><div>Fecha de creación</div><strong>{fechaCreacion}</strong></div>
        <div class='info-item'><div>Vigencia</div><strong>{vigenciaInicio} al {vigenciaFin}</strong></div>
    </div>
    <div class='section-title'>Resumen Consolidado - Todas las Sucursales</div>
    <div class='cards'>
        <div class='card'><strong>MATRIZ</strong><br/>Resumen matriz</div>
        <div class='card'><strong>SUCURSALES</strong><br/>Ver detalle en sistema</div>
    </div>
    <div class='section-title'>Servicios Cotizados</div>
    <table>
        <thead>
            <tr><th>Servicio</th><th>Producto</th><th>Ruta</th><th>Días</th><th>Cantidad</th><th>Precio Unit.</th><th>% Adicional</th><th>Subtotal</th></tr>
        </thead>
        <tbody>
            <tr><td colspan='8' style='text-align:center;'>Detalle disponible en el módulo de Cotizaciones por Aprobar.</td></tr>
        </tbody>
    </table>
    <div class='summary'>
        <div class='summary-row'><span>Subtotal servicios</span><span>Ver en sistema</span></div>
        <div class='summary-row'><span>IVA (16%)</span><span>Ver en sistema</span></div>
        <div class='summary-row'><span>Descuento</span><span>Ver en sistema</span></div>
        <div class='total'><span>TOTAL MENSUAL</span><span>Ver en sistema</span></div>
    </div>
</body>
</html>";

            System.IO.File.WriteAllText(filePath, html);
        }
    }
}
