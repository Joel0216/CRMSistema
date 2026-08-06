using System;
using System.Linq;
using System.Web.Mvc;
using CRMSistema.Controllers.Base;
using CRMSistema.DAL.Usuarios;
using CRMSistema.Filters;
using CRMSistema.Models.Usuarios;


namespace CRMSistema.Controllers.Usuarios
{
    /// <summary>
    /// Administración de usuarios del sistema.
    /// Accesible para Superadmin y Jefe.
    /// El Jefe puede crear y ver usuarios, pero no gestionar cuentas Superadmin.
    /// </summary>
    [AuthorizeRole(AppRoles.Jefe, AppRoles.Superadmin)]
    public class UsuariosController : BaseController
    {
        private readonly UsuariosDAL _dal = new UsuariosDAL();

        public ActionResult Index()
        {
            ViewBag.Title = "Usuarios";
            ViewBag.ActiveMenu = "Usuarios";
            return View();
        }

        [HttpGet]
        public ActionResult GetUsuarios()
        {
            try
            {
                var data = _dal.ObtenerTodos()
                    .Where(u => PuedeGestionarUsuario(u.rol))
                    .ToList();
                return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetRoles()
        {
            try
            {
                var data = _dal.ObtenerRoles()
                    .Where(r => PuedeVerRol(r.nombre))
                    .ToList();
                return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetUsuario(int id)
        {
            try
            {
                var usuario = _dal.ObtenerPorId(id);
                if (usuario == null)
                    return Json(new { success = false, error = "Usuario no encontrado." }, JsonRequestBehavior.AllowGet);

                return Json(new { success = true, usuario }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Guardar(UsuarioCrudRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.nombre) || string.IsNullOrWhiteSpace(req.usuario))
                    return Json(new { success = false, error = "Nombre y usuario son obligatorios." });

                if (string.IsNullOrWhiteSpace(req.correo))
                    return Json(new { success = false, error = "El correo electrónico es obligatorio." });

                var correoLimpio = req.correo.Trim();
                if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(correoLimpio))
                    return Json(new { success = false, error = "El correo electrónico no tiene un formato válido." });

                if (_dal.ObtenerTodos().Any(u => !string.IsNullOrWhiteSpace(u.correo) &&
                    u.correo.Trim().Equals(correoLimpio, StringComparison.OrdinalIgnoreCase) && u.id != req.id))
                    return Json(new { success = false, error = "El correo electrónico ya está registrado. Usa otro correo." });

                req.correo = correoLimpio;

                if (req.rolId <= 0)
                    return Json(new { success = false, error = "Debes seleccionar un rol." });

                var usuarioActualId = Session["UsuarioId"] as int? ?? 0;
                var rolSeleccionado = _dal.ObtenerRoles().FirstOrDefault(r => r.id == req.rolId);
                var nombreRolSeleccionado = rolSeleccionado?.nombre ?? "";

                if (!PuedeAsignarRol(nombreRolSeleccionado))
                    return Json(new { success = false, error = "No tienes permiso para asignar ese rol." });

                if (req.id > 0)
                {
                    // Editar
                    var usuarioExistente = _dal.ObtenerPorId(req.id);
                    if (usuarioExistente != null && !PuedeGestionarUsuario(usuarioExistente.rol))
                        return Json(new { success = false, error = "No tienes permiso para editar este usuario." });

                    if (_dal.ExisteUsuario(req.usuario, req.id))
                        return Json(new { success = false, error = "El nombre de usuario ya está registrado." });

                    var ok = _dal.Editar(req, usuarioActualId);
                    if (ok)
                        return Json(new { success = true, message = "Usuario actualizado." });
                    return Json(new { success = false, error = "No se pudo actualizar." });
                }
                else
                {
                    // Crear
                    if (string.IsNullOrWhiteSpace(req.password))
                        return Json(new { success = false, error = "La contraseña es obligatoria para crear un usuario." });

                    if (_dal.ExisteUsuario(req.usuario))
                        return Json(new { success = false, error = "El nombre de usuario ya está registrado." });

                    var nuevoId = _dal.Crear(req, usuarioActualId);
                    if (nuevoId > 0)
                        return Json(new { success = true, id = nuevoId, message = "Usuario creado correctamente." });
                    return Json(new { success = false, error = "No se pudo crear el usuario." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult CambiarPassword(int id, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                    return Json(new { success = false, error = "La contraseña no puede estar vacía." });

                var usuario = _dal.ObtenerPorId(id);
                if (usuario != null && !PuedeGestionarUsuario(usuario.rol))
                    return Json(new { success = false, error = "No tienes permiso para cambiar la contraseña de este usuario." });

                var ok = _dal.CambiarPassword(id, password);
                if (ok)
                    return Json(new { success = true, message = "Contraseña actualizada." });
                return Json(new { success = false, error = "No se pudo actualizar la contraseña." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult CambiarRol(int id, int rolId)
        {
            try
            {
                var usuarioActualId = Session["UsuarioId"] as int? ?? 0;
                var usuario = _dal.ObtenerPorId(id);
                if (usuario != null && !PuedeGestionarUsuario(usuario.rol))
                    return Json(new { success = false, error = "No tienes permiso para cambiar el rol de este usuario." });

                var rolDestino = _dal.ObtenerRoles().FirstOrDefault(r => r.id == rolId);
                if (rolDestino != null && !PuedeAsignarRol(rolDestino.nombre))
                    return Json(new { success = false, error = "No tienes permiso para asignar ese rol." });

                var ok = _dal.CambiarRol(id, rolId, usuarioActualId);
                if (ok)
                    return Json(new { success = true, message = "Rol actualizado." });
                return Json(new { success = false, error = "No se pudo cambiar el rol." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Anular(UsuarioIdRequest req)
        {
            try
            {
                var id = req?.id ?? 0;
                var usuarioActualId = Session["UsuarioId"] as int? ?? 0;
                if (id == usuarioActualId)
                    return Json(new { success = false, error = "No puedes anular tu propio usuario." });

                var usuario = _dal.ObtenerPorId(id);
                if (usuario != null && !PuedeGestionarUsuario(usuario.rol))
                    return Json(new { success = false, error = "No tienes permiso para anular este usuario." });

                var ok = _dal.Desactivar(id, usuarioActualId);
                if (ok)
                    return Json(new { success = true, message = "Usuario anulado." });
                return Json(new { success = false, error = "No se pudo anular." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Activar(UsuarioIdRequest req)
        {
            try
            {
                var id = req?.id ?? 0;
                var usuarioActualId = Session["UsuarioId"] as int? ?? 0;

                var usuario = _dal.ObtenerPorId(id);
                if (usuario != null && !PuedeGestionarUsuario(usuario.rol))
                    return Json(new { success = false, error = "No tienes permiso para activar este usuario." });

                var ok = _dal.Activar(id, usuarioActualId);
                if (ok)
                    return Json(new { success = true, message = "Usuario activado." });
                return Json(new { success = false, error = "No se pudo activar." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        #region Helpers de permisos sobre usuarios

        /// <summary>
        /// Superadmin puede gestionar cualquier usuario.
        /// Jefe puede gestionar usuarios con rol Vendedor, Supervisor, Coordinador o Jefe.
        /// Ningún otro rol debería llegar aquí gracias al AuthorizeRole.
        /// </summary>
        private bool PuedeGestionarUsuario(string rolUsuario)
        {
            if (EsSuperadmin()) return true;
            if (!EsJefe()) return false;

            // Jefe no puede tocar cuentas Superadmin
            return !AppRoles.EsSuperadmin(rolUsuario);
        }

        /// <summary>
        /// Superadmin puede asignar cualquier rol.
        /// Jefe puede asignar Vendedor, Supervisor, Coordinador o Jefe, pero no Superadmin.
        /// </summary>
        private bool PuedeAsignarRol(string rolAsignar)
        {
            if (EsSuperadmin()) return true;
            if (!EsJefe()) return false;

            return !AppRoles.EsSuperadmin(rolAsignar);
        }

        /// <summary>
        /// Superadmin puede ver todos los roles en el dropdown.
        /// Jefe solo ve roles que puede asignar.
        /// </summary>
        private bool PuedeVerRol(string nombreRol)
        {
            if (EsSuperadmin()) return true;
            if (!EsJefe()) return false;

            return !AppRoles.EsSuperadmin(nombreRol);
        }

        #endregion
    }
}
