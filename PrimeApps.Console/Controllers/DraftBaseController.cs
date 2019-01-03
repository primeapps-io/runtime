using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Console.ActionFilters;
using PrimeApps.Model.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Console.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer"), CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]


    public class DraftBaseController : BaseController
    {
        public static int? AppId { get; set; }
        public static int? TenantId { get; set; }
        public static string PreviewMode { get; set; }

        public void SetContext(ActionExecutingContext context)
        {
            SetOrganization(context);

            TenantId = null;
            AppId = null;

            context.HttpContext.Request.Headers.TryGetValue("X-App-Id", out var appIdValues);
            context.HttpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues);

            var tenantId = 0;
            var appId = 0;

            var organizationId = context.HttpContext.Items["organization_id"];

            if (organizationId == null)
                context.Result = new UnauthorizedResult();

            if (tenantIdValues.Count != 0 && !string.IsNullOrWhiteSpace(tenantIdValues[0]))
                int.TryParse(tenantIdValues[0], out tenantId);

            if (appIdValues.Count != 0 && !string.IsNullOrWhiteSpace(appIdValues[0]))
                int.TryParse(appIdValues[0], out appId);

            if (tenantId == 0 && appId == 0)
                context.Result = new UnauthorizedResult();

            var appDraftRepository = (IAppDraftRepository)context.HttpContext.RequestServices.GetService(typeof(IAppDraftRepository));
            var tenantRepository = (ITenantRepository)context.HttpContext.RequestServices.GetService(typeof(ITenantRepository));

            var appIds = appDraftRepository.GetAppIdsByOrganizationId((int)organizationId);

            if (tenantId != 0)
            {
                var tenant = tenantRepository.Get(tenantId);

                if (!appIds.Contains(tenant.AppId))
                    context.Result = new UnauthorizedResult();
                else
                    context.HttpContext.Items.Add("tenant_id", tenantId);

                PreviewMode = "tenant";
                TenantId = tenantId;
            }
            else
            {
                if (!appIds.Contains(appId))
                    context.Result = new UnauthorizedResult();

                else
                    context.HttpContext.Items.Add("app_id", appId);

                PreviewMode = "app";
                AppId = appId;
            }
        }
    }
}
