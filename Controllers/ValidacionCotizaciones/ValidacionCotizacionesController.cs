using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CRMSistema.Controllers.Base;
using CRMSistema.DAL.Cotizador;
using CRMSistema.DAL.Prospectos;
using CRMSistema.Models.Cotizador;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CRMSistema.Controllers.ValidacionCotizaciones
{
    [Authorize]
    public class ValidacionCotizacionesController : BaseController
    {
        private readonly CotizacionesDAL _dal = new CotizacionesDAL();
        private readonly ApiProspectosDAL _prospectosDal = new ApiProspectosDAL();

        public ActionResult Index()
        {
            ViewBag.Title = "Cotizaciones por Aprobar";
            ViewBag.ActiveMenu = "CotizacionesPorAprobar";
            return View();
        }

        private int ObtenerProspectoId(object p)
        {
            if (p == null) return 0;
            var dic = p as IDictionary<string, object>;
            if (dic == null) return 0;
            foreach (var key in new[] { "Prospecto_ID", "id", "Id" })
            {
                if (dic.TryGetValue(key, out var val) && val != null)
                    return ToInt(val);
            }
            return 0;
        }

        private object ProspectoLimpio(object p)
        {
            if (p == null) return null;
            var dic = p as IDictionary<string, object>;
            if (dic == null) return p;
            var campos = new[] { "id", "nombre", "rfc", "email", "telefono", "nombreComercial", "calle", "numExt", "colonia", "municipio", "cp", "estado", "vendedorNombre", "tipoPersona", "tipoInmueble", "tieneSucursales", "estatus", "fecha" };
            var result = new Dictionary<string, object>();
            foreach (var key in campos)
            {
                if (dic.TryGetValue(key, out var val) && val != null && !(val is byte[]))
                    result[key] = val;
            }
            return result;
        }

        private List<object> SucursalesLimpias(List<dynamic> sucursales)
        {
            if (sucursales == null) return new List<object>();
            var campos = new[] { "id", "nombre_sucursal", "correo_electronico", "telefono_sucursal", "nombre_responsable", "calle", "numExt", "numInt", "colonia", "municipio", "cp", "estado", "concesionaria", "referencias", "folio_catastral" };
            var lista = new List<object>();
            foreach (var s in sucursales)
            {
                var dic = s as IDictionary<string, object>;
                if (dic == null) continue;
                var item = new Dictionary<string, object>();
                foreach (var key in campos)
                {
                    if (dic.TryGetValue(key, out var val) && val != null && !(val is byte[]))
                        item[key] = val;
                }
                lista.Add(item);
            }
            return lista;
        }

        [HttpGet]
        public ActionResult GetPendientes()
        {
            try
            {
                var data = _dal.ObtenerValidacionesPendientes();
                return JsonContent(new { success = true, data });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult GetDetalle(int id)
        {
            try
            {
                var validacion = _dal.ObtenerValidacionPorId(id);
                if (validacion == null)
                    return JsonContent(new { success = false, error = "No se encontró la validación." });

                var prospectos = _prospectosDal.ObtenerTodos();
                object prospecto = null;
                foreach (var p in prospectos)
                {
                    if (ObtenerProspectoId(p) == validacion.Prospecto_ID)
                    {
                        prospecto = p;
                        break;
                    }
                }

                var sucursales = _prospectosDal.ObtenerSucursales(validacion.Prospecto_ID);

                return JsonContent(new { success = true, validacion, prospecto = ProspectoLimpio(prospecto), sucursales = SucursalesLimpias(sucursales) });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Rechazar(CotizacionValidacionRequest req)
        {
            try
            {
                _dal.ActualizarEstatusValidacion(req.validacion_id ?? 0, "Rechazada", req.motivo, User.Identity.Name);
                return JsonContent(new { success = true, message = "Cotización rechazada." });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Autorizar(int id)
        {
            try
            {
                var validacion = _dal.ObtenerValidacionPorId(id);
                if (validacion == null)
                    return JsonContent(new { success = false, error = "Validación no encontrada." });

                _dal.ActualizarEstatusValidacion(id, "Autorizada", null, User.Identity.Name);

                decimal monto = 0;
                try
                {
                    var datos = JObject.Parse(validacion.Datos_Cotizacion ?? "{}");
                    var totales = datos["totales"];
                    if (totales != null)
                        monto = totales["total"] != null ? Convert.ToDecimal(totales["total"]) : 0;
                }
                catch { }

                var folio = $"COT-{DateTime.Now:yyyy}-{id.ToString().PadLeft(4, '0')}";
                var cDal = new CRMSistema.DAL.Contratos.ContratosDAL();
                cDal.CrearContratoAutorizado(new Models.Contratos.ContratoAutorizadoModel
                {
                    Prospecto_ID = validacion.Prospecto_ID,
                    Validacion_ID = id,
                    Folio = folio,
                    Monto_Mensual = monto,
                    Autorizado_Por = User.Identity.Name
                });

                try
                {
                    var pDal = new CRMSistema.DAL.Prospectos.ApiProspectosDAL();
                    pDal.ActualizarEstatus(validacion.Prospecto_ID, "Aprobado");
                }
                catch { }

                return JsonContent(new { success = true, message = "Cotización autorizada y contrato creado.", folio });
            }
            catch (Exception ex)
            {
                return JsonContent(new { success = false, error = ex.Message });
            }
        }
    }
}
