using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CRMSistema
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

#if DEBUG
            // En desarrollo desactivamos minificación para evitar errores de WebGrease
            // con archivos ES6 modernos (Bootstrap 5) y facilitar depuración.
            BundleTable.EnableOptimizations = false;
#endif

            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
