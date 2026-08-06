using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using CRMSistema.DAL.Manifiestos;
using CRMSistema.Filters;
using CRMSistema.Models.Usuarios;
using CRMSistema.Models.Manifiestos;

namespace CRMSistema.Controllers.Manifiestos
{
    /// <summary>
    /// Módulo de Manifiestos de recolección, transporte y entrega.
    /// Ahora consulta y guarda en la tabla crm_manifiestos mediante los SPs sugeridos.
    /// Si los SPs o la tabla aún no existen, cae a datos de demostración para que la
    /// interfaz siga funcionando mientras Operaciones crea los objetos en BD.
    /// </summary>
    [AuthorizeRole(AppRoles.Supervisor, AppRoles.Coordinador, AppRoles.Jefe, AppRoles.Superadmin)]
    public class ManifiestosController : Controller
    {
        private readonly ManifiestosDAL _dal = new ManifiestosDAL();
        private readonly bool _modoDevSinBd;

        public ManifiestosController()
        {
            bool.TryParse(ConfigurationManager.AppSettings["ModoDesarrolloSinBD"], out _modoDevSinBd);
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Manifiestos";
            ViewBag.ActiveMenu = "Manifiestos";
            return View();
        }

        [HttpGet]
        public ActionResult GetManifiestos()
        {
            try
            {
                if (_modoDevSinBd)
                {
                    var demo = ObtenerDemoSesion();
                    return Json(new { success = true, data = demo }, JsonRequestBehavior.AllowGet);
                }
                var data = _dal.ObtenerTodos();
                return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Mientras no exista la tabla/SPs reales, mostrar datos demo para que la interfaz siga activa.
                return Json(new { success = true, data = GenerarDatosDemo(), warning = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Guardar(ManifiestoFormModel manifiesto)
        {
            try
            {
                if (manifiesto == null)
                    return Json(new { success = false, error = "No se recibieron datos." });

                if (string.IsNullOrWhiteSpace(manifiesto.Folio) ||
                    string.IsNullOrWhiteSpace(manifiesto.Generador) ||
                    string.IsNullOrWhiteSpace(manifiesto.Transportista) ||
                    string.IsNullOrWhiteSpace(manifiesto.Destino))
                    return Json(new { success = false, error = "Folio, generador, transportista y destino son obligatorios." });

                if (!manifiesto.Fecha.HasValue)
                    return Json(new { success = false, error = "La fecha es obligatoria." });

                if (!manifiesto.Volumen.HasValue || manifiesto.Volumen.Value < 0)
                    return Json(new { success = false, error = "El volumen debe ser mayor o igual a 0." });

                // Si estamos en modo sin BD, guardar en sesión para que el listado lo refleje inmediatamente.
                if (_modoDevSinBd)
                {
                    var demo = ObtenerDemoSesion();
                    int maxId = 0;
                    foreach (var d in demo)
                    {
                        int curr = (int)d.id;
                        if (curr > maxId) maxId = curr;
                    }
                    var nuevo = new
                    {
                        id = maxId + 1,
                        folio = manifiesto.Folio,
                        fecha = manifiesto.Fecha.Value.ToString("dd/MM/yyyy"),
                        generador = manifiesto.Generador,
                        transportista = manifiesto.Transportista,
                        destino = manifiesto.Destino,
                        volumen = manifiesto.Volumen.Value,
                        estatus = manifiesto.Estatus
                    };
                    demo.Add(nuevo);
                    Session["ManifiestosDemo"] = demo;
                    return Json(new { success = true, id = nuevo.id, message = "Manifiesto guardado correctamente." });
                }

                var creadoPor = Session["UsuarioId"] as int?;
                var id = _dal.Guardar(manifiesto, creadoPor);
                return Json(new { success = true, id = id, message = "Manifiesto guardado correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            try
            {
                _dal.Eliminar(id);
                return Json(new { success = true, message = "Manifiesto eliminado." });
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
                _dal.CambiarEstatus(id, estatus);
                return Json(new { success = true, message = "Estatus actualizado." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        #region Datos de demostración (fallback mientras no haya tabla real)

        private List<dynamic> ObtenerDemoSesion()
        {
            var demo = Session["ManifiestosDemo"] as List<dynamic>;
            if (demo == null)
            {
                demo = GenerarDatosDemo();
                Session["ManifiestosDemo"] = demo;
            }
            return demo;
        }

        private List<dynamic> GenerarDatosDemo()
        {
            return new List<dynamic>
            {
                new { id = 1, folio = "MAN-2026-001", fecha = "05/08/2026", generador = "Grupo Bachoco S.A. de C.V.", transportista = "Transportes Seguros S.A.", destino = "Relleno Sanitario Norte", volumen = 12.5m, estatus = "Entregado" },
                new { id = 2, folio = "MAN-2026-002", fecha = "04/08/2026", generador = "Hospital Juárez", transportista = "BioTransportes", destino = "Planta de tratamiento RME", volumen = 2.3m, estatus = "En tránsito" },
                new { id = 3, folio = "MAN-2026-003", fecha = "03/08/2026", generador = "Instituto Tecnológico de Mérida", transportista = "Transportes Seguros S.A.", destino = "Relleno Sanitario Centro", volumen = 8.0m, estatus = "Entregado" },
                new { id = 4, folio = "MAN-2026-004", fecha = "02/08/2026", generador = "Grupo Walmart", transportista = "Transportes del Sureste", destino = "Relleno Sanitario Sur", volumen = 15.2m, estatus = "Cancelado" }
            };
        }

        #endregion
    }
}
