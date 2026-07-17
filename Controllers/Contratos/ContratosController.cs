using System.Web.Mvc;

namespace CRMSistema.Controllers.Contratos
{
    [Authorize]
    public class ContratosController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Contratos";
            ViewBag.ActiveMenu = "Contratos";
            return View();
        }
    }
}
