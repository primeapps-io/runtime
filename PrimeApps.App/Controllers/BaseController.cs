using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System.Linq;
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

        public void SetCurrentUser(IRepositoryBaseTenant repository, string previewMode, int? tenantId, int? appId)
        {
            if (AppUser != null)
            {
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
                TenantLanguage = tenant.Setting?.Language ?? tenant.App.Setting?.Language ?? "en",
                Email = platformUser.Email,
                FullName = platformUser.FirstName + " " + platformUser.LastName,
                IsIntegrationUser = platformUser.IsIntegrationUser
            };

            if (!tenant.App.UseTenantSettings)
            {
                appUser.Currency = tenant.App.Setting?.Currency;
                appUser.Culture = tenant.App.Setting?.Culture;
                appUser.Language = tenant.App.Setting?.Language ?? "en";
                appUser.TimeZone = tenant.App.Setting?.TimeZone;
            }
            else if (!tenant.UseUserSettings)
            {
                appUser.Currency = tenant.Setting?.Currency;
                appUser.Culture = tenant.Setting?.Culture;
                appUser.Language = tenant.Setting?.Language ?? "en";
                appUser.TimeZone = tenant.Setting?.TimeZone;
            }
            else
            {
                appUser.Currency = platformUser.Setting?.Currency;
                appUser.Culture = platformUser.Setting?.Culture;
                appUser.Language = platformUser.Setting?.Language ?? "en";
                appUser.TimeZone = platformUser.Setting?.TimeZone;
            }

            if (string.IsNullOrEmpty(appUser.Currency))
                appUser.Currency = "USD";

            if (string.IsNullOrEmpty(appUser.Culture))
                appUser.Culture = "en-US";

            if (string.IsNullOrEmpty(appUser.Language))
                appUser.Language = "en";

            if (string.IsNullOrEmpty(appUser.TimeZone))
                appUser.TimeZone = "America/New_York";

            var configuration = (IConfiguration)HttpContext.RequestServices.GetService(typeof(IConfiguration));
            var previewMode = configuration.GetValue("AppSettings:PreviewMode", string.Empty);
            previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

            var tenantUserRepository = (IUserRepository)HttpContext.RequestServices.GetService(typeof(IUserRepository));
            tenantUserRepository.CurrentUser = new CurrentUser { UserId = appUser.Id, TenantId = previewMode == "app" ? appUser.AppId : appUser.TenantId, PreviewMode = previewMode };

            var tenantUser = tenantUserRepository.GetByIdSync(platformUser.Id);

            appUser.RoleId = (int)tenantUser.RoleId;
            appUser.ProfileId = (int)tenantUser.ProfileId;

            appUser.HasAdminProfile = previewMode == "app" || tenantUser.Profile != null && tenantUser.Profile.HasAdminRights;
            appUser.WarehouseDatabaseName = "0";

            return appUser;
        }
    }
}