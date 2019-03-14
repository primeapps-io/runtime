using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;

namespace PrimeApps.Studio.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
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
            var emailConfirmed = jwtToken?.Claims.First(claim => claim.Type == "email_confirmed")?.Value;
            
            if (!string.IsNullOrEmpty(_configuration.GetValue("AppSettings:GiteaEnabled", string.Empty)) && bool.Parse(_configuration.GetValue("AppSettings:GiteaEnabled", string.Empty)))
            {
                var giteaToken = jwtToken?.Claims.First(claim => claim.Type == "gitea_token")?.Value;
                //var giteaToken = Request.Cookies["gitea_token"];
                //var giteaToken = jwtToken.Claims.First(claim => claim.Type == "gitea_token")?.Value;
                if (giteaToken != null)
                    Response.Cookies.Append("gitea_token", giteaToken);
            }

            var previewUrl = _configuration.GetValue("AppSettings:PreviewUrl", string.Empty);

            if (!string.IsNullOrEmpty(previewUrl))
                ViewBag.PreviewUrl = previewUrl;

            var blobUrl = _configuration.GetValue("AppSettings:BlobUrl", string.Empty);

            if (!string.IsNullOrEmpty(blobUrl))
                ViewBag.BlobUrl = blobUrl;

            var functionUrl = _configuration.GetValue("AppSettings:FunctionUrl", string.Empty);

            if (!string.IsNullOrEmpty(functionUrl))
                ViewBag.FunctionUrl = functionUrl;

            var giteaUrl = _configuration.GetValue("AppSettings:GiteaUrl", string.Empty);

            if (!string.IsNullOrEmpty(giteaUrl))
                ViewBag.GiteaUrl = giteaUrl;

            var useCdnSetting = _configuration.GetValue("AppSettings:UseCdn", string.Empty);
            var useCdn = false;

            if (!string.IsNullOrEmpty(useCdnSetting))
                useCdn = bool.Parse(useCdnSetting);

            if (useCdn)
            {
                var versionDynamic = System.Reflection.Assembly.GetAssembly(typeof(HomeController)).GetName().Version.ToString();
                var versionStatic = ((AssemblyVersionStaticAttribute)System.Reflection.Assembly.GetAssembly(typeof(HomeController)).GetCustomAttributes(typeof(AssemblyVersionStaticAttribute), false)[0]).Version;
                var cdnUrl = _configuration.GetValue("AppSettings:CdnUrl", string.Empty);

                if (!string.IsNullOrEmpty(cdnUrl))
                {
                    ViewBag.CdnUrlDynamic = cdnUrl + "/" + versionDynamic + "/";
                    ViewBag.CdnUrlStatic = cdnUrl + "/" + versionStatic + "/";
                }
            }
            else
            {
                ViewBag.CdnUrlDynamic = "";
                ViewBag.CdnUrlStatic = "";
            }
        }
    }
}