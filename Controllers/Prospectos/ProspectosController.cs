using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using CRMSistema.Controllers.Base;
using CRMSistema.DAL.Cotizador;
using CRMSistema.DAL.Prospectos;
using CRMSistema.DAL.Usuarios;
using CRMSistema.Models.Prospectos;
using CRMSistema.Models.ViewModels;
using Newtonsoft.Json;

namespace CRMSistema.Controllers.Prospectos
{
    [Authorize]
    public class ProspectosController : BaseController
    {
        private readonly ApiProspectosDAL _dal = new ApiProspectosDAL();
        private readonly UsuariosDAL _usuariosDal = new UsuariosDAL();
        private readonly bool _modoDevSinBd;

        public ProspectosController()
        {
            bool.TryParse(ConfigurationManager.AppSettings["ModoDesarrolloSinBD"], out _modoDevSinBd);
        }

        public ActionResult Index(string filtro = "", string estatus = "")
        {
            ViewBag.Title = "Prospectos";
            ViewBag.ActiveMenu = "Prospectos";

            var lista = ObtenerListaProspectos();

            if (!string.IsNullOrEmpty(filtro))
            {
                var q = filtro.ToLower();
                lista = lista.Where(p =>
                    (p.Nombre?.ToLower().Contains(q) ?? false) ||
                    (p.Contacto?.ToLower().Contains(q) ?? false) ||
                    (p.Rfc?.ToLower().Contains(q) ?? false)).ToList();
            }

            if (!string.IsNullOrEmpty(estatus))
                lista = lista.Where(p => p.Estatus == estatus).ToList();

            ViewBag.Filtro = filtro;
            ViewBag.EstatusFiltro = estatus;

            CargarViewBags();
            return View(lista);
        }

        private List<ProspectoViewModel> ObtenerListaProspectos()
        {
            try
            {
                var rows = _dal.ObtenerTodos();
                return rows.Select(MapListItem).ToList();
            }
            catch
            {
                if (_modoDevSinBd)
                    return GetSampleProspectos();
                throw;
            }
        }

        private ProspectoViewModel CargarProspectoCompleto(int id)
        {
            try
            {
                var row = _dal.ObtenerTodos().FirstOrDefault(r => ToInt(Val(r, "id")) == id);
                if (row == null) return null;

                var m = MapDetalle(row);
                m.Contactos = _dal.ObtenerContactos(id).Select(MapContacto).ToList();
                m.Sucursales = _dal.ObtenerSucursales(id).Select(MapSucursal).ToList();

                try
                {
                    var cotDal = new CotizacionesDAL();
                    var val = cotDal.ObtenerValidacionPorProspecto(id);
                    if (val != null)
                    {
                        m.EstatusCotizacion = val.Estatus;
                        m.MotivoRechazoCotizacion = val.Motivo_Rechazo;
                    }
                }
                catch { }

                return m;
            }
            catch
            {
                if (_modoDevSinBd)
                    return GetSampleProspectos().FirstOrDefault(p => p.Id == id);
                throw;
            }
        }

        public ActionResult PartialNuevo()
        {
            ViewBag.Accion = "Nuevo";
            CargarViewBags();
            return PartialView("_FormularioProspecto", new ProspectoViewModel
            {
                Estatus = "Nuevo",
                TipoPersona = "Moral",
                TieneSucursales = "No",
                Estado = "Yucatán"
            });
        }

        public ActionResult PartialEditar(int id)
        {
            var model = CargarProspectoCompleto(id);
            if (model == null) return HttpNotFound();

            ViewBag.Accion = "Editar";
            CargarViewBags(id);
            return PartialView("_FormularioProspecto", model);
        }

        public ActionResult PartialDetalle(int id)
        {
            var model = CargarProspectoCompleto(id);
            if (model == null) return HttpNotFound();

            ViewBag.Accion = "Detalle";
            CargarViewBags(id);
            return PartialView("_FormularioProspecto", model);
        }

