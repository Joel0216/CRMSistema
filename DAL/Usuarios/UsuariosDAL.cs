using System.Collections.Generic;
using System.Data;
using System.Linq;
using CRMSistema.Models.Usuarios;

namespace CRMSistema.DAL.Usuarios
{
    /// <summary>
    /// Acceso a datos para usuarios del sistema.
    /// </summary>
    public class UsuariosDAL
    {
        public List<UsuarioDto> ObtenerActivos()
        {
            var rows = AdoHelper.Query(@"
                SELECT UsuarioId as id, Nombre as nombre, Apellidos as apellido, Email as correo,
                       (SELECT Nombre FROM Roles WHERE Roles.RolId = Usuarios.RolId) as rol
                FROM Usuarios
                WHERE Activo = 1 OR Activo = '1'", CommandType.Text);

            return rows.Select(u => new UsuarioDto
            {
                id = u.id != null ? (int)u.id : 0,
                nombre = u.nombre?.ToString() ?? "",
                apellido = u.apellido?.ToString() ?? "",
                correo = u.correo?.ToString() ?? "",
                rol = u.rol?.ToString() ?? ""
            }).ToList();
        }

        public UsuarioDto ValidarUsuario(string usuario, string password)
        {
            var rows = AdoHelper.Query(@"
                SELECT UsuarioId as id, Nombre as nombre, Apellidos as apellido, Email as correo,
                       (SELECT Nombre FROM Roles WHERE Roles.RolId = Usuarios.RolId) as rol
                FROM Usuarios
                WHERE (Usuario = @usuario OR Email = @usuario)
                  AND PasswordHash = HASHBYTES('SHA2_256', @password)
                  AND (Activo = 1 OR Activo = '1')",
                CommandType.Text,
                new System.Data.SqlClient.SqlParameter("@usuario", usuario),
                new System.Data.SqlClient.SqlParameter("@password", password));

            var row = rows.FirstOrDefault();
            if (row == null) return null;

            return new UsuarioDto
            {
                id = row.id != null ? (int)row.id : 0,
                nombre = row.nombre?.ToString() ?? "",
                apellido = row.apellido?.ToString() ?? "",
                correo = row.correo?.ToString() ?? "",
                rol = row.rol?.ToString() ?? ""
            };
        }
    }
}
