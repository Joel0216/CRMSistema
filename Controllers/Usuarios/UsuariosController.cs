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
    /// Solo accesible para Superadmin.
    /// </summary>
    [AuthorizeRole(AppRoles.Superadmin)]
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
                var data = _dal.ObtenerTodos();
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
                var data = _dal.ObtenerRoles();
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

                if (req.rolId <= 0)
                    return Json(new { success = false, error = "Debes seleccionar un rol." });

                var usuarioActualId = Session["UsuarioId"] as int? ?? 0;

                if (req.id > 0)
                {
                    // Editar
                    if (_dal.ExisteUsuario(req.usuario, req.id))
                        return Json(new { success = false, error = "El nombre de usuario ya está registrado." });

                    var ok = _dal.Editar(req, usuarioActualId);
                    return Json(new { success = ok, message = ok ? "Usuario actualizado." : "No se pudo actualizar." });
                }
                else
                {
                    // Crear
                    if (string.IsNullOrWhiteSpace(req.password))
                        return Json(new { success = false, error = "La contraseña es obligatoria para crear un usuario." });

                    if (_dal.ExisteUsuario(req.usuario))
                        return Json(new { success = false, error = "El nombre de usuario ya está registrado." });

                    var nuevoId = _dal.Crear(req, usuarioActualId);
                    return Json(new { success = nuevoId > 0, id = nuevoId, message = nuevoId > 0 ? "Usuario creado correctamente." : "No se pudo crear el usuario." });
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

                var ok = _dal.CambiarPassword(id, password);
                return Json(new { success = ok, message = ok ? "Contraseña actualizada." : "No se pudo actualizar la contraseña." });
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
                var ok = _dal.CambiarRol(id, rolId, usuarioActualId);
                return Json(new { success = ok, message = ok ? "Rol actualizado." : "No se pudo cambiar el rol." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Anular(int id)
        {
            try
            {
                var usuarioActualId = Session["UsuarioId"] as int? ?? 0;
                if (id == usuarioActualId)
                    return Json(new { success = false, error = "No puedes anular tu propio usuario." });

                var ok = _dal.Desactivar(id, usuarioActualId);
                return Json(new { success = ok, message = ok ? "Usuario anulado." : "No se pudo anular." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Activar(int id)
        {
            try
            {
                var usuarioActualId = Session["UsuarioId"] as int? ?? 0;
                var ok = _dal.Activar(id, usuarioActualId);
                return Json(new { success = ok, message = ok ? "Usuario activado." : "No se pudo activar." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

    }
}
