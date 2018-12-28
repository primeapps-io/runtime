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
    [Authorize(AuthenticationSchemes = "Bearer"), CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class ApiBaseController : BaseController
    {
        public void SetContext(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues))
                context.Result = new UnauthorizedResult();

            var tenantId = 0;

            if (tenantIdValues.Count == 0 || string.IsNullOrWhiteSpace(tenantIdValues[0]) || !int.TryParse(tenantIdValues[0], out tenantId))
                context.Result = new UnauthorizedResult();

            if (tenantId < 1)
                context.Result = new UnauthorizedResult();

            if (!context.HttpContext.User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(context.HttpContext.User.FindFirst("email").Value))
                context.Result = new UnauthorizedResult();

            var cacheHelper = (ICacheHelper)context.HttpContext.RequestServices.GetService(typeof(ICacheHelper));
            var email = context.HttpContext.User.FindFirst("email").Value;
            var cacheKeyPlatformUser = "platform_user_" + email + "_" + tenantId;
            var platformUser = cacheHelper.Get<PlatformUser>(cacheKeyPlatformUser);

            if (platformUser == null)
            {
                var platformUserRepository = (IPlatformUserRepository)context.HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));
                platformUserRepository.CurrentUser = new CurrentUser { UserId = 1 };

                platformUser = platformUserRepository.GetByEmailAndTenantId(email, tenantId);

                var reuslt = cacheHelper.Set(cacheKeyPlatformUser, platformUser);
            }

            if (platformUser?.TenantsAsUser == null || platformUser.TenantsAsUser.Count < 1)
                context.Result = new UnauthorizedResult();

            context.HttpContext.Items.Add("user", platformUser);
        }
    }
}