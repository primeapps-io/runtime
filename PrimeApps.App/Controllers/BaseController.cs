using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.App.Controllers
{
	public class BaseController : Controller
	{
		private UserItem _appUser;

		public UserItem AppUser
		{
			get
			{
				if (_appUser == null && HttpContext.Items?["user"] != null)
				{
					_appUser = GetUser();
				}

				return _appUser;
			}
		}

		public JsonSerializerSettings CacheSerializerSettings => new JsonSerializerSettings
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		};

		public void SetCurrentUser(IRepositoryBaseTenant repository, string previewMode, int? tenantId, int? appId)
		{
			if (AppUser != null)
			{
				if (previewMode == "app")
					repository.CurrentUser = new CurrentUser { UserId = 1, TenantId = appId ?? (tenantId ?? 0), PreviewMode = previewMode };
				else
					repository.CurrentUser = new CurrentUser { UserId = AppUser.Id, TenantId = appId ?? (tenantId ?? 0), PreviewMode = previewMode };
			}
		}

		public void SetCurrentUser(IRepositoryBasePlatform repository)
		{
			if (AppUser != null)
			{
				repository.CurrentUser = new CurrentUser { UserId = AppUser.Id, TenantId = AppUser.TenantId };
			}
		}

		private UserItem GetUser()
		{
			var platformUser = (PlatformUser)HttpContext.Items["user"];
			var tenant = platformUser.TenantsAsUser.Single().Tenant;

			var appUser = new UserItem
			{
				Id = platformUser.Id,
				AppId = tenant.AppId,
				TenantId = tenant.Id,
				TenantGuid = tenant.GuidId,
				TenantLanguage = tenant.Setting?.Language,
				Email = platformUser.Email,
				FullName = platformUser.FirstName + " " + platformUser.LastName,
				IsIntegrationUser = platformUser.IsIntegrationUser
			};

			if (!tenant.App.UseTenantSettings)
			{
				appUser.Currency = tenant.App.Setting?.Currency;
				appUser.Culture = tenant.App.Setting?.Culture;
				appUser.Language = tenant.App.Setting?.Language;
				appUser.TimeZone = tenant.App.Setting?.TimeZone;
			}
			else if (!tenant.UseUserSettings)
			{
				appUser.Currency = tenant.Setting?.Currency;
				appUser.Culture = tenant.Setting?.Culture;
				appUser.Language = tenant.Setting?.Language;
				appUser.TimeZone = tenant.Setting?.TimeZone;
			}
			else
			{
				appUser.Currency = platformUser.Setting?.Currency;
				appUser.Culture = platformUser.Setting?.Culture;
				appUser.Language = platformUser.Setting?.Language;
				appUser.TimeZone = platformUser.Setting?.TimeZone;
			}

			//var cacheHelper = (ICacheHelper) HttpContext.RequestServices.GetService(typeof(ICacheHelper));
			//var cacheKeyTenantUser = "tenant_user_" + appUser.Id;
			//var tenantUser = cacheHelper.Get<TenantUser>(cacheKeyTenantUser);

			var configuration = (IConfiguration)HttpContext.RequestServices.GetService(typeof(IConfiguration));
			var previewMode = configuration.GetValue("AppSettings:PreviewMode", string.Empty);

			//if (tenantUser == null)
			//{
			var tenantUserRepository = (IUserRepository)HttpContext.RequestServices.GetService(typeof(IUserRepository));
			if (!string.IsNullOrEmpty(previewMode))
			{
				tenantUserRepository.CurrentUser = new CurrentUser { UserId = appUser.Id, TenantId = previewMode == "app" ? appUser.AppId : appUser.TenantId, PreviewMode = previewMode };
			}
			var tenantUser = tenantUserRepository.GetByIdSync(platformUser.Id);

			//cacheHelper.Set(cacheKeyTenantUser, tenantUser);
			//}

			appUser.RoleId = previewMode == "app" ? 1 : tenantUser.RoleId ?? 0;
			appUser.ProfileId = previewMode == "app" ? 1 : tenantUser.RoleId ?? 0;
			appUser.HasAdminProfile = previewMode == "app" || tenantUser.Profile != null && tenantUser.Profile.HasAdminRights;

			if (previewMode != "app" && tenant.License?.AnalyticsLicenseCount > 0)
			{
				//var cacheKeyWarehouse = "platform_warehouse_" + appUser.Id;
				//var platformWarehouse = cacheHelper.Get<PlatformWarehouse>(cacheKeyWarehouse);

				//if (platformWarehouse == null)
				//{
				//var warehouseRepository = (IPlatformWarehouseRepository) HttpContext.RequestServices.GetService(typeof(IPlatformWarehouseRepository));
				// var platformWarehouse = warehouseRepository.GetByTenantIdSync(tenant.Id);

				//cacheHelper.Set(cacheKeyWarehouse, platformWarehouse);
				//}

				// appUser.WarehouseDatabaseName = platformWarehouse.DatabaseName;
			}
			else
			{
				appUser.WarehouseDatabaseName = "0";
			}

			return appUser;
		}
	}
}