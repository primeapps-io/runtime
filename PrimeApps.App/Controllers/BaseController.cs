using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

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

        public void SetCurrentUser(IRepositoryBaseTenant repository)
        {
            if (AppUser != null)
            {
                repository.CurrentUser = new CurrentUser { UserId = AppUser.Id, TenantId = AppUser.TenantId };
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
                FullName = platformUser.FirstName + " " + platformUser.LastName
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

            var cacheService = (IDistributedCache)HttpContext.RequestServices.GetService(typeof(IDistributedCache));
            var cacheKeyTenantUser = "tenant_user_" + appUser.Id;
            var tenantUserCache = cacheService.GetString(cacheKeyTenantUser);
            TenantUser tenantUser = null;

            if (!string.IsNullOrEmpty(tenantUserCache))
                tenantUser = JsonConvert.DeserializeObject<TenantUser>(tenantUserCache);

            if (tenantUser == null)
            {
                var tenantUserRepository = (IUserRepository)HttpContext.RequestServices.GetService(typeof(IUserRepository));
                tenantUserRepository.CurrentUser = new CurrentUser { UserId = appUser.Id, TenantId = appUser.TenantId };

                tenantUser = tenantUserRepository.GetByIdSync(platformUser.Id);

                cacheService.SetString(cacheKeyTenantUser, JsonConvert.SerializeObject(tenantUser, Formatting.Indented, CacheSerializerSettings));
            }

            appUser.RoleId = tenantUser.RoleId ?? 0;
            appUser.ProfileId = tenantUser.RoleId ?? 0;
            appUser.HasAdminProfile = tenantUser.Profile != null && tenantUser.Profile.HasAdminRights;

            if (tenant.License?.AnalyticsLicenseCount > 0)
            {
                var cacheKeyWarehouse = "platform_warehouse_" + appUser.Id;
                var platformWarehouseCache = cacheService.GetString(cacheKeyWarehouse);
                PlatformWarehouse platformWarehouse = null;

                if (!string.IsNullOrEmpty(tenantUserCache))
                    platformWarehouse = JsonConvert.DeserializeObject<PlatformWarehouse>(platformWarehouseCache);

                if (platformWarehouse == null)
                {
                    var warehouseRepository = (IPlatformWarehouseRepository)HttpContext.RequestServices.GetService(typeof(IPlatformWarehouseRepository));
                    platformWarehouse = warehouseRepository.GetByTenantIdSync(tenant.Id);

                    cacheService.SetString(cacheKeyWarehouse, JsonConvert.SerializeObject(platformWarehouse, Formatting.Indented, CacheSerializerSettings));
                }

                appUser.WarehouseDatabaseName = platformWarehouse.DatabaseName;
            }
            else
            {
                appUser.WarehouseDatabaseName = "0";
            }

            return appUser;
        }
    }
}