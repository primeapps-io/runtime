using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
using PrimeApps.App.Helpers;

namespace PrimeApps.App.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public async Task<ActionResult> Index()
        {
            await SetValues();

            return View();
        }

        public async Task<ActionResult> Preview()
        {
            await SetValues();

            return View("Index");
        }

        private async Task SetValues()
        {
            var lang = Request.Cookies["_lang"];
            var language = lang != null ? lang.Value : "tr";

            var useCdn = bool.Parse(ConfigurationManager.AppSettings["UseCdn"]);
            ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, language);

            if (useCdn)
            {
                var versionDynamic = System.Reflection.Assembly.GetAssembly(typeof(HomeController)).GetName().Version.ToString();
                var versionStatic = ((AssemblyVersionStaticAttribute)System.Reflection.Assembly.GetAssembly(typeof(HomeController)).GetCustomAttributes(typeof(AssemblyVersionStaticAttribute), false)[0]).Version;
                ViewBag.CdnUrlDynamic = ConfigurationManager.AppSettings["CdnUrl"] + "/" + versionDynamic + "/";
                ViewBag.CdnUrlStatic = ConfigurationManager.AppSettings["CdnUrl"] + "/" + versionStatic + "/";
            }
            else
            {
                ViewBag.CdnUrlDynamic = "";
                ViewBag.CdnUrlStatic = "";
            }
        }
    }
}