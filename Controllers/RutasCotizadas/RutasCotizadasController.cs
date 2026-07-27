using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CRMSistema.Controllers.Base;
using CRMSistema.Filters;
using CRMSistema.Models.Usuarios;

namespace CRMSistema.Controllers.RutasCotizadas
{
    /// <summary>
    /// Módulo de Rutas Cotizadas (maqueta operativa).
    /// Centraliza cotizaciones aprobadas que requieren asignación de ruta, unidad y operador.
    /// Accesible para Superadmin y Supervisor.
    /// </summary>
    [AuthorizeRole(AppRoles.Supervisor, AppRoles.Superadmin)]
    public class RutasCotizadasController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Rutas Cotizadas";
            ViewBag.ActiveMenu = "RutasCotizadas";
            return View();
        }

        [HttpGet]
        public ActionResult GetPendientes()
        {
            try
            {
                var data = GenerarDatosDemo();
                return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetDetalle(int id)
        {
            try
            {
                var data = GenerarDatosDemo();
                var item = data.FirstOrDefault(x => x.id == id);
                if (item == null)
                    return Json(new { success = false, error = "Cotización no encontrada." }, JsonRequestBehavior.AllowGet);

                var rutas = GenerarDisponibilidadRutas();
                var ocupacion = GenerarOcupacionPorDia();
                var sugerencia = GenerarSugerenciaOperativa();
                var historial = GenerarHistorial(id);

                return Json(new
                {
                    success = true,
                    item,
                    rutas,
                    ocupacion,
                    sugerencia,
                    historial
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult AsignarRuta(int id, string ruta, string unidad, string operador, string observaciones)
        {
            try
            {
                // TODO: persistir asignación en tabla crm_rutas_cotizadas cuando exista.
                return Json(new { success = true, message = "Ruta asignada correctamente." });
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
                // TODO: cambiar estatus a "Requiere ajuste comercial" y notificar a ventas.
                return Json(new { success = true, message = "Solicitud rechazada. Se notificará a ventas." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        #region Datos de demostración

        private List<dynamic> GenerarDatosDemo()
        {
            return new List<dynamic>
            {
                new {
                    id = 1,
                    folio = "COT-2026-001",
                    cliente = "GRUPO BACHOCO S.A. DE C.V.",
                    rfc = "BAC000220B25",
                    domicilio = "Av. Itzaes 1234, Mérida, Yucatán",
                    latitud = 20.970034,
                    longitud = -89.619972,
                    diasDisponibles = new[] { "Lun", "Mie", "Vie" },
                    servicios = new[] { new { tipo = "RSU", nombre = "Recolector RSU", frecuencia = "Semanal", cantidad = 7 } },
                    diasAsignados = new string[] { },
                    operador = "José Hernández",
                    estatus = "Pendiente",
                    fechaCotizacion = "24/05/2026",
                    creador = "Juan Pérez",
                    observaciones = "Recolección actual los lunes, miércoles y viernes."
                },
                new {
                    id = 2,
                    folio = "COT-2026-002",
                    cliente = "INSTITUTO TECNOLÓGICO DE MÉRIDA",
                    rfc = "ITM987654321",
                    domicilio = "Calle 10 #10, Caucel, Mérida",
                    latitud = 20.9945,
                    longitud = -89.6812,
                    diasDisponibles = new[] { "Lun" },
                    servicios = new[] { new { tipo = "RME", nombre = "Lodos", frecuencia = "Mensual", cantidad = 1 } },
                    diasAsignados = new string[] { },
                    operador = "Jorge Pech",
                    estatus = "Pendiente",
                    fechaCotizacion = "22/05/2026",
                    creador = "Ana López",
                    observaciones = "Acceso restringido, coordinar con vigilancia."
                },
                new {
                    id = 3,
                    folio = "COT-2026-003",
                    cliente = "GRUPO WALMART",
                    rfc = "WAL123456789",
                    domicilio = "Av. Itzaes #947, García Ginerés, Mérida",
                    latitud = 20.9867,
                    longitud = -89.6234,
                    diasDisponibles = new[] { "Mar", "Mie" },
                    servicios = new[] { new { tipo = "RSU", nombre = "Recolector RSU", frecuencia = "Semanal", cantidad = 14 } },
                    diasAsignados = new[] { "Mar" },
                    operador = "Esteban Huitz",
                    estatus = "Asignado",
                    fechaCotizacion = "20/05/2026",
                    creador = "Carlos Ruiz",
                    observaciones = "Alto volumen, preferencia martes por la mañana."
                },
                new {
                    id = 4,
                    folio = "COT-2026-004",
                    cliente = "GRUPO CRÍO",
                    rfc = "CRI9876543210",
                    domicilio = "Calle 54 #120, Mérida",
                    latitud = 20.9654,
                    longitud = -89.6123,
                    diasDisponibles = new[] { "Lun", "Mie", "Vie" },
                    servicios = new[] { new { tipo = "RSU", nombre = "Recolector RSU", frecuencia = "Semanal", cantidad = 5 } },
                    diasAsignados = new string[] { },
                    operador = "Pedro Moreno",
                    estatus = "Pendiente",
                    fechaCotizacion = "18/05/2026",
                    creador = "Juan Pérez",
                    observaciones = "Horario de recolección después de las 9:00 hrs."
                },
                new {
                    id = 5,
                    folio = "COT-2026-005",
                    cliente = "HOSPITAL JUÁREZ",
                    rfc = "HOS1234567890",
                    domicilio = "Calle Roma #98, Los Héroes, Mérida",
                    latitud = 20.9512,
                    longitud = -89.6456,
                    diasDisponibles = new[] { "Diario" },
                    servicios = new[] { new { tipo = "RME", nombre = "Residuos Biológicos", frecuencia = "Diaria", cantidad = 2 } },
                    diasAsignados = new string[] { },
                    operador = "Ricardo Garrido",
                    estatus = "Pendiente",
                    fechaCotizacion = "15/05/2026",
                    creador = "Diana Torres",
                    observaciones = "Servicio de manejo especial, requiere permisos."
                }
            };
        }

        private object GenerarSugerenciaOperativa()
        {
            return new
            {
                ruta = "Ruta Norte",
                unidad = "U-03 | RSU Norte",
                operador = "Operador 1",
                probabilidadCumplimiento = 94,
                capacidadRestante = 10,
                distanciaAdicional = 1.8
            };
        }

        private List<dynamic> GenerarDisponibilidadRutas()
        {
            return new List<dynamic>
            {
                new { nombre = "Ruta Norte", ocupacion = 88, distancia = 1.0, recomendacion = "No recomendado" },
                new { nombre = "Ruta Centro", ocupacion = 62, distancia = 4.2, recomendacion = "Recomendada" },
                new { nombre = "Ruta Sur", ocupacion = 71, distancia = 6.8, recomendacion = "Posible" }
            };
        }

        private List<dynamic> GenerarOcupacionPorDia()
        {
            return new List<dynamic>
            {
                new { dia = "Lun", actual = 80, despues = 90 },
                new { dia = "Mar", actual = 92, despues = 98 },
                new { dia = "Mie", actual = 88, despues = 94 },
                new { dia = "Jue", actual = 70, despues = 70 },
                new { dia = "Vie", actual = 95, despues = 95 },
                new { dia = "Sab", actual = 65, despues = 65 },
                new { dia = "Dom", actual = 40, despues = 40 }
            };
        }

        private List<dynamic> GenerarHistorial(int id)
        {
            return new List<dynamic>
            {
                new { fecha = "24/05/2026 09:15", usuario = "Juan Pérez (Ventas)", accion = "Cotización creada" },
                new { fecha = "24/05/2026 10:22", usuario = "Sistema", accion = "Pendiente de validación operativa" }
            };
        }

        #endregion
    }
}
