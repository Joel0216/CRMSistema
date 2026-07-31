using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CRMSistema.DAL.Usuarios;
using CRMSistema.Models.Permisos;
using CRMSistema.Models.Usuarios;
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

            model.Usuario = (model.Usuario ?? "").Trim();
            model.Password = (model.Password ?? "").Trim();

            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(model.Usuario) || string.IsNullOrWhiteSpace(model.Password))
            {
                model.Error = "Debes ingresar usuario y contraseña.";
                return View(model);
            }

            // Modo desarrollo sin BD: credenciales de prueba
            if (_modoDevSinBd)
            {
                var credencialesDev = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "superadmin", "admin123" },
                    { "vendedor", "venta123" },
                    { "supervisor", "super123" },
                    { "coordinador", "coord123" },
                    { "jefe", "jefe123" }
                };

                if (credencialesDev.TryGetValue(model.Usuario, out string passEsperada) && model.Password == passEsperada)
                {
                    string rol;
                    var usuarioLower = model.Usuario.ToLowerInvariant();

                    if (usuarioLower.Contains("superadmin")) rol = AppRoles.Superadmin;
                    else if (usuarioLower.Contains("jefe")) rol = AppRoles.Jefe;
                    else if (usuarioLower.Contains("coordinador")) rol = AppRoles.Coordinador;
                    else if (usuarioLower.Contains("supervisor")) rol = AppRoles.Supervisor;
                    else rol = AppRoles.Vendedor;

                    FormsAuthentication.SetAuthCookie(model.Usuario, model.Recordarme);
                    Session["UsuarioNombre"] = model.Usuario;
                    Session["Rol"] = rol;
                    PermisoService.CargarPermisosEnSesion(null, true);
                    return RedirectToAction("Index", "Dashboard");
                }

                model.Error = "Usuario o contraseña incorrectos.";
                return View(model);
            }

            // Validación contra base de datos
            try
            {
                var usuario = _usuariosDal.ValidarUsuario(model.Usuario, model.Password);
                if (usuario != null)
                {
                    var nombreParaCookie = !string.IsNullOrWhiteSpace(usuario.nombre) ? usuario.nombre : usuario.usuario;
                    FormsAuthentication.SetAuthCookie(nombreParaCookie, model.Recordarme);
                    Session["UsuarioId"] = usuario.id;
                    Session["UsuarioNombre"] = nombreParaCookie;
                    Session["Rol"] = usuario.rol;
                    PermisoService.CargarPermisosEnSesion(usuario.rolId, _modoDevSinBd);
                    return RedirectToAction("Index", "Dashboard");
                }
            }
            catch (Exception ex)
            {
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

            model.Usuario = (model.Usuario ?? "").Trim();
            model.Password = (model.Password ?? "").Trim();

            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(model.Usuario) || string.IsNullOrWhiteSpace(model.Password))
            {
                model.Error = "Debes ingresar usuario y contraseña.";
                return View(model);
            }

            // TODO: implementar validación de cliente contra tabla Clientes/Usuarios.
            // Mientras tanto solo se permite la cuenta de demostración cliente/cliente123;
            // el modo desarrollo NO habilita credenciales arbitrarias en el portal.
            if (model.Usuario.Equals("cliente", StringComparison.OrdinalIgnoreCase) && model.Password == "cliente123")
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
