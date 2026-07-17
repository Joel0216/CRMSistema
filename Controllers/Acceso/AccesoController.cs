using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CRMSistema.DAL.Usuarios;
using CRMSistema.Models.ViewModels;

namespace CRMSistema.Controllers.Acceso
{
    public class AccesoController : Controller
    {
        private readonly UsuariosDAL _usuariosDal = new UsuariosDAL();
        private readonly bool _modoDevSinBd;

        public AccesoController()
        {
            bool.TryParse(ConfigurationManager.AppSettings["ModoDesarrolloSinBD"], out _modoDevSinBd);
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            ViewBag.Title = "Iniciar sesión";
            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        public ActionResult LoginCliente()
        {
            ViewBag.Title = "Acceso Clientes";
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            ViewBag.Title = "Iniciar sesión";

            if (!ModelState.IsValid)
                return View(model);

            // Modo desarrollo sin BD: credenciales de prueba
            if (_modoDevSinBd)
            {
                if ((model.Usuario.Equals("admin", StringComparison.OrdinalIgnoreCase) && model.Password == "admin123") ||
                    (model.Usuario.Equals("vendedor", StringComparison.OrdinalIgnoreCase) && model.Password == "venta123"))
                {
                    FormsAuthentication.SetAuthCookie(model.Usuario, model.Recordarme);
                    Session["UsuarioNombre"] = model.Usuario;
                    Session["Rol"] = model.Usuario.ToLowerInvariant().Contains("admin") ? "admin" : "vendedor";
                    return RedirectToAction("Index", "Dashboard");
                }
            }

            // Validación contra base de datos
            try
            {
                var usuario = _usuariosDal.ValidarUsuario(model.Usuario, model.Password);
                if (usuario != null)
                {
                    FormsAuthentication.SetAuthCookie(usuario.nombre, model.Recordarme);
                    Session["UsuarioId"] = usuario.id;
                    Session["UsuarioNombre"] = usuario.nombre;
                    Session["Rol"] = usuario.rol;
                    return RedirectToAction("Index", "Dashboard");
                }
            }
            catch (Exception ex)
            {
                // En desarrollo, si falla la BD y el flag está activo, permite acceso directo
                if (_modoDevSinBd)
                {
                    FormsAuthentication.SetAuthCookie(model.Usuario, model.Recordarme);
                    Session["UsuarioNombre"] = model.Usuario;
                    Session["Rol"] = "admin";
                    return RedirectToAction("Index", "Dashboard");
                }
                model.Error = "No se pudo conectar a la base de datos: " + ex.Message;
                return View(model);
            }

            model.Error = "Usuario o contraseña incorrectos.";
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult LoginCliente(LoginViewModel model)
        {
            ViewBag.Title = "Acceso Clientes";

            if (!ModelState.IsValid)
                return View(model);

            // TODO: implementar validación de cliente contra tabla Clientes/Usuarios
            if (_modoDevSinBd ||
                (model.Usuario.Equals("cliente", StringComparison.OrdinalIgnoreCase) && model.Password == "cliente123"))
            {
                FormsAuthentication.SetAuthCookie(model.Usuario, model.Recordarme);
                Session["UsuarioNombre"] = model.Usuario;
                Session["Rol"] = "cliente";
                return RedirectToAction("Index", "Cliente");
            }

            model.Error = "Cliente o contraseña incorrectos.";
            return View(model);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login", "Acceso");
        }
    }
}
