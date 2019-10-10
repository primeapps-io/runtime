using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;
using WorkflowCore.Interface;

namespace PrimeApps.App.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;
        private IApplicationRepository _applicationRepository;
        private IUserRepository _userRepository;
        private IEnvironmentHelper _environmentHelper;

        public HomeController(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, IApplicationRepository applicationRepository,
            IUserRepository userRepository, IEnvironmentHelper environmentHelper)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _applicationRepository = applicationRepository;
            _userRepository = userRepository;
            _environmentHelper = environmentHelper;
        }

        [Authorize]
        public async Task<ActionResult> Index([FromQuery]string preview = null)
        {
            var applicationRepository = (IApplicationRepository)HttpContext.RequestServices.GetService(typeof(IApplicationRepository));
            var tenantRepository = (ITenantRepository)HttpContext.RequestServices.GetService(typeof(ITenantRepository));
            var platformUserRepository = (IPlatformUserRepository)HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));

            //var cryptoString = CryptoHelper.Encrypt("app_id=5", "222EF106646458CD59995D4378B55DF2");
            //var decrypto = CryptoHelper.Decrypt("dZ1AdGTKBHFtas+vLzM30fPlOkVmzwJn7Nzd/nkcGH0=", "222EF106646458CD59995D4378B55DF2");

            if (preview != null)
            {
                var previewApp = AppHelper.GetPreviewApp(preview);

                if (!string.IsNullOrEmpty(previewApp))
                {
                    if (previewApp.Contains("app"))
                    {
                        var previewClient = _configuration.GetValue("AppSettings:PreviewClient", string.Empty);
                        var appId = int.Parse(previewApp.Split("app_id=")[1]);
                        var app = await applicationRepository.GetByNameAsync(!string.IsNullOrEmpty(previewClient) ? previewClient : "primeapps_preview");

                        var userId = await platformUserRepository.GetIdByEmail(HttpContext.User.FindFirst("email").Value);

                        _userRepository.CurrentUser = new CurrentUser { UserId = userId, TenantId = appId, PreviewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty) };

                        var appDraftUser = await _userRepository.GetByEmail(HttpContext.User.FindFirst("email").Value);

                        if (appDraftUser == null)
                        {
                            Response.Cookies.Delete("app_id");
                            await HttpContext.SignOutAsync();
                            return Redirect(Request.Scheme + "://" + app.Setting.AuthDomain + "/Account/Logout?returnUrl=" + Request.Scheme + "://" + app.Setting.AppDomain + "?preview=" + preview + "&error=NotFound");
                        }

                        /*var tenant = await platformUserRepository.GetTenantByEmailAndAppId(HttpContext.User.FindFirst("email").Value, appId);
    
                        if (tenant == null)
                        {
                            Response.Cookies.Delete("app_id");
                            await HttpContext.SignOutAsync();
                            return Redirect(Request.Scheme + "://" + app.Setting.AuthDomain + "/Account/Logout?returnUrl=" + Request.Scheme + "://" + app.Setting.AppDomain);
                        }*/


                        await SetValues(userId, app, null, appId, true);

                        Response.Cookies.Append("app_id", appId.ToString());
                    }
                    else
                    {
                        var tenantId = int.Parse(previewApp.Split("tenant_id=")[1]);
                        var tenant = tenantRepository.Get(tenantId);

                        if (tenant == null)
                        {
                            Response.Cookies.Delete("tenant_id");
                            await HttpContext.SignOutAsync();
                            throw new Exception("Tenant Not Found !!");
                        }

                        var app = await applicationRepository.Get(tenant.AppId);

                        var userId = await platformUserRepository.GetIdByEmail(HttpContext.User.FindFirst("email").Value);

                        await SetValues(userId, app, tenant.Id, null, true);

                        Response.Cookies.Append("tenant_id", tenant.Id.ToString());
                    }
                }
                else
                {
                    Response.Cookies.Delete("app_id");
                    Response.Cookies.Delete("tenant_id");
                    await HttpContext.SignOutAsync();
                    ErrorHandler.LogError(new Exception("Preview token is not valid or null!"), "Primeapps.App -> HomeController -> Preview");
                }
            }
            else
            {
                var app = await applicationRepository.GetAppWithDomain(Request.Host.Value);

                var tenant = await platformUserRepository.GetTenantByEmailAndAppId(HttpContext.User.FindFirst("email").Value, app.Id);

                if (tenant == null)
                {
                    Response.Cookies.Delete("tenant_id");
                    await HttpContext.SignOutAsync();
                    return Redirect(Request.Scheme + "://" + app.Setting.AuthDomain + "/Account/Logout?returnUrl=" + Request.Scheme + "://" + app.Setting.AppDomain + "&error=NotFound");
                }

                var userId = await platformUserRepository.GetIdByEmail(HttpContext.User.FindFirst("email").Value);

                await SetValues(userId, app, tenant.Id, null, false);

                Response.Cookies.Append("tenant_id", tenant.Id.ToString());
            }

            return View();
        }

        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            var appInfo = await _applicationRepository.Get(Request.Host.Value);

            Response.Cookies.Delete("tenant_id");
            await HttpContext.SignOutAsync();

            var preview = HttpContext.Request.Query["preview"].ToString();
            preview = !string.IsNullOrEmpty(preview) ? "?preview=" + preview : "";

            return Redirect(Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/Account/Logout?returnUrl=" + Request.Scheme + "://" + appInfo.Setting.AppDomain + preview);
        }


        [Route("register")]
        public async Task<IActionResult> Register()
        {
            var appInfo = await _applicationRepository.Get(Request.Host.Value);

            return Redirect(Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/Account/Register?ReturnUrl=/connect/authorize/callback?client_id=" + appInfo.Name + "%26redirect_uri=" + Request.Scheme + "%3A%2F%2F" + appInfo.Setting.AppDomain + "%2Fsignin-oidc%26response_type=code%20id_token&scope=openid%20profile%20api1%20email&response_mode=form_post");
        }

        [HttpGet, Route("healthz")]
        public IActionResult Healthz()
        {
            return Ok();
        }

        private async Task SetValues(int userId, Model.Entities.Platform.App app, int? tenantId, int? appId, bool preview = false)
        {
            var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
            previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

            ViewBag.Token = await HttpContext.GetTokenAsync("access_token");

            var useCdn = _configuration.GetValue("AppSettings:UseCdn", string.Empty);
            ViewBag.AppInfo = await AppHelper.GetApplicationInfo(_configuration, Request, app, appId, preview);

            var storageUrl = _configuration.GetValue("AppSettings:StorageUrl", string.Empty);

            if (!string.IsNullOrEmpty(storageUrl))
            {
                ViewBag.BlobUrl = storageUrl;
            }

            if (!string.IsNullOrEmpty(useCdn) && bool.Parse(useCdn))
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

            var jsonString = "";
            var hasAdminRight = false;

            var componentRepository = (IComponentRepository)HttpContext.RequestServices.GetService(typeof(IComponentRepository));
            var scriptRepository = (IScriptRepository)HttpContext.RequestServices.GetService(typeof(IScriptRepository));
            var moduleRepository = (IModuleRepository)HttpContext.RequestServices.GetService(typeof(IModuleRepository));

            scriptRepository.CurrentUser = componentRepository.CurrentUser = moduleRepository.CurrentUser = new CurrentUser { UserId = userId, TenantId = previewMode == "app" ? (int)appId : (int)tenantId, PreviewMode = previewMode };

            var components = await componentRepository.GetByType(ComponentType.Component);
            components = _environmentHelper.DataFilter(components.ToList());

            if (components.Count > 0)
                jsonString = JsonConvert.SerializeObject(components);

            var globalSettings = await scriptRepository.GetGlobalSettings();
            globalSettings = _environmentHelper.DataFilter(globalSettings);
            
            var serializerSettings = JsonHelper.GetDefaultJsonSerializerSettings();
            var modules = await moduleRepository.GetAll();
            var modulesJson = JsonConvert.SerializeObject(modules, serializerSettings);

            //TODO: add all necessary objects here which are in appService.js getMyAccount method for increase performance
            var account = new JObject();
            account["modules"] = JArray.Parse(modulesJson);

            //TODO Account Suspended control !
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var userRepository = new UserRepository(databaseContext, _configuration))
                {
                    userRepository.CurrentUser = new CurrentUser { UserId = userId, TenantId = previewMode == "app" ? (int)appId : (int)tenantId, PreviewMode = previewMode };

                    var userInfo = await userRepository.GetUserInfoAsync(userId);

                    if (userInfo != null)
                        hasAdminRight = userInfo.profile.HasAdminRights;
                }
            }

            ViewBag.Preview = previewMode == "app";
            ViewBag.Components = jsonString;
            ViewBag.HasAdminRight = hasAdminRight;
            ViewBag.TenantId = tenantId;
            ViewBag.AppId = appId;
            ViewBag.EncryptedUserId = CryptoHelper.Encrypt(userId.ToString(), ".btA99KnTp+%','L");
            ViewBag.GlobalSettings = globalSettings != null ? globalSettings.Content : null;
            ViewBag.GoogleMapsApiKey = _configuration.GetValue("AppSettings:GoogleMapsApiKey", string.Empty);
            ViewBag.Account = account;
        }
    }
}