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
        public const string Coordinador = "Coordinador";
        public const string Jefe = "Jefe";
        public const string Superadmin = "Superadmin";

        /// <summary>
        /// Rol que en versiones anteriores se llamaba "Administrador".
        /// Se mantiene como alias de Supervisor para compatibilidad con datos antiguos.
        /// </summary>
        public const string Administrador = "Administrador";

        /// <summary>
        /// Determina si el rol es Superadmin (desarrolladores / acceso total).
        /// </summary>
        public static bool EsSuperadmin(string rol)
        {
            return string.Equals(rol, Superadmin, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determina si el rol es Jefe (gerente de área). Incluye a Superadmin.
        /// </summary>
        public static bool EsJefe(string rol)
        {
            return EsSuperadmin(rol)
                || string.Equals(rol, Jefe, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determina si el rol es Coordinador. Incluye a Jefe y Superadmin.
        /// </summary>
        public static bool EsCoordinador(string rol)
        {
            return EsJefe(rol)
                || string.Equals(rol, Coordinador, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determina si el rol tiene privilegios de Supervisor o superior
        /// (Supervisor, Coordinador, Jefe, Superadmin o el alias Administrador).
        /// </summary>
        public static bool EsSupervisorOAdmin(string rol)
        {
            return EsCoordinador(rol)
                || string.Equals(rol, Supervisor, StringComparison.OrdinalIgnoreCase)
                || string.Equals(rol, Administrador, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determina si el rol puede acceder a las funciones base de ventas.
        /// Incluye todos los roles jerárquicamente superiores a Vendedor.
        /// </summary>
        public static bool EsVendedor(string rol)
        {
            return EsSupervisorOAdmin(rol)
                || string.Equals(rol, Vendedor, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determina si el rol puede gestionar usuarios (crear, ver).
        /// Jefe puede gestionar usuarios de su área; Superadmin tiene acceso total.
        /// </summary>
        public static bool EsGestionUsuarios(string rol)
        {
            return EsJefe(rol);
        }

        /// <summary>
        /// Comparación exacta de rol (ignora mayúsculas/minúsculas).
        /// </summary>
        public static bool TieneRol(string rolActual, string rolRequerido)
        {
            return string.Equals(rolActual, rolRequerido, StringComparison.OrdinalIgnoreCase);
        }
    }
}
