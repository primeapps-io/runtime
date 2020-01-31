using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Services;

namespace PrimeApps.Studio.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration;
        private IBackgroundTaskQueue _queue;
        private IMigrationHelper _migrationHelper;

        public HomeController(IConfiguration configuration, IBackgroundTaskQueue queue, IMigrationHelper migrationHelper)
        {
            _configuration = configuration;
            _queue = queue;
            _migrationHelper = migrationHelper;
        }

        [Authorize]
        public async Task<ActionResult> Index()
        {
            var platformUserRepository = (IPlatformUserRepository)HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));

            var userId = await platformUserRepository.GetIdByEmail(HttpContext.User.FindFirst("email").Value);
            await SetValues(userId);

            var b = WebUtility.HtmlEncode("<License><Data><LicensedTo>PrimeApps Bilgi Teknolojileri</LicensedTo><EmailTo>serdar.turan@primeapps.io</EmailTo><LicenseType>Developer OEM</LicenseType> <LicenseNote>Limited to 1 developer, unlimited physical locations</LicenseNote><OrderID>191206081533</OrderID>    <UserID>135031388</UserID><OEM>This is a redistributable license</OEM><Products><Product>Aspose.Total for .NET</Product></Products><EditionType>Enterprise</EditionType>  <SerialNumber>5c201cac-7a73-46cc-991b-dafd33fca928</SerialNumber><SubscriptionExpiry>20201206</SubscriptionExpiry><LicenseVersion>3.0</LicenseVersion><LicenseInstructions>https://purchase.aspose.com/policies/use-license</LicenseInstructions></Data> <Signature>NJscWkrrUWMD6zB5dlG/3mT2TgsziDpg2X6bHIa4TLvyum0xc0FY9XmL0TTuDRWTnme8P12HVxdBWB1jVURkAaUrxW8aSJVJvETxfznX6uJJRFBy/GQR6g/JH5klOG68kFj6XN2xiZ01rcIMXrO3oXwQ8vgg0Y5q7pQucehT+rY=</Signature></License>");
            var a = WebUtility.HtmlDecode(b);

              var bb = HttpUtility.HtmlEncode("<License><Data><LicensedTo>PrimeApps Bilgi Teknolojileri</LicensedTo><EmailTo>serdar.turan@primeapps.io</EmailTo><LicenseType>Developer OEM</LicenseType> <LicenseNote>Limited to 1 developer, unlimited physical locations</LicenseNote><OrderID>191206081533</OrderID>    <UserID>135031388</UserID><OEM>This is a redistributable license</OEM><Products><Product>Aspose.Total for .NET</Product></Products><EditionType>Enterprise</EditionType>  <SerialNumber>5c201cac-7a73-46cc-991b-dafd33fca928</SerialNumber><SubscriptionExpiry>20201206</SubscriptionExpiry><LicenseVersion>3.0</LicenseVersion><LicenseInstructions>https://purchase.aspose.com/policies/use-license</LicenseInstructions></Data> <Signature>NJscWkrrUWMD6zB5dlG/3mT2TgsziDpg2X6bHIa4TLvyum0xc0FY9XmL0TTuDRWTnme8P12HVxdBWB1jVURkAaUrxW8aSJVJvETxfznX6uJJRFBy/GQR6g/JH5klOG68kFj6XN2xiZ01rcIMXrO3oXwQ8vgg0Y5q7pQucehT+rY=</Signature></License>");
            var aa= HttpUtility.HtmlDecode("&lt;License&gt;&lt;Data&gt;&lt;LicensedTo&gt;PrimeApps Bilgi Teknolojileri&lt;/LicensedTo&gt;&lt;EmailTo&gt;serdar.turan@primeapps.io&lt;/EmailTo&gt;&lt;LicenseType&gt;Developer OEM&lt;/LicenseType&gt; &lt;LicenseNote&gt;Limited to 1 developer, unlimited physical locations&lt;/LicenseNote&gt;&lt;OrderID&gt;191206081533&lt;/OrderID&gt;    &lt;UserID&gt;135031388&lt;/UserID&gt;&lt;OEM&gt;This is a redistributable license&lt;/OEM&gt;&lt;Products&gt;&lt;Product&gt;Aspose.Total for .NET&lt;/Product&gt;&lt;/Products&gt;&lt;EditionType&gt;Enterprise&lt;/EditionType&gt;  &lt;SerialNumber&gt;5c201cac-7a73-46cc-991b-dafd33fca928&lt;/SerialNumber&gt;&lt;SubscriptionExpiry&gt;20201206&lt;/SubscriptionExpiry&gt;&lt;LicenseVersion&gt;3.0&lt;/LicenseVersion&gt;&lt;LicenseInstructions&gt;https://purchase.aspose.com/policies/use-license&lt;/LicenseInstructions&gt;&lt;/Data&gt; &lt;Signature&gt;NJscWkrrUWMD6zB5dlG/3mT2TgsziDpg2X6bHIa4TLvyum0xc0FY9XmL0TTuDRWTnme8P12HVxdBWB1jVURkAaUrxW8aSJVJvETxfznX6uJJRFBy/GQR6g/JH5klOG68kFj6XN2xiZ01rcIMXrO3oXwQ8vgg0Y5q7pQucehT+rY=&lt;/Signature&gt;&lt;/License&gt;");


            return View();
        }

        [Route("register")]
        public async Task<IActionResult> Register()
        {
            var applicationRepository = (IApplicationRepository)HttpContext.RequestServices.GetService(typeof(IApplicationRepository));

            var appInfo = await applicationRepository.Get(Request.Host.Value);

            return Redirect(Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/Account/Register?ReturnUrl=/connect/authorize/callback?client_id=" + appInfo.Name + "%26redirect_uri=" + Request.Scheme + "%3A%2F%2F" + appInfo.Setting.AppDomain + "%2Fsignin-oidc%26response_type=code%20id_token&scope=openid%20profile%20api1%20email&response_mode=form_post");
        }

        [Authorize, Route("set_gitea_token")]
        public async Task<IActionResult> SetGiteaToken()
        {
            var applicationRepository = (IApplicationRepository)HttpContext.RequestServices.GetService(typeof(IApplicationRepository));

            var appInfo = await applicationRepository.Get(Request.Host.Value);

            return Redirect(Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/Account/Register?ReturnUrl=/connect/authorize/callback?client_id=" + appInfo.Name + "%26redirect_uri=" + Request.Scheme + "%3A%2F%2F" + appInfo.Setting.AppDomain + "%2Fsignin-oidc%26response_type=code%20id_token&scope=openid%20profile%20api1%20email&response_mode=form_post");
        }

        [HttpGet, Route("healthz")]
        public IActionResult Healthz()
        {
            return Ok();
        }

        [HttpGet, Route("migration")]
        public IActionResult Migration()
        {
            var isLocal = Request.Host.Value.Contains("localhost");
            var schema = Request.Scheme;
            _queue.QueueBackgroundWorkItem(token => _migrationHelper.Apply(schema, isLocal));
            return Ok();
        }

        private async Task SetValues(int userId)
        {
            ViewBag.Token = await HttpContext.GetTokenAsync("access_token");
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(ViewBag.Token) as JwtSecurityToken;
            var emailConfirmed = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "email_confirmed")?.Value;

            if (!string.IsNullOrEmpty(_configuration.GetValue("AppSettings:GiteaEnabled", string.Empty)) && bool.Parse(_configuration.GetValue("AppSettings:GiteaEnabled", string.Empty)))
            {
                var giteaToken = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == "gitea_token")?.Value;
                //var giteaToken = Request.Cookies["gitea_token"];
                //var giteaToken = jwtToken.Claims.First(claim => claim.Type == "gitea_token")?.Value;
                if (giteaToken != null)
                    Response.Cookies.Append("gitea_token", giteaToken);
            }

            var previewUrl = _configuration.GetValue("AppSettings:PreviewUrl", string.Empty);

            if (!string.IsNullOrEmpty(previewUrl))
                ViewBag.PreviewUrl = previewUrl;

            var blobUrl = _configuration.GetValue("AppSettings:StorageUrl", string.Empty);

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