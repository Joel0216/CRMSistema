using System;
using System.Web;
using System.Web.Helpers;
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

            // El token anti-forgery no debe estar ligado al nombre de usuario; de lo contrario
            // cambiar de sesión (login con otro usuario / expiración de forms auth) genera
            // HttpAntiForgeryException por mismatch de identidad. Aún protege contra CSRF.
            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;

#if DEBUG
            // En desarrollo desactivamos minificación para evitar errores de WebGrease
            // con archivos ES6 modernos (Bootstrap 5) y facilitar depuración.
            BundleTable.EnableOptimizations = false;
#endif

            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            var httpEx = ex as HttpAntiForgeryException
                ?? ex?.GetBaseException() as HttpAntiForgeryException;

            if (httpEx != null)
            {
                Server.ClearError();
                Response.Clear();
                Response.TrySkipIisCustomErrors = true;
                Response.StatusCode = 403;

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    Response.ContentType = "application/json";
                    Response.Write("{\"success\":false,\"error\":\"Su sesión cambió o el token de seguridad expiró. Vuelva a iniciar sesión.\"}");
                }
                else
                {
                    Response.Redirect("~/Acceso/Login?error=session_expired");
                }

                Response.End();
            }
        }
    }
}
