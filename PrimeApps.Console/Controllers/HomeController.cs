using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Console.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;

        public HomeController(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        [Authorize]
        public async Task<ActionResult> Index()
        {
            var platformUserRepository = (IPlatformUserRepository)HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));
            var userId = await platformUserRepository.GetIdByEmail(HttpContext.User.FindFirst("email").Value);

            await SetValues(userId);

            return View();
        }

        private async Task SetValues(int userId)
        {
            ViewBag.Token = await HttpContext.GetTokenAsync("access_token");

            var useCdn = bool.Parse(_configuration.GetSection("AppSettings")["UseCdn"]);
            ViewBag.BlobUrl = _configuration.GetSection("AppSettings")["BlobUrl"];
            ViewBag.FunctionUrl = _configuration.GetSection("AppSettings")["FunctionUrl"];

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