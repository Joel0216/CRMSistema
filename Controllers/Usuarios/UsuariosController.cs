using System.Web.Mvc;

namespace CRMSistema.Controllers.Usuarios
{
    [Authorize]
    public class UsuariosController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Usuarios";
            return View();
        }
    }
}
