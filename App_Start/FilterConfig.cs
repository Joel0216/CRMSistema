using System.Web.Mvc;
using CRMSistema.Filters;

namespace CRMSistema
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new JsonExceptionFilter());
        }
    }
}
