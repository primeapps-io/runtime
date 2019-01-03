using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.ActionFilters;
using PrimeApps.Model.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.App.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer"), CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class ApiBaseController : BaseController
    {
        public static int? AppId { get; set; }
        public static int? TenantId { get; set; }
        public static string DBMode { get; set; }

        public void SetContext(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(context.HttpContext.User.FindFirst("email").Value))
                context.Result = new UnauthorizedResult();

            var configuration = (IConfiguration)HttpContext.RequestServices.GetService(typeof(IConfiguration));
            DBMode = configuration.GetSection("AppSettings")["DBMode"];

            var tenantId = 0;
            var appId = 0;

            if (DBMode == "tenant")
            {
                if (!context.HttpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues))
                    context.Result = new UnauthorizedResult();

                if (tenantIdValues.Count == 0 || string.IsNullOrWhiteSpace(tenantIdValues[0]) || !int.TryParse(tenantIdValues[0], out tenantId))
                    context.Result = new UnauthorizedResult();

                if (tenantId < 1)
                    context.Result = new UnauthorizedResult();

                TenantId = tenantId;
            }
            else
            {
                if (!context.HttpContext.Request.Headers.TryGetValue("X-App-Id", out var appIdValues))
                    context.Result = new UnauthorizedResult();

                if (appIdValues.Count == 0 || string.IsNullOrWhiteSpace(appIdValues[0]) || !int.TryParse(appIdValues[0], out appId))
                    context.Result = new UnauthorizedResult();

                if (appId < 1)
                    context.Result = new UnauthorizedResult();

                AppId = appId;
            }

            var cacheHelper = (ICacheHelper)context.HttpContext.RequestServices.GetService(typeof(ICacheHelper));
            var email = context.HttpContext.User.FindFirst("email").Value;
            var cacheKeyPlatformUser = typeof(PlatformUser).Name + "_" + email + "_" + tenantId;
            var platformUser = cacheHelper.Get<PlatformUser>(cacheKeyPlatformUser);

            if (platformUser == null)
            {
                var platformUserRepository = (IPlatformUserRepository)context.HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));
                platformUserRepository.CurrentUser = new CurrentUser { UserId = 1 };

                platformUser = platformUserRepository.GetByEmailAndTenantId(email, tenantId);

                cacheHelper.Set(cacheKeyPlatformUser, platformUser);
            }

            if (platformUser?.TenantsAsUser == null || platformUser.TenantsAsUser.Count < 1)
                context.Result = new UnauthorizedResult();

            context.HttpContext.Items.Add("user", platformUser); 
        }
    }
}