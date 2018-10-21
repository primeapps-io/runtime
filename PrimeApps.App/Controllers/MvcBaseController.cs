using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.ActionFilters;
using PrimeApps.Model.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;

namespace PrimeApps.App.Controllers
{
    [Authorize, CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class MvcBaseController : BaseController
    {
        public void SetContext(ActionExecutingContext context)
        {
            if (string.IsNullOrWhiteSpace(context.HttpContext.Request.Cookies["tenant_id"]))
                context.Result = new UnauthorizedResult();

            if (!int.TryParse(context.HttpContext.Request.Cookies["tenant_id"], out int tenantId))
                context.Result = new UnauthorizedResult();

            if (tenantId < 1)
                context.Result = new UnauthorizedResult();

            if (!context.HttpContext.User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(context.HttpContext.User.FindFirst("email").Value))
                context.Result = new UnauthorizedResult();

            var cacheService = (IDistributedCache)context.HttpContext.RequestServices.GetService(typeof(IDistributedCache));
            var email = context.HttpContext.User.FindFirst("email").Value;
            var key = "platform_user_" + email + "_" + tenantId;
            var platformUserCache = cacheService.GetString(key);
            PlatformUser platformUser = null;

            if (!string.IsNullOrEmpty(platformUserCache))
                platformUser = JsonConvert.DeserializeObject<PlatformUser>(platformUserCache);

            if (platformUser == null)
            {
                var platformUserRepository = (IPlatformUserRepository)context.HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));
                platformUserRepository.CurrentUser = new CurrentUser { UserId = 1 };

                platformUser = platformUserRepository.GetByEmailAndTenantId(email, tenantId);

                cacheService.SetString(key, JsonConvert.SerializeObject(platformUser, Formatting.Indented, CacheSerializerSettings));
            }

            if (platformUser?.TenantsAsUser == null || platformUser.TenantsAsUser.Count < 1)
                context.Result = new UnauthorizedResult();

            context.HttpContext.Items.Add("user", platformUser);
        }
    }
}