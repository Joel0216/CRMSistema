using System;
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
    }
}
