using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(ViewBag.Token) as JwtSecurityToken;
            var emailConfirmed = jwtToken.Claims.First(claim => claim.Type == "email_confirmed")?.Value;
            if (bool.Parse(_configuration.GetSection("AppSettings")["EnableGiteaIntegration"]))
            {
                var giteaToken = jwtToken.Claims.First(claim => claim.Type == "gitea_token")?.Value;

                Request.Cookies.Append(new System.Collections.Generic.KeyValuePair<string, string>("gitea_token", giteaToken));
                Response.Cookies.Append("gitea_token", giteaToken);
            }

            var useCdn = bool.Parse(_configuration.GetSection("AppSettings")["UseCdn"]);
            ViewBag.BlobUrl = _configuration.GetSection("AppSettings")["BlobUrl"];
            ViewBag.FunctionUrl = _configuration.GetSection("AppSettings")["FunctionUrl"];
            ViewBag.GiteaUrl = _configuration.GetSection("AppSettings")["GiteaUrl"];

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