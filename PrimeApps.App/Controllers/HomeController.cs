using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Authorize]
        public ActionResult Index([FromQuery(Name = "appId")] int localAppId = 0)
        {
            if (string.IsNullOrEmpty(Request.Cookies["tenant_id"]))
            {
                var applicationRepository = (IApplicationRepository)HttpContext.RequestServices.GetService(typeof(IApplicationRepository));
                var platformUserRepository = (IPlatformUserRepository)HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));

				var appId = localAppId != 0 ? localAppId : applicationRepository.GetAppIdWithDomain(Request.Host.Value);

				if (appId == 0)
					appId = 1;

                var tenant = platformUserRepository.GetTenantByEmailAndAppId(HttpContext.User.FindFirst("email").Value, appId);

                if (tenant == null)
                    return BadRequest();


                Response.Cookies.Append("tenant_id", tenant.Id.ToString());
            }

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

            var useCdn = bool.Parse(_configuration.GetSection("AppSettings")["UseCdn"]);
            ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, language, _configuration);

            if (useCdn)
            {
                var versionDynamic = System.Reflection.Assembly.GetAssembly(typeof(HomeController)).GetName().Version.ToString();
                var versionStatic = ((AssemblyVersionStaticAttribute)System.Reflection.Assembly.GetAssembly(typeof(HomeController)).GetCustomAttributes(typeof(AssemblyVersionStaticAttribute), false)[0]).Version;
                ViewBag.CdnUrlDynamic = _configuration.GetSection("AppSettings")["CdnUrl"] + "/" + versionDynamic + "/";
                ViewBag.CdnUrlStatic = _configuration.GetSection("AppSettings")["CdnUrl"] + "/" + versionStatic + "/";
            }
            else
            {
                ViewBag.CdnUrlDynamic = "";
                ViewBag.CdnUrlStatic = "";
            }
        }
    }
}