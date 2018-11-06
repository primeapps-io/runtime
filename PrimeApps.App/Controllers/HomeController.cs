using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PrimeApps.App.Helpers;
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
        private IDefinitionLoader _definitionLoader;
        private IWorkflowRegistry _workflowRegistry;

        public HomeController(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, IDefinitionLoader definitionLoader, IWorkflowRegistry workflowRegistry)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _definitionLoader = definitionLoader;
            _workflowRegistry = workflowRegistry;
        }

        [Authorize]
        public async Task<ActionResult> Index()
        {
            var applicationRepository = (IApplicationRepository)HttpContext.RequestServices.GetService(typeof(IApplicationRepository));
            var platformUserRepository = (IPlatformUserRepository)HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));

            var app = await applicationRepository.GetAppWithDomain(Request.Host.Value);

            var tenant = await platformUserRepository.GetTenantByEmailAndAppId(HttpContext.User.FindFirst("email").Value, app.Id);

            if (tenant == null)
            {
                Response.Cookies.Delete("tenant_id");
                await HttpContext.SignOutAsync();
                return Redirect(Request.Scheme + "://" + app.Setting.AuthDomain + "/Account/Logout?returnUrl=" + Request.Scheme + "://" + app.Setting.AppDomain);
            }

            var userId = await platformUserRepository.GetIdByEmail(HttpContext.User.FindFirst("email").Value);

            await SetValues(userId, tenant.Id);

            Response.Cookies.Append("tenant_id", tenant.Id.ToString());
            
            return View();
        }

        public async Task<ActionResult> Preview()
        {
            //await SetValues();

            return View("Index");
        }

       
        private async Task SetValues(int userId, int tenantId)
        {
            ViewBag.Token = await HttpContext.GetTokenAsync("access_token");

            var lang = Request.Cookies["_lang"];
            var language = lang ?? "tr";

            var useCdn = bool.Parse(_configuration.GetSection("AppSettings")["UseCdn"]);
            ViewBag.AppInfo = await AuthHelper.GetApplicationInfo(Request, language, _configuration);
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

            string jsonString = "";
            var hasAdminRight = false;

            var componentRepository = (IComponentRepository)HttpContext.RequestServices.GetService(typeof(IComponentRepository));
            componentRepository.CurrentUser = new CurrentUser { UserId = userId, TenantId = tenantId };
            var components = await componentRepository.GetByType(ComponentType.Component);

            if(components.Count > 0)
                jsonString = JsonConvert.SerializeObject(components);

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var userRepository = new UserRepository(databaseContext, _configuration))
                {
                    userRepository.CurrentUser = new CurrentUser { UserId = userId, TenantId = tenantId };
                    var userInfo = await userRepository.GetUserInfoAsync(userId);

                    if (userInfo != null)
                    {
                        hasAdminRight = userInfo.profile.HasAdminRights;
                    }
                }
            }
            ViewBag.Components = jsonString;
            ViewBag.HasAdminRight = hasAdminRight;
            ViewBag.TenantId = tenantId;
        }
    }
}