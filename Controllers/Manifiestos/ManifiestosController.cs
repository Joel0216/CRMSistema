using System.Web.Mvc;

namespace CRMSistema.Controllers.Manifiestos
{
    [Authorize]
    public class ManifiestosController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Manifiestos";
            ViewBag.ActiveMenu = "Manifiestos";
            return View();
        }
    }
}
