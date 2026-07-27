using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using CRMSistema.Models.Usuarios;

namespace CRMSistema.Filters
{
    /// <summary>
    /// Filtro de autorización por roles que lee el rol desde Session["Rol"].
    /// FormsAuthentication por defecto no guarda roles en la cookie, por lo que
    /// el filtro nativo AuthorizeAttribute(Roles=...) falla.
    /// </summary>
    public class AuthorizeRoleAttribute : AuthorizeAttribute
    {
        private readonly string[] _roles;

        public AuthorizeRoleAttribute(params string[] roles)
        {
            _roles = roles ?? new string[0];
        }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (httpContext == null) return false;
            if (!httpContext.User.Identity.IsAuthenticated) return false;

            var rolEnSesion = httpContext.Session?["Rol"]?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(rolEnSesion)) return false;

            if (_roles.Length == 0) return true;

            return _roles.Any(r => AppRoles.TieneRol(rolEnSesion, r));
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary
                {
                    { "controller", "Acceso" },
                    { "action", "Login" }
                });
        }
    }
}
