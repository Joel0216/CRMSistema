using System.Web.Mvc;

namespace CRMSistema.Controllers.Home
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }

        public ActionResult About()
        {
            ViewBag.Title = "Acerca de";
            ViewBag.ActiveMenu = "About";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Title = "Contacto";
            ViewBag.ActiveMenu = "Contact";
            return View();
        }
    }
}
