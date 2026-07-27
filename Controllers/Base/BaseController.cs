using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
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
    }
}
