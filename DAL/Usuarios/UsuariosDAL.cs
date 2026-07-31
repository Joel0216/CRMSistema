using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using CRMSistema.Models.Usuarios;

namespace CRMSistema.DAL.Usuarios
{
    /// <summary>
    /// Acceso a datos para usuarios del sistema. Toda la lógica SQL vive en SQL Server (stored procedures).
    /// </summary>
    public class UsuariosDAL
    {
        #region Lectura

        public List<UsuarioDto> ObtenerActivos()
        {
            var rows = AdoHelper.Query("SP_Usuarios_GetActivos", CommandType.StoredProcedure);
            return rows.Select(MapUsuario).ToList();
        }

        public List<UsuarioDto> ObtenerTodos()
        {
            var rows = AdoHelper.Query("SP_Usuarios_GetAll", CommandType.StoredProcedure);
            return rows.Select(MapUsuario).ToList();
        }

        public UsuarioDto ObtenerPorId(int id)
        {
            var rows = AdoHelper.Query("SP_Usuarios_GetById", CommandType.StoredProcedure,
                new SqlParameter("@id", id));

            var row = rows.FirstOrDefault();
            return row != null ? MapUsuario(row) : null;
        }

        public UsuarioDto ValidarUsuario(string usuario, string password)
        {
            var rows = AdoHelper.Query("SP_Usuarios_ValidarLogin", CommandType.StoredProcedure,
                new SqlParameter("@usuario", SqlDbType.VarChar, 50) { Value = (object)usuario ?? DBNull.Value },
                new SqlParameter("@password", SqlDbType.VarChar, 100) { Value = (object)password ?? DBNull.Value });

            var row = rows.FirstOrDefault();
            return row != null ? MapUsuario(row) : null;
        }

        public bool ValidarPasswordActual(int usuarioId, string password)
        {
            var rows = AdoHelper.Query("SP_Usuarios_ValidarPassword", CommandType.StoredProcedure,
                new SqlParameter("@usuarioId", usuarioId),
                new SqlParameter("@password", SqlDbType.VarChar, 100) { Value = (object)password ?? DBNull.Value });

            var total = rows.FirstOrDefault()?.total;
            return Convert.ToInt32(total ?? 0) > 0;
        }

        public List<RolDto> ObtenerRoles()
        {
            var rows = AdoHelper.Query("SP_Roles_GetActivos", CommandType.StoredProcedure);
            return rows.Select(r =>
            {
                var dict = r as IDictionary<string, object>;
                return new RolDto
                {
                    id = ValInt(dict, "RolId", "id"),
                    nombre = ValString(dict, "Nombre", "nombre"),
                    descripcion = ValString(dict, "Descripcion", "descripcion"),
                    activo = ValBool(dict, "Activo", "activo")
                };
            }).ToList();
        }

