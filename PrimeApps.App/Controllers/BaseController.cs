using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Cache;
using PrimeApps.App.ActionFilters;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using PrimeApps.App.Services;

namespace PrimeApps.App.Controllers
{
	[Authorize(AuthenticationSchemes = "Bearer"), CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]

	public class BaseController : Controller
	{
		private UserItem _appUser;

		public UserItem AppUser
		{
			get
			{
				if ( _appUser == null && HttpContext.Items?["user"] != null)
				{
					_appUser = GetUser();
				}

				return _appUser;
			}
		}

		//public override void OnActionExecuting(ActionExecutingContext context)
		//{
		//	if (!context.HttpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues))
		//		context.Result = new UnauthorizedResult();

		//	var tenantId = 0;

		//	if (string.IsNullOrWhiteSpace(tenantIdValues[0]) || !int.TryParse(tenantIdValues[0], out tenantId))
		//		context.Result = new UnauthorizedResult();

		//	if (tenantId < 1)
		//		context.Result = new UnauthorizedResult();

		//	if (!context.HttpContext.User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(context.HttpContext.User.FindFirst("email").Value))
		//		context.Result = new UnauthorizedResult();

		//	var platformUserRepository = (IPlatformUserRepository)context.HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));
		//	var platformUser = platformUserRepository.GetByEmailAndTenantId(context.HttpContext.User.FindFirst("email").Value, tenantId);

		//	if (platformUser == null || platformUser.TenantsAsUser == null || platformUser.TenantsAsUser.Count < 1)
		//		context.Result = new UnauthorizedResult();

		//	context.HttpContext.Items.Add("user", platformUser);

		//	_httpContext = context.HttpContext;

		//	base.OnActionExecuting(context);
		//}

		public void SetCurrentUser(IRepositoryBaseTenant repository)
		{
			repository.CurrentUser = new CurrentUser { UserId = AppUser.Id, TenantId = AppUser.TenantId };
		}

		private UserItem GetUser()
		{
			var platformUser = (PlatformUser)HttpContext.Items["user"];
			var tenant = platformUser.TenantsAsUser.Single();

			var currency = "";
			var culture = "";
			var language = "";
			var timeZone = "";

			if (!tenant.App.UseTenantSettings)
			{
				currency = tenant.App.Setting.Currency;
				culture = tenant.App.Setting.Culture;
				language = tenant.App.Setting.Language;
				timeZone = tenant.App.Setting.TimeZone;
			}
			else if (!tenant.UseUserSettings)
			{
				currency = tenant.Setting.Currency;
				culture = tenant.Setting.Culture;
				language = tenant.Setting.Language;
				timeZone = tenant.Setting.TimeZone;
			}
			else
			{
				currency = platformUser.Setting.Currency;
				currency = platformUser.Setting.Culture;
				language = platformUser.Setting.Language;
				timeZone = platformUser.Setting.TimeZone;
			}

			var appUser = new UserItem
			{
				Id = platformUser.Id,
				AppId = tenant.AppId,
				TenantId = tenant.Id,
				TenantGuid = tenant.GuidId,
				TenantLanguage = tenant.Setting.Language,
				Language = language,
				Email = platformUser.Email,
				UserName = platformUser.FirstName + " " + platformUser.LastName,
				Culture = culture,
				Currency = currency,
				TimeZone = timeZone
			};

			var tenantUserRepository = (IUserRepository)HttpContext.RequestServices.GetService(typeof(IUserRepository));
			tenantUserRepository.CurrentUser = new CurrentUser { UserId = appUser.Id, TenantId = appUser.TenantId };
			var tenantUser = tenantUserRepository.GetByIdSync(platformUser.Id);

			appUser.Role = tenantUser.RoleId != null ? (int)tenantUser.RoleId : 0;
			appUser.ProfileId = tenantUser.RoleId != null ? (int)tenantUser.RoleId : 0;
			appUser.HasAdminProfile = tenantUser.Profile != null && tenantUser.Profile.HasAdminRights;

			if (tenant.License?.AnalyticsLicenseCount > 0)
			{
				var warehouseRepository = (IPlatformWarehouseRepository)HttpContext.RequestServices.GetService(typeof(IPlatformWarehouseRepository));
				var warehouse = warehouseRepository.GetByTenantIdSync(tenant.Id);

				appUser.WarehouseDatabaseName = warehouse.DatabaseName;
			}
			else
			{
				appUser.WarehouseDatabaseName = "0";
			}

			return appUser;
		}
	}
}
