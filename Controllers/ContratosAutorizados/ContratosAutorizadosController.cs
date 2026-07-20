using System;
using System.Web.Mvc;
using CRMSistema.DAL.Contratos;

namespace CRMSistema.Controllers.ContratosAutorizados
{
    [Authorize]
    public class ContratosAutorizadosController : Controller
    {
        private readonly ContratosDAL _dal = new ContratosDAL();

        public ActionResult Index()
        {
            ViewBag.Title = "Contratos Autorizados";
            ViewBag.ActiveMenu = "ContratosAutorizados";
            return View();
        }

        [HttpGet]
        public ActionResult GetContratos()
        {
            try
            {
                var data = _dal.ObtenerContratosAutorizados();
                return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
