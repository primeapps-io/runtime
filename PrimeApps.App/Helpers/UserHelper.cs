using Microsoft.AspNetCore.Http;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Entities.Platform;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.App.Helpers
{
    public static class UserHelper
    {
        public static CurrentUser GetCurrentUser(IHttpContextAccessor context, IConfiguration configuration)
        {
            var previewMode = configuration.GetValue("AppSettings:PreviewMode", string.Empty);
            previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

            var tenantId = 0;
            var appId = 0;

            if (previewMode == "tenant")
            {
                if (!context.HttpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues))
                    if (!context.HttpContext.Request.Headers.TryGetValue("x-tenant-id", out tenantIdValues))
                        return null;

                if (tenantIdValues.Count == 0 || string.IsNullOrWhiteSpace(tenantIdValues[0]) ||
                    !int.TryParse(tenantIdValues[0], out tenantId))
                    return null;

                if (tenantId < 1)
                    return null;

                if (!context.HttpContext.User.Identity.IsAuthenticated ||
                    string.IsNullOrWhiteSpace(context.HttpContext.User.FindFirst("email").Value))
                    return null;
            }
            else
            {
                if (!context.HttpContext.Request.Headers.TryGetValue("X-App-Id", out var appIdValues))
                    if (!context.HttpContext.Request.Headers.TryGetValue("x-app-id", out appIdValues))
                        return null;

                if (appIdValues.Count == 0 || string.IsNullOrWhiteSpace(appIdValues[0]) ||
                    !int.TryParse(appIdValues[0], out appId))
                    return null;

                if (appId < 1)
                    return null;
            }
            //var cacheRepository = (ICacheRepository)context.HttpContext.RequestServices.GetService(typeof(ICacheRepository));

            string email = context.HttpContext.User.FindFirst("email").Value;

            //string key = typeof(PlatformUser).Name + "-" + email + "-" + tenantId;
            //var platformUser = cacheRepository.Get<PlatformUser>(key);

            //if (platformUser == null)
            //{
            var platformUserRepository =
                (IPlatformUserRepository)context.HttpContext.RequestServices.GetService(
                    typeof(IPlatformUserRepository));
            platformUserRepository.CurrentUser = new CurrentUser { UserId = 1 };
            PlatformUser platformUser;

            if (appId > 0)
            {
                platformUser = platformUserRepository.GetByEmailAndTenantId(email, 1);
                platformUser.TenantsAsUser.Single().Tenant.AppId = appId;
            }
            else
                platformUser = platformUserRepository.GetByEmailAndTenantId(email, tenantId);

            //var data = cacheRepository.Add(key, platformUser);
            //}
            if (platformUser?.TenantsAsUser == null || platformUser.TenantsAsUser.Count < 1)
                return null;

            return new CurrentUser
            {
                TenantId = previewMode == "app" ? appId : tenantId, UserId = platformUser.Id, PreviewMode = previewMode
            };
        }

        public static bool CheckPermission(PermissionEnum operation, int? moduleId, EntityType type,
            Profile userProfile)
        {
            var isAllowed = false;

            var permission = userProfile?.Permissions.FirstOrDefault(x => x.ModuleId == moduleId && x.Type == type);
            if (permission == null) return false;

            switch (operation)
            {
                case PermissionEnum.Write:
                    isAllowed = permission.Write;
                    break;
                case PermissionEnum.Read:
                    isAllowed = permission.Read;
                    break;
                case PermissionEnum.Remove:
                    isAllowed = permission.Remove;
                    break;
                case PermissionEnum.Modify:
                    isAllowed = permission.Modify;
                    break;
            }

            return isAllowed;
        }
    }
}