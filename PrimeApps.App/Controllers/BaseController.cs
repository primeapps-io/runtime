using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

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
            repository.CurrentUser = new CurrentUser { UserId = AppUser.Id, TenantId = AppUser.TenantId };
        }

        private UserItem GetUser()
        {
            var platformUser = (PlatformUser)HttpContext.Items["user"];
            var userTenant = platformUser.TenantsAsUser.Single();

            var currency = "";
            var culture = "";
            var language = "";
            var timeZone = "";


            if (!userTenant.Tenant.App.UseTenantSettings)
            {
                currency = userTenant.Tenant.App.Setting?.Currency;
                culture = userTenant.Tenant.App.Setting?.Culture;
                language = userTenant.Tenant.App.Setting?.Language;
                timeZone = userTenant.Tenant.App.Setting?.TimeZone;
            }
            else if (!userTenant.Tenant.UseUserSettings)
            {
                currency = userTenant.Tenant.Setting?.Currency;
                culture = userTenant.Tenant.Setting?.Culture;
                language = userTenant.Tenant.Setting?.Language;
                timeZone = userTenant.Tenant.Setting?.TimeZone;
            }
            else
            {
                currency = platformUser.Setting?.Currency;
                culture = platformUser.Setting?.Culture;
                language = platformUser.Setting?.Language;
                timeZone = platformUser.Setting?.TimeZone;
            }

            var appUser = new UserItem
            {
                Id = platformUser.Id,
                AppId = userTenant.Tenant.AppId,
                TenantId = userTenant.Tenant.Id,
                TenantGuid = userTenant.Tenant.GuidId,
                TenantLanguage = userTenant.Tenant.Setting?.Language,
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

            if (userTenant.Tenant.License?.AnalyticsLicenseCount > 0)
            {
                var warehouseRepository = (IPlatformWarehouseRepository)HttpContext.RequestServices.GetService(typeof(IPlatformWarehouseRepository));
                var warehouse = warehouseRepository.GetByTenantIdSync(userTenant.Tenant.Id);

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
