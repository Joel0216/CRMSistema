using System.Web.Mvc;
using CRMSistema.Filters;
using CRMSistema.Models.Usuarios;

namespace CRMSistema.Controllers.Manifiestos
{
    [AuthorizeRole(AppRoles.Supervisor, AppRoles.Superadmin)]
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
