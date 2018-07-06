using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
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
			var applicationRepository = (IApplicationRepository)HttpContext.RequestServices.GetService(typeof(IApplicationRepository));
			var platformUserRepository = (IPlatformUserRepository)HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));

			var appId = await applicationRepository.GetAppIdWithDomain(Request.Host.Value);

			var tenant = await platformUserRepository.GetTenantByEmailAndAppId(HttpContext.User.FindFirst("email").Value, appId);

			if (tenant == null)
				return BadRequest();

			/*var userId = await platformUserRepository.GetIdByEmail(HttpContext.User.FindFirst("email").Value);

			await SetValues(userId, tenant.Id);*/

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

			var componentRepository = (IComponentRepository)HttpContext.RequestServices.GetService(typeof(IComponentRepository));
			componentRepository.CurrentUser = new CurrentUser { UserId = userId, TenantId = tenantId };
			await componentRepository.GetByType(ComponentType.Component);

		}
	}
}