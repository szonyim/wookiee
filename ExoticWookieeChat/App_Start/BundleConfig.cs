using System.Web;
using System.Web.Optimization;

namespace ExoticWookieeChat
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                    "~/Public/vendor/jquery/jquery-3.3.1.min.js",
                    "~/Public/vendor/jquery/jquery.cookie.js"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                    "~/Public/vendor/modernizr/modernizr.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                    "~/Public/vendor/bootstrap/js/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/site").Include(
                    "~/Public/js/EWCGlobal.js",
                    "~/Public/js/EWCCustomer.js",
                    "~/Public/js/EWCSupport.js"
            ));


            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Public/vendor/bootstrap/css/bootstrap.css",
                      "~/Public/css/Site.css"));
        }
    }
}
