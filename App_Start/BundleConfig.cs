using System.Web;
using System.Web.Optimization;

namespace CRMSistema
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Usamos Bundle (no ScriptBundle) para evitar que WebGrease intente
            // minificar archivos de terceros con ES6 moderno (Bootstrap 5, jQuery 3.7).
            AddScriptBundle(bundles, "~/bundles/jquery",
                "~/Scripts/jquery-3.7.1.min.js");

            AddScriptBundle(bundles, "~/bundles/jqueryval",
                "~/Scripts/jquery.validate.min.js",
                "~/Scripts/jquery.validate.unobtrusive.min.js");

            AddScriptBundle(bundles, "~/bundles/modernizr",
                "~/Scripts/modernizr-2.8.3.js");

            AddScriptBundle(bundles, "~/bundles/bootstrap",
                "~/Scripts/bootstrap.bundle.min.js");

            AddScriptBundle(bundles, "~/bundles/site",
                "~/Scripts/site.js");

            var cssBundle = new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.min.css",
                      "~/Content/site.css");
            bundles.Add(cssBundle);
        }

        private static void AddScriptBundle(BundleCollection bundles, string virtualPath, params string[] files)
        {
            var bundle = new Bundle(virtualPath);
            foreach (var file in files)
                bundle.Include(file);
            bundle.Transforms.Clear();
            bundles.Add(bundle);
        }
    }
}
