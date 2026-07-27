using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CRMSistema.Controllers.Base;
using CRMSistema.DAL.Contratos;
using CRMSistema.DAL.Cotizador;
using CRMSistema.DAL.Prospectos;
using CRMSistema.Filters;
using CRMSistema.Models.Contratos;
using CRMSistema.Models.Usuarios;
using Newtonsoft.Json;

namespace CRMSistema.Controllers.ContratosPorAutorizar
{
    [AuthorizeRole(AppRoles.Supervisor, AppRoles.Superadmin)]
    public class ContratosPorAutorizarController : BaseController
    {
        private readonly ContratosDAL _dal = new ContratosDAL();
        private readonly ApiProspectosDAL _prospectosDal = new ApiProspectosDAL();
        private readonly TratosDAL _tratosDal = new TratosDAL();
        private readonly CotizacionesDAL _cotizacionesDal = new CotizacionesDAL();

        public ActionResult Index()
        {
            ViewBag.Title = "Contratos por Autorizar";
            ViewBag.ActiveMenu = "ContratosPorAutorizar";
            return View();
        }

        [HttpGet]
        public ActionResult GetPendientes()
        {
            try
            {
                var data = _dal.ObtenerContratosPorAutorizar()
                    .Where(PuedeVerContrato)
                    .Select(c => new
                    {
                        c.Contrato_ID,
                        c.Prospecto_ID,
                        c.Validacion_ID,
                        c.Folio,
                        c.Monto_Mensual,
                        c.Estatus,
                        c.Motivo_Rechazo,
                        c.Usuario_Rechaza,
                        Fecha_Rechazo = FormatearFecha(c.Fecha_Rechazo),
                        Fecha_Autorizacion = FormatearFecha(c.Fecha_Autorizacion),
                        c.RazonSocial,
                        c.RFC,
                        c.Calle,
                        c.Num_Ext,
                        c.Num_Int,
                        c.Colonia,
                        c.Municipio,
                        c.CP,
                        c.Estado,
                        c.Telefono,
                        c.Correo,
                        c.Contacto,
                        c.Nombre_Comercial,
                        c.Tipo_Persona,
                        c.Referencias,
                        c.Folio_Catastral,
                        c.Dias_Disponibles,
                        c.Horario,
                        c.Ruta,
                        c.VendedorNombre
                    })
                    .ToList();
                return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private bool PuedeVerContrato(ContratoAutorizadoModel c)
        {
            var rol = Session["Rol"]?.ToString() ?? "";
            if (AppRoles.EsSupervisorOAdmin(rol))
                return true;

            var usuarioNombre = Session["UsuarioNombre"]?.ToString() ?? "";
            return !string.IsNullOrWhiteSpace(c.VendedorNombre)
                && c.VendedorNombre.Equals(usuarioNombre, StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet]
        public ActionResult GetDetalle(int id)
        {
            try
            {
                var contrato = _dal.ObtenerPorId(id);
                if (contrato == null)
                    return Json(new { success = false, error = "Contrato no encontrado." }, JsonRequestBehavior.AllowGet);

                if (!PuedeVerContrato(contrato))
                    return Json(new { success = false, error = "No tienes permiso para ver este contrato." }, JsonRequestBehavior.AllowGet);

                var prospectos = _prospectosDal.ObtenerTodos();
                object prospecto = null;
                foreach (var p in prospectos)
                {
                    if (ObtenerProspectoId(p) == contrato.Prospecto_ID)
                    {
                        prospecto = ProspectoLimpio(p);
                        break;
                    }
                }

                var tratos = _tratosDal.ObtenerPorProspecto(contrato.Prospecto_ID);
                var trato = tratos?.FirstOrDefault(t =>
                    (t.Nombre_Trato ?? "").Equals(contrato.Folio, StringComparison.OrdinalIgnoreCase));

                int tratoId = trato?.Trato_ID ?? 0;
                if (trato == null || tratoId <= 0)
                {
                    var validacion = _cotizacionesDal.ObtenerValidacionPorId(contrato.Validacion_ID);
                    if (validacion != null)
                    {
                        decimal total;
                        tratoId = _tratosDal.SincronizarTratoDesdeCotizacion(
                            contrato.Prospecto_ID, contrato.Folio, validacion.Datos_Cotizacion, out total);
                        if (total > 0)
                            _dal.ActualizarMontoMensual(contrato.Contrato_ID, total);
                    }
                }

                var servicios = new List<Models.Cotizador.ServicioCotizadoModel>();
                if (tratoId > 0)
                {
                    servicios = _cotizacionesDal.ObtenerServiciosCotizados(tratoId);
                }

                var sucursales = _prospectosDal.ObtenerSucursales(contrato.Prospecto_ID);

                return Json(new
                {
                    success = true,
                    contrato = new
                    {
                        contrato.Contrato_ID,
                        contrato.Prospecto_ID,
                        contrato.Validacion_ID,
                        contrato.Folio,
                        contrato.Monto_Mensual,
                        contrato.Estatus,
                        contrato.Motivo_Rechazo,
                        contrato.Usuario_Rechaza,
                        Fecha_Rechazo = FormatearFecha(contrato.Fecha_Rechazo),
                        Fecha_Autorizacion = FormatearFecha(contrato.Fecha_Autorizacion),
                        contrato.RazonSocial,
                        contrato.RFC,
                        contrato.Calle,
                        contrato.Num_Ext,
                        contrato.Num_Int,
                        contrato.Colonia,
                        contrato.Municipio,
                        contrato.CP,
                        contrato.Estado,
                        contrato.Telefono,
                        contrato.Correo,
                        contrato.Contacto,
                        contrato.Nombre_Comercial,
                        contrato.Tipo_Persona,
                        contrato.Referencias,
                        contrato.Folio_Catastral,
                        contrato.Dias_Disponibles,
                        contrato.Horario,
                        contrato.Ruta,
                        contrato.VendedorNombre
                    },
                    prospecto,
                    tratos,
                    servicios,
                    sucursales = SucursalesLimpias(sucursales)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Autorizar(int id)
        {
            try
            {
                var contrato = _dal.ObtenerPorId(id);
                if (contrato == null)
                    return Json(new { success = false, error = "Contrato no encontrado." });

                _dal.ActualizarEstatus(id, "Activo");
                return Json(new { success = true, message = "Contrato autorizado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Rechazar(ContratoRechazoRequest req)
        {
            try
            {
                var contrato = _dal.ObtenerPorId(req.contrato_id);
                if (contrato == null)
                    return Json(new { success = false, error = "Contrato no encontrado." });

                if (string.IsNullOrWhiteSpace(req.motivo))
                    return Json(new { success = false, error = "Debes indicar el motivo de rechazo." });

                _dal.ActualizarEstatus(req.contrato_id, "Rechazado", req.motivo, User.Identity.Name);
                return Json(new { success = true, message = "Contrato rechazado y devuelto al vendedor." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        #region Helpers

        private static string FormatearFecha(DateTime? fecha)
        {
            return fecha?.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        private static string FormatearFecha(DateTime fecha)
        {
            return fecha.ToString("yyyy-MM-ddTHH:mm:ss");
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
            var campos = new[] { "id", "nombre", "rfc", "email", "telefono", "nombreComercial", "calle", "numExt", "numInt", "colonia", "municipio", "cp", "estado", "vendedorNombre", "tipoPersona", "tipoInmueble", "tieneSucursales", "estatus", "fecha", "referencias", "folioCatastral", "dias_disponibles", "horario", "ruta" };
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

        #endregion
    }
}
