using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CRMSistema.Controllers.Base;
using CRMSistema.DAL.Contratos;
using CRMSistema.DAL.Cotizador;
using CRMSistema.DAL.Prospectos;
using CRMSistema.Filters;
using CRMSistema.Models.Contratos;
using CRMSistema.Models.Usuarios;
using Newtonsoft.Json;

namespace CRMSistema.Controllers.Contratos
{
    [AuthorizeRole(AppRoles.Vendedor, AppRoles.Supervisor, AppRoles.Coordinador, AppRoles.Jefe, AppRoles.Superadmin)]
    public class ContratosController : BaseController
    {
        private readonly ContratosDAL _dal = new ContratosDAL();
        private readonly ApiProspectosDAL _prospectosDal = new ApiProspectosDAL();
        private readonly TratosDAL _tratosDal = new TratosDAL();
        private readonly CotizacionesDAL _cotizacionesDal = new CotizacionesDAL();

        public ActionResult Index()
        {
            ViewBag.Title = "Contratos";
            ViewBag.ActiveMenu = "Contratos";
            return View();
        }

        [HttpGet]
        public ActionResult GetContratosPorFirmar()
        {
            try
            {
                // Mostramos todos los contratos; la vista filtra por estatus.
                var data = _dal.ObtenerContratosAutorizados()
                    .Where(c => base.PuedeVerContrato(c))
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

        // PuedeVerContrato ahora vive en BaseController.

        [HttpGet]
        public ActionResult GetDetalle(int id)
        {
            try
            {
                var contrato = _dal.ObtenerPorId(id);
                if (contrato == null)
                    return Json(new { success = false, error = "Contrato no encontrado." }, JsonRequestBehavior.AllowGet);

                if (!base.PuedeVerContrato(contrato))
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
                    // Si no existe trato (cotizaciones autorizadas antes del cambio), regenerarlo
                    // desde el JSON de la validación para poder mostrar servicios reales.
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
                var serviciosResiduos = _cotizacionesDal.ObtenerServiciosResiduos();
                var unidadesRme = _cotizacionesDal.ObtenerUnidadesRme();

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
                    sucursales = SucursalesLimpias(sucursales),
                    catalogos = new
                    {
                        serviciosResiduos,
                        unidadesRme
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult EnviarContrato(int id)
        {
            try
            {
                var contrato = _dal.ObtenerPorId(id);
                if (contrato == null)
                    return Json(new { success = false, error = "Contrato no encontrado." });

                if (!base.PuedeVerContrato(contrato))
                    return Json(new { success = false, error = "No tienes permiso para modificar este contrato." });

                _dal.ActualizarEstatus(id, "Enviado");
                return Json(new { success = true, message = "Contrato enviado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult MarcarFirmado(int id)
        {
            try
            {
                var contrato = _dal.ObtenerPorId(id);
                if (contrato == null)
                    return Json(new { success = false, error = "Contrato no encontrado." });

                if (!base.PuedeVerContrato(contrato))
                    return Json(new { success = false, error = "No tienes permiso para modificar este contrato." });

                _dal.ActualizarEstatus(id, "Firmado");
                _prospectosDal.ActualizarEstatus(contrato.Prospecto_ID, "Firmado");
                return Json(new { success = true, message = "Contrato firmado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult EnviarARevision(int id)
        {
            try
            {
                var contrato = _dal.ObtenerPorId(id);
                if (contrato == null)
                    return Json(new { success = false, error = "Contrato no encontrado." });

                if (!base.PuedeVerContrato(contrato))
                    return Json(new { success = false, error = "No tienes permiso para modificar este contrato." });

                if ((contrato.Estatus ?? "").Equals("Por Autorizar", StringComparison.OrdinalIgnoreCase))
                    return Json(new { success = false, error = "El contrato ya está en revisión." });

                _dal.ActualizarEstatus(id, "Por Autorizar");
                return Json(new { success = true, message = "Contrato enviado a revisión del supervisor." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult GuardarServicios(Models.Contratos.GuardarServiciosRequest req)
        {
            try
            {
                int id = req?.contrato_id ?? 0;
                var servicios = req?.servicios ?? new List<Models.Cotizador.ServicioCotizadoModel>();

                var contrato = _dal.ObtenerPorId(id);
                if (contrato == null)
                    return Json(new { success = false, error = "Contrato no encontrado." });

                if (!base.PuedeVerContrato(contrato))
                    return Json(new { success = false, error = "No tienes permiso para modificar este contrato." });

                if (servicios.Count == 0)
                    return Json(new { success = false, error = "No se recibieron servicios." });

                // Actualizar datos del cliente si vienen en el payload
                if (req?.cliente != null)
                {
                    var cliente = req.cliente;
                    var prospecto = _prospectosDal.ObtenerTodos()
                        .Cast<object>()
                        .FirstOrDefault(p => ObtenerProspectoId(p) == contrato.Prospecto_ID);

                    var dic = prospecto as System.Collections.Generic.IDictionary<string, object>;

                    var modelo = new Models.Prospectos.ApiProspectoModel
                    {
                        id = contrato.Prospecto_ID.ToString(),
                        nombre = cliente.razon_social,
                        rfc = cliente.rfc,
                        contacto = cliente.representante_legal,
                        email = cliente.correo,
                        telefono = cliente.telefono,
                        nombreComercial = cliente.nombre_comercial,
                        folioCatastral = cliente.folio_catastral,
                        calle = ObtenerValorDireccion(cliente.domicilio_fiscal, 0),
                        numExt = ObtenerValorDireccion(cliente.domicilio_fiscal, 1),
                        numInt = ObtenerValorDireccion(cliente.domicilio_fiscal, 2),
                        colonia = ObtenerValorDireccion(cliente.domicilio_fiscal, 3),
                        municipio = ObtenerValorDireccion(cliente.domicilio_fiscal, 4),
                        cp = ObtenerValorDireccion(cliente.domicilio_fiscal, 5),
                        estado = ObtenerValorDireccion(cliente.domicilio_fiscal, 6),
                        domicilioFiscal = cliente.domicilio_fiscal ?? "",
                        domicilioRecoleccion = cliente.domicilio_recoleccion ?? "",
                        tipoPersona = dic != null && dic.ContainsKey("tipoPersona") ? dic["tipoPersona"]?.ToString() : "Moral",
                        tieneSucursales = dic != null && dic.ContainsKey("tieneSucursales") ? dic["tieneSucursales"]?.ToString() : "No",
                        estatus = dic != null && dic.ContainsKey("estatus") ? dic["estatus"]?.ToString() : "Nuevo",
                        tipoInmueble = dic != null && dic.ContainsKey("tipoInmueble") ? dic["tipoInmueble"]?.ToString() : "",
                        referencias = dic != null && dic.ContainsKey("referencias") ? dic["referencias"]?.ToString() : "",
                        dias_disponibles = dic != null && dic.ContainsKey("dias_disponibles") ? dic["dias_disponibles"]?.ToString() : "",
                        horario = dic != null && dic.ContainsKey("horario") ? dic["horario"]?.ToString() : "",
                        ruta = dic != null && dic.ContainsKey("ruta") ? dic["ruta"]?.ToString() : "",
                        concesionaria = dic != null && dic.ContainsKey("concesionaria") ? dic["concesionaria"]?.ToString() : ""
                    };

                    _prospectosDal.ActualizarBasicoDesdeContrato(contrato.Prospecto_ID, modelo, cliente.representante_legal);
                }

                var tratos = _tratosDal.ObtenerPorProspecto(contrato.Prospecto_ID);
                var trato = tratos?.FirstOrDefault(t =>
                    (t.Nombre_Trato ?? "").Equals(contrato.Folio, StringComparison.OrdinalIgnoreCase));

                int tratoId = trato?.Trato_ID ?? 0;
                if (trato == null || tratoId <= 0)
                {
                    var validacion = _cotizacionesDal.ObtenerValidacionPorId(contrato.Validacion_ID);
                    if (validacion == null)
                        return Json(new { success = false, error = "No se encontró la cotización base para generar el trato." });

                    decimal total;
                    tratoId = _tratosDal.SincronizarTratoDesdeCotizacion(
                        contrato.Prospecto_ID, contrato.Folio, validacion.Datos_Cotizacion, out total);
                }

                if (tratoId <= 0)
                    return Json(new { success = false, error = "No se pudo obtener o crear el trato asociado." });

                var existentes = _cotizacionesDal.ObtenerServiciosCotizados(tratoId);

                foreach (var s in servicios)
                {
                    s.trato_id = tratoId;
                    if (s.id > 0)
                    {
                        var existente = existentes.FirstOrDefault(e => e.id == s.id);
                        if (existente != null)
                            _cotizacionesDal.ActualizarServicioCotizado(s.id, s);
                        else
                            _cotizacionesDal.CrearServicioCotizado(s);
                    }
                    else
                    {
                        _cotizacionesDal.CrearServicioCotizado(s);
                    }
                }

                // Recalcular total mensual
                var actualizados = _cotizacionesDal.ObtenerServiciosCotizados(tratoId);
                decimal subtotal = CalcularSubtotalServicios(actualizados);
                decimal totalMensual = subtotal * 1.16m;
                _dal.ActualizarMontoMensual(id, totalMensual);

                // Volver a enviar a autorización
                _dal.ActualizarEstatus(id, "Por Autorizar");

                return Json(new { success = true, message = "Servicios actualizados. El contrato volvió a 'Por Autorizar'.", folio = contrato.Folio, monto_mensual = totalMensual });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult GuardarDireccionProspecto(Models.Contratos.GuardarDireccionProspectoRequest req)
        {
            try
            {
                int contratoId = req?.contrato_id ?? 0;
                var contrato = _dal.ObtenerPorId(contratoId);
                if (contrato == null)
                    return Json(new { success = false, error = "Contrato no encontrado." });

                if (!base.PuedeVerContrato(contrato))
                    return Json(new { success = false, error = "No tienes permiso para modificar este contrato." });

                string domFiscal = FormatearDireccion(
                    req.fiscal_calle, req.fiscal_num_ext, req.fiscal_num_int,
                    req.fiscal_colonia, req.fiscal_municipio, req.fiscal_cp, req.fiscal_estado);

                string domRecoleccion = FormatearDireccion(
                    req.recoleccion_calle, req.recoleccion_num_ext, req.recoleccion_num_int,
                    req.recoleccion_colonia, req.recoleccion_municipio, req.recoleccion_cp, req.recoleccion_estado);

                var prospecto = _prospectosDal.ObtenerTodos()
                    .Cast<object>()
                    .FirstOrDefault(p => ObtenerProspectoId(p) == contrato.Prospecto_ID);
                var dic = prospecto as System.Collections.Generic.IDictionary<string, object>;

                var modelo = new Models.Prospectos.ApiProspectoModel
                {
                    id = contrato.Prospecto_ID.ToString(),
                    nombre = getProp(dic, "nombre") ?? contrato.RazonSocial,
                    rfc = getProp(dic, "rfc") ?? contrato.RFC,
                    contacto = getProp(dic, "contacto") ?? contrato.Contacto,
                    email = getProp(dic, "email") ?? contrato.Correo,
                    telefono = getProp(dic, "telefono") ?? contrato.Telefono,
                    nombreComercial = getProp(dic, "nombreComercial") ?? contrato.Nombre_Comercial,
                    folioCatastral = req.folio_catastral ?? "",
                    calle = req.fiscal_calle,
                    numExt = req.fiscal_num_ext,
                    numInt = req.fiscal_num_int,
                    colonia = req.fiscal_colonia,
                    municipio = req.fiscal_municipio,
                    cp = req.fiscal_cp,
                    estado = req.fiscal_estado,
                    domicilioFiscal = domFiscal,
                    domicilioRecoleccion = domRecoleccion,
                    tipoPersona = getProp(dic, "tipoPersona") ?? "Moral",
                    tieneSucursales = getProp(dic, "tieneSucursales") ?? "No",
                    estatus = getProp(dic, "estatus") ?? "Nuevo",
                    tipoInmueble = getProp(dic, "tipoInmueble") ?? "",
                    referencias = getProp(dic, "referencias") ?? "",
                    dias_disponibles = getProp(dic, "dias_disponibles") ?? "",
                    horario = getProp(dic, "horario") ?? "",
                    ruta = getProp(dic, "ruta") ?? "",
                    concesionaria = getProp(dic, "concesionaria") ?? ""
                };

                _prospectosDal.ActualizarBasicoDesdeContrato(contrato.Prospecto_ID, modelo, null, true);

                _prospectosDal.ActualizarArchivosYFolio(
                    contrato.Prospecto_ID,
                    domFiscal,
                    domRecoleccion,
                    req.folio_catastral ?? "",
                    ToBytes(Request.Files["foto_fachada"]),
                    ToBytes(Request.Files["foto_acceso"]),
                    ToBytes(Request.Files["foto_referencia"]),
                    ToBytes(Request.Files["documento_catastral"]),
                    Request.Files["documento_catastral"]?.FileName);

                return Json(new { success = true, domicilio_fiscal = domFiscal, domicilio_recoleccion = domRecoleccion, folio_catastral = req.folio_catastral, message = "Dirección y archivos actualizados." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private static byte[] ToBytes(HttpPostedFileBase archivo)
        {
            if (archivo == null || archivo.ContentLength <= 0) return null;
            using (var ms = new MemoryStream())
            {
                archivo.InputStream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private static string ObtenerValorDireccion(string direccion, int indice)
        {
            if (string.IsNullOrWhiteSpace(direccion)) return "";
            var partes = direccion.Split(',').Select(p => p.Trim()).ToArray();
            return indice < partes.Length ? partes[indice] : "";
        }

        private static string FormatearDireccion(string calle, string numExt, string numInt, string colonia, string municipio, string cp, string estado)
        {
            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(calle)) partes.Add(calle.Trim());
            if (!string.IsNullOrWhiteSpace(numExt)) partes.Add("#" + numExt.Trim());
            if (!string.IsNullOrWhiteSpace(numInt)) partes.Add("Int. " + numInt.Trim());
            if (!string.IsNullOrWhiteSpace(colonia)) partes.Add(colonia.Trim());
            if (!string.IsNullOrWhiteSpace(municipio)) partes.Add(municipio.Trim());
            if (!string.IsNullOrWhiteSpace(cp)) partes.Add("C.P. " + cp.Trim());
            if (!string.IsNullOrWhiteSpace(estado)) partes.Add(estado.Trim());
            return string.Join(", ", partes);
        }

        #region Helpers

        private static string getProp(IDictionary<string, object> dic, string key)
        {
            if (dic == null || !dic.ContainsKey(key) || dic[key] == null) return null;
            return dic[key].ToString();
        }

        private static string FormatearFecha(DateTime fecha)
        {
            return fecha.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        private static string FormatearFecha(DateTime? fecha)
        {
            return fecha?.ToString("yyyy-MM-ddTHH:mm:ss");
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
            var campos = new[] { "id", "nombre", "rfc", "email", "telefono", "nombreComercial", "calle", "numExt", "numInt", "colonia", "municipio", "cp", "estado", "domicilioFiscal", "domicilioRecoleccion", "vendedorNombre", "tipoPersona", "tipoInmueble", "tieneSucursales", "estatus", "fecha", "referencias", "folioCatastral", "dias_disponibles", "horario", "ruta" };
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

        private decimal CalcularSubtotalServicios(List<Models.Cotizador.ServicioCotizadoModel> servicios)
        {
            decimal subtotal = 0;
            foreach (var s in servicios)
            {
                var dias = (s.dias_asignados ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                int diasPorSemana = dias.Count > 0 ? dias.Count : 1;
                decimal adicional = (s.porcentaje_adicional ?? 0) / 100m;
                decimal descuento = (s.porcentaje_descuento ?? 0) / 100m;
                string tipo = (s.tipo_residuo ?? "").ToUpperInvariant();
                bool isRsu = !tipo.Contains("ESPECIAL") && (tipo.Contains("RSU") || tipo.Contains("URBANO") || tipo.Contains("SÓLIDO") || (s.costo_tonelada ?? 0) == 0 && (s.costo_disposicion ?? 0) == 0);
                bool esUnico = (s.tipo_cobro ?? "").Equals("UNICO", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrWhiteSpace(s.fecha_unica);
                decimal baseCalc = 0;

                if (isRsu)
                {
                    decimal bolsas = s.volumen_estimado ?? 0;
                    decimal bolsasMensuales = (bolsas * diasPorSemana * 52m) / 12m;
                    baseCalc = bolsasMensuales * (s.precio_unitario ?? 18.60m);
                }
                else if (esUnico)
                {
                    decimal costoT = s.costo_tonelada ?? 0;
                    decimal costoD = s.costo_disposicion ?? 0;
                    baseCalc = costoT + costoD;
                }
                else
                {
                    decimal rec = s.recolectores ?? 0;
                    decimal viajesMensuales = rec * diasPorSemana * 4m;
                    decimal costoT = s.costo_tonelada ?? 0;
                    decimal costoD = s.costo_disposicion ?? 0;
                    baseCalc = viajesMensuales * (costoT + costoD);
                }

                subtotal += baseCalc * (1m + adicional) * (1m - descuento);
            }
            return subtotal;
        }

        #endregion
    }
}
