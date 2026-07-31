using System.Web.Mvc;
using CRMSistema.Filters;

namespace CRMSistema
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // JsonExceptionFilter debe ejecutarse primero para peticiones AJAX,
            // de lo contrario HandleErrorAttribute devuelve la vista Error.cshtml (HTML).
            filters.Add(new JsonExceptionFilter());
            filters.Add(new HandleErrorAttribute());
        }
    }
}
