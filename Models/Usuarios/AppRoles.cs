using System;

namespace CRMSistema.Models.Usuarios
{
    /// <summary>
    /// Nombres oficiales de roles del sistema. Usar estas constantes en lugar de
    /// strings sueltos para evitar inconsistencias entre BD, controllers, filtros y vistas.
    /// </summary>
    public static class AppRoles
    {
        public const string Vendedor = "Vendedor";
        public const string Supervisor = "Supervisor";
        public const string Superadmin = "Superadmin";

        /// <summary>
        /// Rol que en versiones anteriores se llamaba "Administrador".
        /// Se mantiene como alias de Supervisor para compatibilidad con datos antiguos.
        /// </summary>
        public const string Administrador = "Administrador";

        public static bool EsSuperadmin(string rol)
        {
            return string.Equals(rol, Superadmin, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsSupervisorOAdmin(string rol)
        {
            return EsSuperadmin(rol)
                || string.Equals(rol, Supervisor, StringComparison.OrdinalIgnoreCase)
                || string.Equals(rol, Administrador, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsVendedor(string rol)
        {
            return EsSupervisorOAdmin(rol)
                || string.Equals(rol, Vendedor, StringComparison.OrdinalIgnoreCase);
        }

        public static bool TieneRol(string rolActual, string rolRequerido)
        {
            return string.Equals(rolActual, rolRequerido, StringComparison.OrdinalIgnoreCase);
        }
    }
}
