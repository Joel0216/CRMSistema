using System;
using System.Web.Mvc;
using CRMSistema.Controllers.Base;
using CRMSistema.DAL.Usuarios;

namespace CRMSistema.Controllers.MiCuenta
{
    /// <summary>
    /// Operaciones de la cuenta del usuario autenticado.
    /// Accesible para cualquier usuario logueado, sin importar su rol.
    /// </summary>
    [Authorize]
    public class MiCuentaController : BaseController
    {
        private readonly UsuariosDAL _dal = new UsuariosDAL();

        /// <summary>
        /// Cambia la contraseña del usuario que está en sesión.
        /// Requiere la contraseña actual y confirmar la nueva.
        /// </summary>
        [HttpPost]
        public ActionResult CambiarPassword(string passwordActual, string passwordNuevo, string passwordConfirmar)
        {
            try
            {
                var usuarioActualId = Session["UsuarioId"] as int? ?? 0;
                if (usuarioActualId <= 0)
                    return Json(new { success = false, error = "No has iniciado sesión." });

                if (string.IsNullOrWhiteSpace(passwordActual) || string.IsNullOrWhiteSpace(passwordNuevo))
                    return Json(new { success = false, error = "Debes completar todos los campos de contraseña." });

                if (passwordNuevo != passwordConfirmar)
                    return Json(new { success = false, error = "La nueva contraseña y su confirmación no coinciden." });

                if (passwordNuevo.Length < 6)
                    return Json(new { success = false, error = "La nueva contraseña debe tener al menos 6 caracteres." });

                var ok = _dal.CambiarPasswordPropio(usuarioActualId, passwordActual, passwordNuevo);
                if (!ok)
                    return Json(new { success = false, error = "La contraseña actual es incorrecta." });

                return Json(new { success = true, message = "Contraseña actualizada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
