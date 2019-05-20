using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.ActionFilters;

namespace PrimeApps.Studio.Controllers
{
    [Authorize, CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class MvcBaseController : BaseController
    {
        public static int? AppId { get; set; }
        public static int? TenantId { get; set; }
        public static string PreviewMode { get; set; }

        public void SetContext(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(context.HttpContext.User.FindFirst("email").Value))
                context.Result = new UnauthorizedResult();

            var organizationId = Convert.ToInt32(context.HttpContext.Request.Query["organizationId"].ToString());

            if (organizationId <= 0)
                context.Result = new UnauthorizedResult();

            TenantId = null;
            AppId = null;

            if (!context.HttpContext.Request.Headers.TryGetValue("X-App-Id", out var appIdValues))
                context.HttpContext.Request.Headers.TryGetValue("x-app-id", out appIdValues);

            if (!context.HttpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues))
                context.HttpContext.Request.Headers.TryGetValue("x-tenant-id", out tenantIdValues);

            var tenantId = 0;
            var appId = Convert.ToInt32(context.HttpContext.Request.Query["appId"].ToString());

            if (tenantIdValues.Count != 0 && !string.IsNullOrWhiteSpace(tenantIdValues[0]))
                int.TryParse(tenantIdValues[0], out tenantId);

            if (appIdValues.Count != 0 && !string.IsNullOrWhiteSpace(appIdValues[0]))
                int.TryParse(appIdValues[0], out appId);

            if (tenantId == 0 && appId == 0)
                context.Result = new UnauthorizedResult();

            var appDraftRepository = (IAppDraftRepository)context.HttpContext.RequestServices.GetService(typeof(IAppDraftRepository));
            var tenantRepository = (ITenantRepository)context.HttpContext.RequestServices.GetService(typeof(ITenantRepository));

            var appIds = appDraftRepository.GetAppIdsByOrganizationId(organizationId);

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

            SetContextUser();
        }
    }
}