        private static string ValString(IDictionary<string, object> dict, params string[] keys)
        {
            foreach (var key in keys)
            {
                var k = dict?.Keys.FirstOrDefault(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (k != null && dict[k] != null)
                    return dict[k].ToString();
            }
            return "";
        }

        private static int ValInt(IDictionary<string, object> dict, params string[] keys)
        {
            foreach (var key in keys)
            {
                var k = dict?.Keys.FirstOrDefault(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (k != null && dict[k] != null)
                    return Convert.ToInt32(dict[k]);
            }
            return 0;
        }

        private static bool ValBool(IDictionary<string, object> dict, params string[] keys)
        {
            foreach (var key in keys)
            {
                var k = dict?.Keys.FirstOrDefault(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (k != null && dict[k] != null)
                {
                    var valor = dict[k].ToString().ToLowerInvariant();
                    return valor == "1" || valor == "true";
                }
            }
            return true;
        }

        public bool ExisteUsuario(string usuario, int? excluirId = null)
        {
            var rows = AdoHelper.Query("SP_Usuarios_Existe", CommandType.StoredProcedure,
                new SqlParameter("@usuario", usuario),
                new SqlParameter("@excluirId", (object)excluirId ?? DBNull.Value));

            var total = rows.FirstOrDefault()?.total;
            return Convert.ToInt32(total ?? 0) > 0;
        }

        #endregion

        #region Escritura
         
        public int Crear(UsuarioCrudRequest req, int registradoPor)
        {
            var row = AdoHelper.QuerySingle("SP_Usuarios_Insert", CommandType.StoredProcedure,
                new SqlParameter("@nombre", req.nombre ?? ""),
                new SqlParameter("@apellidos", req.apellidos ?? ""),
                new SqlParameter("@correo", req.correo ?? ""),
                new SqlParameter("@usuario", req.usuario ?? ""),
                new SqlParameter("@password", SqlDbType.VarChar, 100) { Value = (object)req.password ?? DBNull.Value },
                new SqlParameter("@rolId", req.rolId),
                new SqlParameter("@registradoPor", registradoPor));

            return row?.nuevoId != null ? Convert.ToInt32(row.nuevoId) : 0;
        }

        public bool Editar(UsuarioCrudRequest req, int actualizadoPor)
        {
            var row = AdoHelper.QuerySingle("SP_Usuarios_Update", CommandType.StoredProcedure,
                new SqlParameter("@id", req.id),
                new SqlParameter("@nombre", req.nombre ?? ""),
                new SqlParameter("@apellidos", req.apellidos ?? ""),
                new SqlParameter("@correo", req.correo ?? ""),
                new SqlParameter("@usuario", req.usuario ?? ""),
                new SqlParameter("@rolId", req.rolId),
                new SqlParameter("@actualizadoPor", actualizadoPor));

            return row?.filas != null && Convert.ToInt32(row.filas) > 0;
        }

        public bool CambiarPassword(int usuarioId, string nuevaPassword)
        {
            var row = AdoHelper.QuerySingle("SP_Usuarios_UpdatePassword", CommandType.StoredProcedure,
                new SqlParameter("@usuarioId", usuarioId),
                new SqlParameter("@password", SqlDbType.VarChar, 100) { Value = (object)nuevaPassword ?? DBNull.Value });

            return row?.filas != null && Convert.ToInt32(row.filas) > 0;
        }

        public bool CambiarPasswordPropio(int usuarioId, string passwordActual, string passwordNuevo)
        {
            if (!ValidarPasswordActual(usuarioId, passwordActual))
                return false;

            return CambiarPassword(usuarioId, passwordNuevo);
        }

        public bool CambiarRol(int usuarioId, int rolId, int actualizadoPor)
        {
            var row = AdoHelper.QuerySingle("SP_Usuarios_UpdateRol", CommandType.StoredProcedure,
                new SqlParameter("@usuarioId", usuarioId),
                new SqlParameter("@rolId", rolId),
                new SqlParameter("@actualizadoPor", actualizadoPor));

            return row?.filas != null && Convert.ToInt32(row.filas) > 0;
        }

        public bool Desactivar(int usuarioId, int desactivadoPor)
        {
            // Soft delete: el usuario ya no puede iniciar sesión, pero sus registros históricos se conservan.
            var row = AdoHelper.QuerySingle("SP_Usuarios_Disable", CommandType.StoredProcedure,
                new SqlParameter("@usuarioId", usuarioId),
                new SqlParameter("@desactivadoPor", desactivadoPor));

            return row?.filas != null && Convert.ToInt32(row.filas) > 0;
        }

        public bool Activar(int usuarioId, int activadoPor)
        {
            var row = AdoHelper.QuerySingle("SP_Usuarios_Enable", CommandType.StoredProcedure,
                new SqlParameter("@usuarioId", usuarioId),
                new SqlParameter("@activadoPor", activadoPor));

            return row?.filas != null && Convert.ToInt32(row.filas) > 0;
        }

        #endregion

        #region Helpers

        private static UsuarioDto MapUsuario(dynamic u)
        {
            var dict = (IDictionary<string, object>)u;

            return new UsuarioDto
            {
                id = GetInt(dict, "UsuarioId", "id"),
                nombre = GetString(dict, "Nombre", "nombre"),
                apellido = GetString(dict, "Apellidos", "apellido", "apellidos"),
                correo = GetString(dict, "Email", "correo", "email"),
                usuario = GetString(dict, "Usuario", "usuario", "userName"),
                rol = GetString(dict, "RolNombre", "rol", "Rol"),
                rolId = GetNullableInt(dict, "RolId", "rolId"),
                activo = GetBool(dict, "Activo", "activo")
            };
        }

        private static string GetString(IDictionary<string, object> dict, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (dict.ContainsKey(key) && dict[key] != null)
                    return dict[key].ToString();
            }
            return "";
        }

        private static int GetInt(IDictionary<string, object> dict, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (dict.ContainsKey(key) && dict[key] != null)
                    return Convert.ToInt32(dict[key]);
            }
            return 0;
        }

        private static int? GetNullableInt(IDictionary<string, object> dict, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (dict.ContainsKey(key) && dict[key] != null)
                    return (int?)Convert.ToInt32(dict[key]);
            }
            return null;
        }

        private static bool GetBool(IDictionary<string, object> dict, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (dict.ContainsKey(key) && dict[key] != null)
                {
                    var valor = dict[key].ToString().ToLowerInvariant();
                    return valor == "1" || valor == "true";
                }
            }
            return true;
        }

        #endregion
    }
}
