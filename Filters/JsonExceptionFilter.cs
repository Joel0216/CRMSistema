using System;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace CRMSistema.Filters
{
    public class JsonExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
                return;

            var ex = filterContext.Exception ?? new Exception("Error desconocido");
            filterContext.Result = new ContentResult
            {
                Content = JsonConvert.SerializeObject(new { success = false, error = ex.Message }),
                ContentType = "application/json"
            };
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.StatusCode = 500;
        }
    }
}
