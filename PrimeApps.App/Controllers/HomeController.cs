using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
	public class HomeController : Controller
    {
		[Authorize]
		public ActionResult Index([FromQuery(Name = "appId")] int localAppId = 0)
		{			
			var platformUserRepository = (IPlatformUserRepository)HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));
			var appId = platformUserRepository.GetAppIdByDomain(Request.Host.Value);

			if (appId == 0 && localAppId != 0)
				appId = localAppId;

			var tenant = platformUserRepository.GetTenantByEmailAndAppId(HttpContext.User.FindFirst("email").Value, appId);

			if (tenant == null)
				return BadRequest();

			Response.Cookies.Append("tenant_id", tenant.Id.ToString());			
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
            var language = lang ?? "tr";

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