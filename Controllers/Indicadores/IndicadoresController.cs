using System.Web.Mvc;
using CRMSistema.Controllers.Base;
using CRMSistema.Filters;
using CRMSistema.Models.Usuarios;

namespace CRMSistema.Controllers.Indicadores
{
    /// <summary>
    /// Módulo de indicadores y reportes gerenciales.
    /// Accesible para Jefe y Superadmin.
    /// </summary>
    [AuthorizeRole(AppRoles.Jefe, AppRoles.Superadmin)]
    public class IndicadoresController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Indicadores";
            ViewBag.ActiveMenu = "Indicadores";
            return View();
        }
    }
}
