using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CRMSistema.Models.Contratos;
using CRMSistema.Models.Usuarios;
using Newtonsoft.Json;

namespace CRMSistema.Controllers.Base
{
    public class BaseController : Controller
    {
        protected string ToString(object value, string defaultValue = "")
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            return value.ToString();
        }

        protected int ToInt(object value, int defaultValue = 0)
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            if (value is int i) return i;
            if (int.TryParse(value.ToString(), out int result)) return result;
            return defaultValue;
        }

        protected long ToLong(object value, long defaultValue = 0)
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            if (value is long l) return l;
            if (long.TryParse(value.ToString(), out long result)) return result;
            return defaultValue;
        }

        protected double ToDouble(object value, double defaultValue = 0.0)
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            if (value is double d) return d;
            if (value is decimal dec) return (double)dec;
            if (double.TryParse(value.ToString(), out double result)) return result;
            return defaultValue;
        }

        protected decimal ToDecimal(object value, decimal defaultValue = 0)
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            if (value is decimal d) return d;
            if (decimal.TryParse(value.ToString(), out decimal result)) return result;
            return defaultValue;
        }

        protected bool ToBool(object value, bool defaultValue = false)
        {
            if (value == null || value == DBNull.Value) return defaultValue;
            if (value is bool b) return b;
            if (bool.TryParse(value.ToString(), out bool result)) return result;
            if (value.ToString() == "1" || value.ToString().ToLower() == "true" || value.ToString().ToLower() == "si")
                return true;
            return defaultValue;
        }

        protected ContentResult JsonContent(object data)
        {
            return Content(JsonConvert.SerializeObject(data), "application/json");
        }

        protected object GetSessionValue(string key)
        {
            return Session[key];
        }

        protected void SetSessionValue(string key, object value)
        {
            Session[key] = value;
        }

        // ─────────────────────────────────────────────────────────
        // Lectura segura de columnas desde ExpandoObject/dynamic
        // (los SPs pueden devolver nombres con casing distinto).
        // ─────────────────────────────────────────────────────────
        protected object Val(dynamic r, params string[] keys)
        {
            if (r == null) return null;
            var dict = r as IDictionary<string, object>;
            if (dict == null) return null;
            foreach (var key in keys)
            {
                var k = dict.Keys.FirstOrDefault(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));
                if (k != null) return dict[k];
            }
            return null;
        }

        protected string ValString(dynamic r, params string[] keys)
        {
            return ToString(Val(r, keys), "");
        }

        protected int ValInt(dynamic r, params string[] keys)
        {
            return ToInt(Val(r, keys), 0);
        }

        protected decimal ValDecimal(dynamic r, params string[] keys)
        {
            return ToDecimal(Val(r, keys), 0);
        }

        protected double ValDouble(dynamic r, params string[] keys)
        {
            return ToDouble(Val(r, keys), 0.0);
        }

        // ─────────────────────────────────────────────────────────
        // Helpers de rol y visibilidad de registros
        // ─────────────────────────────────────────────────────────

        protected string RolActual()
        {
            return Session?["Rol"]?.ToString() ?? "";
        }

        protected bool EsSupervisorOAdmin()
        {
            return AppRoles.EsSupervisorOAdmin(RolActual());
        }

        protected bool EsCoordinador()
        {
            return AppRoles.EsCoordinador(RolActual());
        }

        protected bool EsJefe()
        {
            return AppRoles.EsJefe(RolActual());
        }

        protected bool EsSuperadmin()
        {
            return AppRoles.EsSuperadmin(RolActual());
        }

        protected int? UsuarioIdActual()
        {
            return Session?["UsuarioId"] as int?;
        }

        protected string UsuarioNombreActual()
        {
            return Session?["UsuarioNombre"]?.ToString() ?? "";
        }

        /// <summary>
        /// Determina si el usuario actual puede ver un prospecto.
        /// Supervisores y superadmins ven todo; vendedores solo los asignados a ellos.
        /// </summary>
        protected bool PuedeVerProspecto(dynamic r)
        {
            if (EsSupervisorOAdmin())
                return true;

            var usuarioId = UsuarioIdActual();
            if (!usuarioId.HasValue) return false;

            // Preferir comparación por IDs
            var vendedorId = ToInt(Val(r, "vendedorId"));
            var propietarioId = ToInt(Val(r, "propietarioId"));
            if ((vendedorId > 0 && vendedorId == usuarioId.Value)
                || (propietarioId > 0 && propietarioId == usuarioId.Value))
                return true;

            // Fallback por nombre si no hay ID numérico
            var vendedorNombre = ToString(Val(r, "vendedorNombre"), "");
            var usuarioNombre = UsuarioNombreActual();
            return !string.IsNullOrWhiteSpace(vendedorNombre)
                && !string.IsNullOrWhiteSpace(usuarioNombre)
                && vendedorNombre.Equals(usuarioNombre, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determina si el usuario actual puede ver un contrato.
        /// Supervisores y superadmins ven todo; vendedores solo los de su prospecto asignado.
        /// </summary>
        protected bool PuedeVerContrato(ContratoAutorizadoModel c)
        {
            if (EsSupervisorOAdmin())
                return true;

            var usuarioId = UsuarioIdActual();
            if (!usuarioId.HasValue) return false;

            if (c.VendedorId > 0 && c.VendedorId == usuarioId.Value)
                return true;

            // Fallback por nombre
            var vendedorNombre = c.VendedorNombre ?? "";
            var usuarioNombre = UsuarioNombreActual();
            return !string.IsNullOrWhiteSpace(vendedorNombre)
                && !string.IsNullOrWhiteSpace(usuarioNombre)
                && vendedorNombre.Equals(usuarioNombre, StringComparison.OrdinalIgnoreCase);
        }
    }
}
