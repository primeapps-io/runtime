using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.ActionFilters;

namespace PrimeApps.Studio.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer"), CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class DraftBaseController : BaseController
    {
        public static ProfileEnum UserProfile { get; set; }
        public static int? AppId { get; set; }
        public static int? TenantId { get; set; }
        public static string PreviewMode { get; set; }
        public static int OrganizationId { get; set; }

        public void SetContext(ActionExecutingContext context)
        {
            OrganizationId = SetOrganization(context);

            if (OrganizationId <= 0)
                context.Result = new UnauthorizedResult();

            TenantId = null;
            AppId = null;

            context.HttpContext.Request.Headers.TryGetValue("X-App-Id", out var appIdValues);
            context.HttpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues);

            var tenantId = 0;
            var appId = 0;

            if (tenantIdValues.Count != 0 && !string.IsNullOrWhiteSpace(tenantIdValues[0]))
                int.TryParse(tenantIdValues[0], out tenantId);

            if (appIdValues.Count != 0 && !string.IsNullOrWhiteSpace(appIdValues[0]))
                int.TryParse(appIdValues[0], out appId);

            if (tenantId == 0 && appId == 0)
                context.Result = new UnauthorizedResult();

            var appDraftRepository = (IAppDraftRepository)context.HttpContext.RequestServices.GetService(typeof(IAppDraftRepository));
            var collaboratorRepository = (ICollaboratorsRepository)context.HttpContext.RequestServices.GetService(typeof(ICollaboratorsRepository));
            var tenantRepository = (ITenantRepository)context.HttpContext.RequestServices.GetService(typeof(ITenantRepository));

            var appIds = appDraftRepository.GetAppIdsByOrganizationId(OrganizationId);

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

                var profiles = collaboratorRepository.GetByUserId(AppUser.Id, OrganizationId, (int)AppId);
                var managerProfile = profiles.FirstOrDefault(x => x.Profile == ProfileEnum.Manager);
                if (managerProfile != null)
                    UserProfile = ProfileEnum.Manager;
                else
                {
                    var developerProfile = profiles.FirstOrDefault(x => x.Profile == ProfileEnum.Developer);
                    if (developerProfile != null)
                        UserProfile = ProfileEnum.Developer;
                    else
                    {
                        var developerTenantAdmin = profiles.FirstOrDefault(x => x.Profile == ProfileEnum.TenantAdmin);
                        UserProfile = developerTenantAdmin != null ? ProfileEnum.TenantAdmin : ProfileEnum.Viewer;
                    }
                }

                PreviewMode = "app";
                AppId = appId;
            }
        }
    }
}