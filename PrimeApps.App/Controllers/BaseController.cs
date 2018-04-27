using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Cache;
using PrimeApps.App.ActionFilters;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    [Authorize, AuthorizeTenant, RequireHttps, ResponseCache(CacheProfileName = "Nocache")]

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
            repository.CurrentUser = new CurrentUser { UserId = _appUser.Id, TenantId = _appUser.TenantId };
        }

        private UserItem GetUser()
        {
            var platformUser = (PlatformUser)HttpContext.Items["user"];
            var tenant = platformUser.TenantsAsUser.Single();

            var appUser = new UserItem
            {
                Id = platformUser.Id,
                AppId = tenant.AppId,
                TenantId = tenant.Id,
                TenantGuid = tenant.GuidId,
                TenantLanguage = tenant.Setting.Language,
                Email = platformUser.Email,
                UserName = platformUser.FirstName + " " + platformUser.LastName,
                Culture = tenant.Setting.Language == "en" ? "en-US" : "tr-TR",
                Currency = platformUser.Currency
            };

            var tenantUserRepository = (IUserRepository)HttpContext.RequestServices.GetService(typeof(IUserRepository));
            var tenantUser = tenantUserRepository.GetByIdSync(platformUser.Id);

            appUser.Role = tenantUser.RoleId ?? 0;
            appUser.ProfileId = tenantUser.ProfileId ?? 0;
            appUser.HasAdminProfile = tenantUser.Profile != null && tenantUser.Profile.HasAdminRights;

            if (tenant.License.AnalyticsLicenseCount > 0)
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
