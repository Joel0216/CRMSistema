using System.Collections.Generic;
using System.Linq;
using System.Web;
using CRMSistema.DAL.Permisos;
using CRMSistema.Models.Usuarios;

namespace CRMSistema.Models.Permisos
{
    /// <summary>
    /// Servicio para cargar y consultar permisos de menú en sesión.
    /// </summary>
    public static class PermisoService
    {
        public const string SessionKey = "Permisos";

        /// <summary>
        /// Carga los permisos del rol del usuario actual en sesión.
        /// Devuelve true si se cargaron desde BD; false si usó fallback.
        /// </summary>
        public static bool CargarPermisosEnSesion(int? rolId, bool modoDevSinBd)
        {
            var ctx = HttpContext.Current;
            if (ctx?.Session == null) return false;

            List<MenuPermisoModel> permisos;

            if (!rolId.HasValue || modoDevSinBd)
            {
                permisos = ObtenerPermisosHardcodeados(ctx.Session["Rol"]?.ToString() ?? "");
                ctx.Session[SessionKey] = permisos;
                return false;
            }

            try
            {
                var dal = new PermisosDAL();
                permisos = dal.ObtenerPermisosPorRol(rolId.Value);

                // Si no hay permisos en BD, usar fallback para no dejar al usuario sin menú
                if (permisos == null || permisos.Count == 0 || permisos.All(m => m.SubMenus.Count == 0))
                {
                    permisos = ObtenerPermisosHardcodeados(ctx.Session["Rol"]?.ToString() ?? "");
                    ctx.Session[SessionKey] = permisos;
                    return false;
                }

                ctx.Session[SessionKey] = permisos;
                return true;
            }
            catch
            {
                permisos = ObtenerPermisosHardcodeados(ctx.Session["Rol"]?.ToString() ?? "");
                ctx.Session[SessionKey] = permisos;
                return false;
            }
        }

        /// <summary>
        /// Obtiene los permisos almacenados en sesión.
        /// </summary>
        public static List<MenuPermisoModel> ObtenerPermisosDesdeSesion()
        {
            var ctx = HttpContext.Current;
            if (ctx?.Session == null) return new List<MenuPermisoModel>();
            return ctx.Session[SessionKey] as List<MenuPermisoModel> ?? new List<MenuPermisoModel>();
        }

        /// <summary>
        /// Determina si el usuario actual tiene acceso a un controlador/acción específicos.
        /// </summary>
        public static bool TieneAcceso(string controlador, string accion = "Index")
        {
            var permisos = ObtenerPermisosDesdeSesion();
            if (permisos == null || permisos.Count == 0) return false;

            return permisos
                .SelectMany(m => m.SubMenus)
                .Any(sm => sm.Controlador.Equals(controlador, System.StringComparison.OrdinalIgnoreCase)
                       && sm.Accion.Equals(accion, System.StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Fallback de permisos cuando no hay BD o no hay registros.
        /// Mantiene la misma lógica que el layout hardcodeado.
        /// </summary>
        private static List<MenuPermisoModel> ObtenerPermisosHardcodeados(string rol)
        {
            var menus = new List<MenuPermisoModel>
            {
                new MenuPermisoModel { Id = 1, Nombre = "PRINCIPAL", Orden = 1, SubMenus = new List<SubMenuPermisoModel> {
                    new SubMenuPermisoModel { Id = 1, MenuId = 1, Nombre = "Dashboard", Controlador = "Dashboard", Accion = "Index", Orden = 1 }
                }},
                new MenuPermisoModel { Id = 2, Nombre = "VENTAS", Orden = 2, SubMenus = new List<SubMenuPermisoModel> {
                    new SubMenuPermisoModel { Id = 2, MenuId = 2, Nombre = "Prospectos", Controlador = "Prospectos", Accion = "Index", Orden = 1 },
                    new SubMenuPermisoModel { Id = 3, MenuId = 2, Nombre = "Cotizador", Controlador = "Cotizador", Accion = "Index", Orden = 2 },
                    new SubMenuPermisoModel { Id = 4, MenuId = 2, Nombre = "Cotizaciones por Aprobar", Controlador = "ValidacionCotizaciones", Accion = "Index", Orden = 3 },
                    new SubMenuPermisoModel { Id = 5, MenuId = 2, Nombre = "Contratos", Controlador = "Contratos", Accion = "Index", Orden = 4 },
                    new SubMenuPermisoModel { Id = 6, MenuId = 2, Nombre = "Contratos por Autorizar", Controlador = "ContratosPorAutorizar", Accion = "Index", Orden = 5 },
                    new SubMenuPermisoModel { Id = 7, MenuId = 2, Nombre = "Contratos Autorizados", Controlador = "ContratosAutorizados", Accion = "Index", Orden = 6 }
                }},
                // OPERACIONES: módulos para Coordinador+, Supervisor+ y Superadmin.
                // - Rutas Cotizadas  ->  Controllers/RutasCotizadas/RutasCotizadasController.cs
                // - Manifiestos      ->  Controllers/Manifiestos/ManifiestosController.cs
                new MenuPermisoModel { Id = 3, Nombre = "OPERACIONES", Orden = 3, SubMenus = new List<SubMenuPermisoModel> {
                    new SubMenuPermisoModel { Id = 8, MenuId = 3, Nombre = "Rutas Cotizadas", Controlador = "RutasCotizadas", Accion = "Index", Orden = 1 },
                    new SubMenuPermisoModel { Id = 9, MenuId = 3, Nombre = "Manifiestos", Controlador = "Manifiestos", Accion = "Index", Orden = 2 }
                }},
                new MenuPermisoModel { Id = 4, Nombre = "ADMINISTRACIÓN", Orden = 4, SubMenus = new List<SubMenuPermisoModel> {
                    new SubMenuPermisoModel { Id = 10, MenuId = 4, Nombre = "Usuarios", Controlador = "Usuarios", Accion = "Index", Icono = "fa-users-cog", Orden = 1 }
                }}
            };

            bool esSuperadmin = AppRoles.EsSuperadmin(rol);
            bool esJefe = AppRoles.EsJefe(rol);
            bool esCoordinador = AppRoles.EsCoordinador(rol);
            bool esSupervisor = AppRoles.EsSupervisorOAdmin(rol);

            foreach (var menu in menus)
            {
                menu.SubMenus = menu.SubMenus.Where(sm =>
                {
                    // Vendedor: básico
                    if (sm.Nombre == "Dashboard" ||
                        sm.Nombre == "Prospectos" ||
                        sm.Nombre == "Cotizador" ||
                        sm.Nombre == "Contratos" ||
                        sm.Nombre == "Contratos Autorizados")
                        return AppRoles.EsVendedor(rol);

                    // Supervisor+: aprobaciones
                    if (sm.Nombre == "Cotizaciones por Aprobar" ||
                        sm.Nombre == "Contratos por Autorizar")
                        return esSupervisor;

                    // Coordinador+: operaciones
                    if (sm.Nombre == "Rutas Cotizadas" ||
                        sm.Nombre == "Manifiestos")
                        return esCoordinador;

                    // Jefe+: usuarios
                    if (sm.Nombre == "Usuarios")
                        return esJefe;

                    return esSuperadmin;
                }).ToList();
            }

            return menus.Where(m => m.SubMenus.Any()).ToList();
        }
    }
}
