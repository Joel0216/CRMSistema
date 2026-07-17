using System;
using System.Collections.Generic;
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
    }
}
