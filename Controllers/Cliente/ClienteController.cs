using System.Web.Mvc;

namespace CRMSistema.Controllers.Cliente
{
    [Authorize]
    public class ClienteController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Inicio";
            ViewBag.ActiveMenu = "Inicio";
            return View();
        }

        public ActionResult Cotizaciones()
        {
            ViewBag.Title = "Mis Cotizaciones";
            ViewBag.ActiveMenu = "Cotizaciones";
            return View();
        }

        public ActionResult Contratos()
        {
            ViewBag.Title = "Mis Contratos";
            ViewBag.ActiveMenu = "Contratos";
            return View();
        }

        public ActionResult Manifiestos()
        {
            ViewBag.Title = "Mis Manifiestos";
            ViewBag.ActiveMenu = "Manifiestos";
            return View();
        }
    }
}
