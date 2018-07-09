using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;
using System.Linq;

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

            var cacheRepository = (ICacheRepository)HttpContext.RequestServices.GetService(typeof(ICacheRepository));

            string key = typeof(TenantUser).Name + "-" + appUser.Id;
            var tenantUser = cacheRepository.Get<TenantUser>(key);

            if (tenantUser == null)
            {
                var tenantUserRepository = (IUserRepository)HttpContext.RequestServices.GetService(typeof(IUserRepository));

                tenantUserRepository.CurrentUser = new CurrentUser { UserId = appUser.Id, TenantId = appUser.TenantId };
                tenantUser = tenantUserRepository.GetByIdSync(platformUser.Id);

                var result = cacheRepository.Add(key, tenantUser);
            }

            appUser.RoleId = tenantUser.RoleId ?? 0;
            appUser.ProfileId = tenantUser.RoleId ?? 0;
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