using System.Web;

namespace CRMSistema.Models.Usuarios
{
    /// <summary>
    /// Helpers centralizados para validar roles desde controllers o vistas.
    /// </summary>
    public static class RolHelper
    {
        public static string RolActual()
        {
            var ctx = HttpContext.Current;
            if (ctx?.Session == null) return "";
            return ctx.Session["Rol"]?.ToString() ?? "";
        }

        public static bool EsSuperadmin()
        {
            return AppRoles.EsSuperadmin(RolActual());
        }

        public static bool EsSupervisor()
        {
            return AppRoles.EsSupervisorOAdmin(RolActual());
        }

        public static bool EsVendedor()
        {
            return AppRoles.EsVendedor(RolActual());
        }

        public static int? UsuarioIdActual()
        {
            var ctx = HttpContext.Current;
            if (ctx?.Session == null) return null;
            return ctx.Session["UsuarioId"] as int?;
        }
    }
}