        public ActionResult Nuevo()
        {
            ViewBag.Title = "Nuevo Prospecto";
            ViewBag.ActiveMenu = "Prospectos";
            ViewBag.Accion = "Nuevo";
            CargarViewBags();
            return View("Formulario", new ProspectoViewModel { Estatus = "Nuevo", TipoPersona = "Moral", TieneSucursales = "No" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Nuevo(ProspectoViewModel model)
        {
            ViewBag.Title = "Nuevo Prospecto";
            ViewBag.ActiveMenu = "Prospectos";
            ViewBag.Accion = "Nuevo";

            BindHijos(model);

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (Request.IsAjaxRequest())
                    return Json(new { success = false, error = "Datos incompletos: " + string.Join("; ", errores) });

                CargarViewBags();
                return View("Formulario", model);
            }

            try
            {
                var apiModel = ToApiModel(model);
                var empresaId = _dal.UpsertEmpresa((apiModel.nombre ?? "").Trim(), apiModel.rfc);
                var nuevoId = _dal.Crear(apiModel, empresaId, (apiModel.contacto ?? "").Trim(), (apiModel.nombre ?? "").Trim());
                _dal.InsertarSucursales(nuevoId, apiModel.sucursales);
                _dal.InsertarContactos(nuevoId, apiModel.contactos);
                TempData["Success"] = "Prospecto registrado correctamente.";

                if (Request.IsAjaxRequest())
                    return Json(new { success = true, id = nuevoId, message = "Prospecto registrado correctamente." });

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                    return Json(new { success = false, error = "Error al guardar: " + ex.Message });

                ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                CargarViewBags();
                return View("Formulario", model);
            }
        }

        public ActionResult Editar(int id)
        {
            ViewBag.Title = "Editar Prospecto";
            ViewBag.ActiveMenu = "Prospectos";
            ViewBag.Accion = "Editar";

            try
            {
                var row = _dal.ObtenerTodos().FirstOrDefault(r => ToInt(Val(r, "id")) == id);
                if (row == null) return HttpNotFound();

                var model = MapDetalle(row);
                model.Contactos = _dal.ObtenerContactos(id).Select(MapContacto).ToList();
                model.Sucursales = _dal.ObtenerSucursales(id).Select(MapSucursal).ToList();
                CargarViewBags(id);
                return View("Formulario", model);
            }
            catch
            {
                if (_modoDevSinBd)
                {
                    var model = GetSampleProspectos().FirstOrDefault(p => p.Id == id);
                    if (model == null) return HttpNotFound();
                    CargarViewBags(id);
                    return View("Formulario", model);
                }
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(int id, ProspectoViewModel model)
        {
            ViewBag.Title = "Editar Prospecto";
            ViewBag.ActiveMenu = "Prospectos";
            ViewBag.Accion = "Editar";

            BindHijos(model);

            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (Request.IsAjaxRequest())
                    return Json(new { success = false, error = "Datos incompletos: " + string.Join("; ", errores) });

                CargarViewBags(id);
                return View("Formulario", model);
            }

            try
            {
                var apiModel = ToApiModel(model);
                _dal.Actualizar(id, apiModel, (apiModel.contacto ?? "").Trim(), (apiModel.nombre ?? "").Trim());
                TempData["Success"] = "Prospecto actualizado correctamente.";

                if (Request.IsAjaxRequest())
                    return Json(new { success = true, id = id, message = "Prospecto actualizado correctamente." });

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                    return Json(new { success = false, error = "Error al actualizar: " + ex.Message });

                ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
                CargarViewBags(id);
                return View("Formulario", model);
            }
        }

        public ActionResult Detalle(int id)
        {
            ViewBag.Title = "Detalle del Prospecto";
            ViewBag.ActiveMenu = "Prospectos";

            try
            {
                var row = _dal.ObtenerTodos().FirstOrDefault(r => ToInt(Val(r, "id")) == id);
                if (row == null) return HttpNotFound();

                var model = MapDetalle(row);
                model.Contactos = _dal.ObtenerContactos(id).Select(MapContacto).ToList();
                model.Sucursales = _dal.ObtenerSucursales(id).Select(MapSucursal).ToList();
                CargarViewBags(id);
                return View(model);
            }
            catch
            {
                if (_modoDevSinBd)
                {
                    var model = GetSampleProspectos().FirstOrDefault(p => p.Id == id);
                    if (model == null) return HttpNotFound();
                    CargarViewBags(id);
                    return View(model);
                }
                throw;
            }
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            try
            {
                _dal.Eliminar(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult AsignarVendedor(int id, int vendedorId)
        {
            try
            {
                var nombre = _dal.AsignarVendedor(id, vendedorId);
                return Json(new { success = true, nombre });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Rechazar(int id, string motivo)
        {
            try
            {
                var usuarioId = Session["UsuarioId"] as int?;
                _dal.Rechazar(id, motivo, usuarioId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult CambiarEstatus(int id, string estatus)
        {
            try
            {
                _dal.ActualizarEstatus(id, estatus);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Notificacion(int id, string req)
        {
            try
            {
                ApiNotificacionModel model = null;
                try
                {
                    model = JsonConvert.DeserializeObject<ApiNotificacionModel>(req);
                }
                catch (Exception exParse)
                {
                    return Json(new { success = false, error = "Formato de solicitud inválido: " + exParse.Message });
                }

                if (model == null)
                    return Json(new { success = false, error = "No se recibieron datos de notificación." });

                // Si no se envió usuario, tomar el de la sesión actual.
                if (!model.enviado_por.HasValue)
                {
                    model.enviado_por = Session["UsuarioId"] as int?;
                }

                string passwordTemporal = model.password_temporal;
                string cotizacionRef = model.cotizacion_ref;
                string vigenciaInicio = model.vigencia_inicio;
                string vigenciaFin = model.vigencia_fin;

                if (model.tipo_asunto == "Finalizar datos de registro")
                {
                    if (string.IsNullOrEmpty(passwordTemporal))
                    {
                        var rand = new Random();
                        passwordTemporal = rand.Next(10000000, 99999999).ToString() + "ABC!";
                    }
                    // Campos no aplicables para este asunto; enviar cadena vacía al SP para evitar parámetro no suministrado.
                    cotizacionRef = cotizacionRef ?? "";
                    vigenciaInicio = vigenciaInicio ?? "";
                    vigenciaFin = vigenciaFin ?? "";
                }
                else if (model.tipo_asunto == "Reenvío de Cotización")
                {
                    if (string.IsNullOrEmpty(cotizacionRef))
                    {
                        var now = DateTime.Now;
                        cotizacionRef = $"COT-{now:yyyy-MM-dd}";
                        vigenciaInicio = now.ToString("yyyy-MM-dd");
                        vigenciaFin = now.AddDays(7).ToString("yyyy-MM-dd");
                    }
                    // Campo no aplicable para este asunto.
                    passwordTemporal = passwordTemporal ?? "";
                }
                else
                {
                    // Defaults para cualquier otro asunto.
                    passwordTemporal = passwordTemporal ?? "";
                    cotizacionRef = cotizacionRef ?? "";
                    vigenciaInicio = vigenciaInicio ?? "";
                    vigenciaFin = vigenciaFin ?? "";
                }

                _dal.InsertarNotificacion(id, model, passwordTemporal, cotizacionRef, vigenciaInicio, vigenciaFin);
                return Json(new { success = true, passwordTemporal, cotizacionRef, vigenciaInicio, vigenciaFin });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        #region Helpers

        private void BindHijos(ProspectoViewModel model)
        {
            try
            {
                var jsonContactos = Request["jsonContactos"] ?? "[]";
                model.Contactos = JsonConvert.DeserializeObject<List<ProspectoContactoViewModel>>(jsonContactos) ?? new List<ProspectoContactoViewModel>();
            }
            catch { model.Contactos = new List<ProspectoContactoViewModel>(); }

            try
            {
                var jsonSucursales = Request["jsonSucursales"] ?? "[]";
                model.Sucursales = JsonConvert.DeserializeObject<List<ProspectoSucursalViewModel>>(jsonSucursales) ?? new List<ProspectoSucursalViewModel>();
            }
            catch { model.Sucursales = new List<ProspectoSucursalViewModel>(); }
        }

        private void CargarViewBags(int? prospectoId = null)
        {
            ViewBag.TiposPersona = new[] { "Física", "Moral" };
            ViewBag.EstatusLista = new[] { "Nuevo", "En revisión", "En seguimiento", "Cotizado", "Aprobado", "Rechazado", "Adeudo", "Inactivo" };
            ViewBag.SiNo = new[] { "No", "Sí" };
            try
            {
                ViewBag.Vendedores = _usuariosDal.ObtenerActivos().Where(u => u.rol?.ToLower() == "vendedor").ToList();
            }
            catch
            {
                ViewBag.Vendedores = new List<Models.Usuarios.UsuarioDto>();
            }
        }

        private static object Val(dynamic r, string key)
        {
            if (r == null) return null;
            var dict = r as IDictionary<string, object>;
            if (dict == null) return null;
            var k = dict.Keys.FirstOrDefault(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));
            return k != null ? dict[k] : null;
        }

        private static string Base64FromBytes(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            try { return Convert.ToBase64String((byte[])value); }
            catch { return null; }
        }

        private static decimal? ParseDecimal(string value)
        {
            return decimal.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d) ? (decimal?)d : null;
        }

        private ProspectoViewModel MapListItem(dynamic r)
        {
            var dict = r as IDictionary<string, object>;
            return new ProspectoViewModel
            {
                Id = ToInt(Val(r, "id")),
                Nombre = ToString(Val(r, "nombre") ?? Val(r, "contacto"), "Sin Nombre"),
                Rfc = ToString(Val(r, "rfc")),
                Contacto = ToString(Val(r, "contacto")),
                Telefono = ToString(Val(r, "telefono")),
                Email = ToString(Val(r, "email")),
                Estatus = ToString(Val(r, "estatus"), "Nuevo"),
                TipoPersona = ToString(Val(r, "tipoPersona"), "Moral"),
                TieneSucursales = ToString(Val(r, "tieneSucursales"), "No"),
                NombreComercial = ToString(Val(r, "nombreComercial")),
                Calle = ToString(Val(r, "calle")),
                NumExt = ToString(Val(r, "numExt")),
                Colonia = ToString(Val(r, "colonia")),
                Municipio = ToString(Val(r, "municipio")),
                Cp = ToString(Val(r, "cp")),
                Lat = ToString(Val(r, "lat")),
                Lng = ToString(Val(r, "lng")),
                VendedorNombre = ToString(Val(r, "vendedorNombre")),
                FotoFachada = Base64FromBytes(Val(r, "foto_fachada")),
                FotoAcceso = Base64FromBytes(Val(r, "foto_acceso")),
                FotoReferencia = Base64FromBytes(Val(r, "foto_referencia")),
                DocumentoCatastral = Base64FromBytes(Val(r, "documento_catastral")),
                DocumentoCatastralNombre = ToString(Val(r, "documento_catastral_nombre"))
            };
        }

        private ProspectoViewModel MapDetalle(dynamic r)
        {
            var m = MapListItem(r);
            m.NumInt = ToString(Val(r, "numInt"));
            m.Estado = ToString(Val(r, "estado"), "Yucatán");
            m.Notas = ToString(Val(r, "notas"));
            m.Concesionaria = ToString(Val(r, "concesionaria"));
            m.Referencias = ToString(Val(r, "referencias"));
            m.FolioCatastral = ToString(Val(r, "folioCatastral"));
            m.DiasDisponibles = ToString(Val(r, "dias_disponibles"));
            m.Horario = ToString(Val(r, "horario"));
            m.Ruta = ToString(Val(r, "ruta"));
            m.MotivoRechazo = ToString(Val(r, "motivoRechazo"));
            var vId = Val(r, "vendedorId");
            m.VendedorId = vId != null ? (int?)ToInt(vId) : null;
            return m;
        }

        private ProspectoContactoViewModel MapContacto(dynamic c)
        {
            return new ProspectoContactoViewModel
            {
                Id = Val(c, "id") != null ? (int?)ToInt(Val(c, "id")) : null,
                NombreContacto = ToString(Val(c, "nombre_contacto")),
                Correo = ToString(Val(c, "correo")),
                Telefono = ToString(Val(c, "telefono")),
                RepresentanteLegal = ToBool(Val(c, "representante_legal"))
            };
        }

        private ProspectoSucursalViewModel MapSucursal(dynamic s)
        {
            return new ProspectoSucursalViewModel
            {
                Id = Val(s, "id") != null ? (int?)ToInt(Val(s, "id")) : null,
                NombreSucursal = ToString(Val(s, "nombre_sucursal")),
                CorreoElectronico = ToString(Val(s, "correo_electronico")),
                TelefonoSucursal = ToString(Val(s, "telefono_sucursal")),
                NombreResponsable = ToString(Val(s, "nombre_responsable")),
                Calle = ToString(Val(s, "calle")),
                NumExt = ToString(Val(s, "numExt")),
                NumInt = ToString(Val(s, "numInt")),
                Colonia = ToString(Val(s, "colonia")),
                Municipio = ToString(Val(s, "municipio")),
                Cp = ToString(Val(s, "cp")),
                Estado = ToString(Val(s, "estado"), "Yucatán"),
                Lat = ToString(Val(s, "lat")),
                Lng = ToString(Val(s, "lng")),
                Concesionaria = ToString(Val(s, "concesionaria")),
                Referencias = ToString(Val(s, "referencias")),
                FolioCatastral = ToString(Val(s, "folioCatastral")),
                FotoFachada = Base64FromBytes(Val(s, "foto_fachada")),
                FotoAcceso = Base64FromBytes(Val(s, "foto_acceso")),
                FotoReferencia = Base64FromBytes(Val(s, "foto_referencia")),
                DocumentoCatastral = Base64FromBytes(Val(s, "documento_catastral")),
                DocumentoCatastralNombre = ToString(Val(s, "documento_catastral_nombre"))
            };
        }

        private ApiProspectoModel ToApiModel(ProspectoViewModel m)
        {
            var api = new ApiProspectoModel
            {
                id = m.Id > 0 ? m.Id.ToString() : null,
                nombre = m.Nombre,
                rfc = m.Rfc,
                nombreComercial = m.NombreComercial,
                tipoPersona = m.TipoPersona,
                tieneSucursales = m.TieneSucursales,
                contacto = m.Contacto,
                telefono = m.Telefono,
                email = m.Email,
                estatus = m.Estatus,
                notas = m.Notas,
                calle = m.Calle,
                numExt = m.NumExt,
                numInt = m.NumInt,
                colonia = m.Colonia,
                municipio = m.Municipio,
                cp = m.Cp,
                estado = m.Estado,
                lat = ParseDecimal(m.Lat),
                lng = ParseDecimal(m.Lng),
                coordenadas_manuales = m.CoordenadasManuales,
                concesionaria = m.Concesionaria,
                referencias = m.Referencias,
                folioCatastral = m.FolioCatastral,
                dias_disponibles = m.DiasDisponibles,
                horario = m.Horario,
                ruta = m.Ruta,
                foto_fachada = m.FotoFachada,
                foto_acceso = m.FotoAcceso,
                foto_referencia = m.FotoReferencia,
                documento_catastral = m.DocumentoCatastral,
                documento_catastral_nombre = m.DocumentoCatastralNombre,
                contactos = m.Contactos?.Select(c => new ApiContactoModel
                {
                    nombre_contacto = c.NombreContacto,
                    correo = c.Correo,
                    telefono = c.Telefono,
                    representante_legal = c.RepresentanteLegal
                }).ToList() ?? new List<ApiContactoModel>(),
                sucursales = m.Sucursales?.Select(s => new ApiSucursalModel
                {
                    nombre_sucursal = s.NombreSucursal,
                    correo_electronico = s.CorreoElectronico,
                    telefono_sucursal = s.TelefonoSucursal,
                    nombre_responsable = s.NombreResponsable,
                    calle = s.Calle,
                    numExt = s.NumExt,
                    numInt = s.NumInt,
                    colonia = s.Colonia,
                    municipio = s.Municipio,
                    cp = s.Cp,
                    estado = s.Estado,
                    lat = ParseDecimal(s.Lat),
                    lng = ParseDecimal(s.Lng),
                    concesionaria = s.Concesionaria,
                    referencias = s.Referencias,
                    folioCatastral = s.FolioCatastral,
                    foto_fachada = s.FotoFachada,
                    foto_acceso = s.FotoAcceso,
                    foto_referencia = s.FotoReferencia,
                    documento_catastral = s.DocumentoCatastral,
                    documento_catastral_nombre = s.DocumentoCatastralNombre
                }).ToList() ?? new List<ApiSucursalModel>()
            };
            return api;
        }

        private List<ProspectoViewModel> GetSampleProspectos()
        {
            return new List<ProspectoViewModel>
            {
                new ProspectoViewModel
                {
                    Id = 1,
                    Nombre = "Empresa Demo SA de CV",
                    NombreComercial = "Empresa Demo",
                    Rfc = "DEM010101ABC",
                    Contacto = "Juan Pérez",
                    Telefono = "9991234567",
                    Email = "demo@ejemplo.com",
                    Estatus = "En seguimiento",
                    TipoPersona = "Moral",
                    TieneSucursales = "No",
                    Calle = "Av. Paseo de Montejo",
                    NumExt = "100",
                    Colonia = "Centro",
                    Municipio = "Mérida",
                    Cp = "97000",
                    Estado = "Yucatán",
                    Lat = "20.9674",
                    Lng = "-89.6237"
                },
                new ProspectoViewModel
                {
                    Id = 2,
                    Nombre = "Constructora Yucatán",
                    NombreComercial = "ConstYuc",
                    Rfc = "CYU020202DEF",
                    Contacto = "Ana Ruiz",
                    Telefono = "9999876543",
                    Email = "contacto@constyuc.com",
                    Estatus = "Cotizado",
                    TipoPersona = "Moral",
                    TieneSucursales = "Sí",
                    Calle = "Calle 60",
                    NumExt = "250",
                    Colonia = "Centro",
                    Municipio = "Mérida",
                    Cp = "97000",
                    Estado = "Yucatán"
                }
            };
        }

        #endregion
    }
}
