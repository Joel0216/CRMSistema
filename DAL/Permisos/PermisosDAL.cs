using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CRMSistema.Models.Permisos;

namespace CRMSistema.DAL.Permisos
{
    /// <summary>
    /// Acceso a datos para el menú dinámico y permisos por rol.
    /// </summary>
    public class PermisosDAL
    {
        /// <summary>
        /// Obtiene todos los menús y submenús activos del sistema.
        /// </summary>
        public List<MenuPermisoModel> ObtenerTodosLosMenus()
        {
            var rows = AdoHelper.Query("SP_Menu_GetAll", CommandType.StoredProcedure);
            return AgruparMenus(rows);
        }

        /// <summary>
        /// Obtiene los menús y submenús permitidos para un rol específico.
        /// </summary>
        public List<MenuPermisoModel> ObtenerPermisosPorRol(int rolId)
        {
            var rows = AdoHelper.Query("SP_Permisos_GetByRol", CommandType.StoredProcedure,
                new SqlParameter("@rolId", rolId));
            return AgruparMenus(rows);
        }

        /// <summary>
        /// Guarda o actualiza un permiso para un rol y submenú.
        /// </summary>
        public void GuardarPermiso(int rolId, int submenuId, bool activo)
        {
            AdoHelper.Execute("SP_Permisos_Save", CommandType.StoredProcedure,
                new SqlParameter("@rolId", rolId),
                new SqlParameter("@submenuId", submenuId),
                new SqlParameter("@activo", activo));
        }

        /// <summary>
        /// Agrupa filas planas de SP en una lista de menús con sus submenús.
        /// </summary>
        private static List<MenuPermisoModel> AgruparMenus(List<dynamic> rows)
        {
            var menus = new List<MenuPermisoModel>();

            foreach (var r in rows)
            {
                var dict = r as IDictionary<string, object>;
                if (dict == null) continue;

                int menuId = GetInt(dict, "menuId");
                string menuNombre = GetString(dict, "menuNombre");
                string menuIcono = GetString(dict, "menuIcono");
                int menuOrden = GetInt(dict, "menuOrden");

                var menu = menus.FirstOrDefault(m => m.Id == menuId);
                if (menu == null)
                {
                    menu = new MenuPermisoModel
                    {
                        Id = menuId,
                        Nombre = menuNombre,
                        Icono = menuIcono,
                        Orden = menuOrden
                    };
                    menus.Add(menu);
                }

                int subMenuId = GetInt(dict, "submenuId");
                if (subMenuId <= 0) continue;

                if (!menu.SubMenus.Any(sm => sm.Id == subMenuId))
                {
                    menu.SubMenus.Add(new SubMenuPermisoModel
                    {
                        Id = subMenuId,
                        MenuId = menuId,
                        Nombre = GetString(dict, "submenuNombre"),
                        Controlador = GetString(dict, "submenuControlador"),
                        Accion = GetString(dict, "submenuAccion", "Index"),
                        Icono = GetString(dict, "submenuIcono"),
                        Orden = GetInt(dict, "submenuOrden")
                    });
                }
            }

            // Asegurar ordenamiento
            foreach (var menu in menus)
            {
                menu.SubMenus = menu.SubMenus.OrderBy(sm => sm.Orden).ThenBy(sm => sm.Nombre).ToList();
            }

            return menus.OrderBy(m => m.Orden).ThenBy(m => m.Nombre).ToList();
        }

        private static string GetString(IDictionary<string, object> dict, string key, string defaultValue = "")
        {
            if (dict.ContainsKey(key) && dict[key] != null)
                return dict[key].ToString();
            return defaultValue;
        }

        private static int GetInt(IDictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
                return Convert.ToInt32(dict[key]);
            return 0;
        }
    }
}
