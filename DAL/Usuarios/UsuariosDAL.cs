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
            return rows.Select(r => new RolDto
            {
                id = r.id != null ? (int)r.id : 0,
                nombre = r.nombre?.ToString() ?? "",
                descripcion = r.descripcion?.ToString() ?? "",
                activo = r.activo != null && (r.activo.ToString() == "1" || r.activo.ToString().ToLower() == "true")
            }).ToList();
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
            return AdoHelper.Execute("SP_Usuarios_Insert", CommandType.StoredProcedure,
                new SqlParameter("@nombre", req.nombre ?? ""),
                new SqlParameter("@apellidos", req.apellidos ?? ""),
                new SqlParameter("@correo", req.correo ?? ""),
                new SqlParameter("@usuario", req.usuario ?? ""),
                new SqlParameter("@password", SqlDbType.VarChar, 100) { Value = (object)req.password ?? DBNull.Value },
                new SqlParameter("@rolId", req.rolId),
                new SqlParameter("@registradoPor", registradoPor));
        }

        public bool Editar(UsuarioCrudRequest req, int actualizadoPor)
        {
            return AdoHelper.Execute("SP_Usuarios_Update", CommandType.StoredProcedure,
                new SqlParameter("@id", req.id),
                new SqlParameter("@nombre", req.nombre ?? ""),
                new SqlParameter("@apellidos", req.apellidos ?? ""),
                new SqlParameter("@correo", req.correo ?? ""),
                new SqlParameter("@usuario", req.usuario ?? ""),
                new SqlParameter("@rolId", req.rolId),
                new SqlParameter("@actualizadoPor", actualizadoPor)) > 0;
        }

        public bool CambiarPassword(int usuarioId, string nuevaPassword)
        {
            return AdoHelper.Execute("SP_Usuarios_UpdatePassword", CommandType.StoredProcedure,
                new SqlParameter("@usuarioId", usuarioId),
                new SqlParameter("@password", SqlDbType.VarChar, 100) { Value = (object)nuevaPassword ?? DBNull.Value }) > 0;
        }

        public bool CambiarPasswordPropio(int usuarioId, string passwordActual, string passwordNuevo)
        {
            if (!ValidarPasswordActual(usuarioId, passwordActual))
                return false;

            return CambiarPassword(usuarioId, passwordNuevo);
        }

        public bool CambiarRol(int usuarioId, int rolId, int actualizadoPor)
        {
            return AdoHelper.Execute("SP_Usuarios_UpdateRol", CommandType.StoredProcedure,
                new SqlParameter("@usuarioId", usuarioId),
                new SqlParameter("@rolId", rolId),
                new SqlParameter("@actualizadoPor", actualizadoPor)) > 0;
        }

        public bool Desactivar(int usuarioId, int desactivadoPor)
        {
            // Soft delete: el usuario ya no puede iniciar sesión, pero sus registros históricos se conservan.
            return AdoHelper.Execute("SP_Usuarios_Disable", CommandType.StoredProcedure,
                new SqlParameter("@usuarioId", usuarioId),
                new SqlParameter("@desactivadoPor", desactivadoPor)) > 0;
        }

        public bool Activar(int usuarioId, int activadoPor)
        {
            return AdoHelper.Execute("SP_Usuarios_Enable", CommandType.StoredProcedure,
                new SqlParameter("@usuarioId", usuarioId),
                new SqlParameter("@activadoPor", activadoPor)) > 0;
        }

        #endregion

        #region Helpers

        private static UsuarioDto MapUsuario(dynamic u)
        {
            var dict = (IDictionary<string, object>)u;

            return new UsuarioDto
            {
                id = dict.ContainsKey("id") && u.id != null ? (int)u.id : 0,
                nombre = dict.ContainsKey("nombre") && u.nombre != null ? u.nombre.ToString() : "",
                apellido = dict.ContainsKey("apellido") && u.apellido != null ? u.apellido.ToString() : "",
                correo = dict.ContainsKey("correo") && u.correo != null ? u.correo.ToString() : "",
                usuario = dict.ContainsKey("usuario") && u.usuario != null ? u.usuario.ToString() : "",
                rol = dict.ContainsKey("rol") && u.rol != null ? u.rol.ToString() : "",
                rolId = dict.ContainsKey("rolId") && u.rolId != null ? (int?)Convert.ToInt32(u.rolId) : null,
                activo = !dict.ContainsKey("activo") || (u.activo != null && (u.activo.ToString() == "1" || u.activo.ToString().ToLower() == "true"))
            };
        }

        #endregion
    }
}
