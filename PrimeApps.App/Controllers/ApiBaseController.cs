using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.ActionFilters;
using PrimeApps.Model.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Entities.Platform;
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

            //var cacheRepository = (ICacheRepository)context.HttpContext.RequestServices.GetService(typeof(ICacheRepository));
            string email = context.HttpContext.User.FindFirst("email").Value;

            string key = typeof(PlatformUser).Name + "-" + email + "-" + tenantId;
            // var platformUser = cacheRepository.Get<PlatformUser>(key);


            //if (platformUser == null)
            //{
            var platformUserRepository = (IPlatformUserRepository)context.HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));
            platformUserRepository.CurrentUser = new CurrentUser { UserId = 1 };

            var platformUser = platformUserRepository.GetByEmailAndTenantId(email, tenantId);
            // var data = cacheRepository.Add(key, platformUser);
            // }

            if (platformUser?.TenantsAsUser == null || platformUser.TenantsAsUser.Count < 1)
                context.Result = new UnauthorizedResult();


            context.HttpContext.Items.Add("user", platformUser);
        }
    }
}
