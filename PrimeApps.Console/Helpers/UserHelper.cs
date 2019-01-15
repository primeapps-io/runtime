using Microsoft.AspNetCore.Http;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Enums;
using System.Linq;

namespace PrimeApps.Console.Helpers
{
    public static class UserHelper
    {
        public static CurrentUser GetCurrentUser(IHttpContextAccessor context)
        {
            context.HttpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues);
            context.HttpContext.Request.Headers.TryGetValue("X-App-Id", out var appIdValues);
            context.HttpContext.Request.Headers.TryGetValue("X-Organization-Id", out var organizationIdValues);


            var tenantId = 0;
            var appId = 0;
            var userId = 0;
            var organizationId = 0;
            var previewMode = "tenant";

            if (tenantIdValues.Count != 0 && !string.IsNullOrWhiteSpace(tenantIdValues[0]))
                int.TryParse(tenantIdValues[0], out tenantId);

            if (appIdValues.Count != 0 && !string.IsNullOrWhiteSpace(appIdValues[0]))
                int.TryParse(appIdValues[0], out appId);

            if (tenantId < 1 && appId < 1)
                return null;

            if (appId != 0)
            {
                if (organizationIdValues.Count != 0 && !string.IsNullOrWhiteSpace(organizationIdValues[0]))
                    int.TryParse(organizationIdValues[0], out organizationId);

                if (organizationId < 1)
                    return null;
            }

            if (!context.HttpContext.User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(context.HttpContext.User.FindFirst("email").Value))
                return null;

            //var cacheRepository = (ICacheRepository)context.HttpContext.RequestServices.GetService(typeof(ICacheRepository));

            string email = context.HttpContext.User.FindFirst("email").Value;

            //string key = typeof(PlatformUser).Name + "-" + email + "-" + tenantId;
            //var platformUser = cacheRepository.Get<PlatformUser>(key);

            //if (platformUser == null)
            //{
            if (tenantId != 0)
            {
                var platformUserRepository = (IPlatformUserRepository)context.HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));
                platformUserRepository.CurrentUser = new CurrentUser { UserId = 1 };
                var platformUser = platformUserRepository.GetByEmailAndTenantId(email, tenantId);

                //var data = cacheRepository.Add(key, platformUser);
                //}
                if (platformUser?.TenantsAsUser == null || platformUser.TenantsAsUser.Count < 1)
                    return null;

                userId = platformUser.Id;
                context.HttpContext.Items.Add("tenant_id", tenantId);

            }
            else
            {
               // context.HttpContext.Items.Add("app_id", appId);

                previewMode = "app";
                tenantId = appId;
            }

            return new CurrentUser { TenantId = tenantId, UserId = userId, PreviewMode = previewMode };
        }

        public static bool CheckPermission(PermissionEnum operation, int? moduleId, EntityType type, Profile userProfile)
        {
            bool isAllowed = false;
            if (userProfile == null) return false;

            var permission = userProfile.Permissions.Where(x => x.ModuleId == moduleId && x.Type == type).SingleOrDefault();
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
                default:
                    break;
            }

            return isAllowed;
        }
    }
}