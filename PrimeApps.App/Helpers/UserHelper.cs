using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Http;
using PrimeApps.App.Models;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Context;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Common.Profile;
using System.Linq;

namespace PrimeApps.App.Helpers
{
    public static class UserHelper
    {
        public static CurrentUser GetCurrentUser(IHttpContextAccessor context)
        {

            if (!context.HttpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues))
                return null;

            var tenantId = 0;

            if (tenantIdValues.Count == 0 || string.IsNullOrWhiteSpace(tenantIdValues[0]) || !int.TryParse(tenantIdValues[0], out tenantId))
                return null;

            if (tenantId < 1)
                return null;

            if (!context.HttpContext.User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(context.HttpContext.User.FindFirst("email").Value))
                return null;

            //var cacheRepository = (ICacheRepository)context.HttpContext.RequestServices.GetService(typeof(ICacheRepository));

            string email = context.HttpContext.User.FindFirst("email").Value;

            //string key = typeof(PlatformUser).Name + "-" + email + "-" + tenantId;
            //var platformUser = cacheRepository.Get<PlatformUser>(key);

            //if (platformUser == null)
            //{
            var platformUserRepository = (IPlatformUserRepository)context.HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));
            platformUserRepository.CurrentUser = new CurrentUser { UserId = 1 };
            var platformUser = platformUserRepository.GetByEmailAndTenantId(email, tenantId);

            //var data = cacheRepository.Add(key, platformUser);
            //}
            if (platformUser?.TenantsAsUser == null || platformUser.TenantsAsUser.Count < 1)
                return null;

            return new CurrentUser { TenantId = tenantId, UserId = platformUser.Id };
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