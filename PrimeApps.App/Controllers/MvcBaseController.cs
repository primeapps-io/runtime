using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.ActionFilters;
using PrimeApps.Model.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

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

            var platformUserRepository = (IPlatformUserRepository)context.HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));
            var platformUser = platformUserRepository.GetByEmailAndTenantId(context.HttpContext.User.FindFirst("email").Value, tenantId);

            if (platformUser?.TenantsAsUser == null || platformUser.TenantsAsUser.Count < 1)
                context.Result = new UnauthorizedResult();

            context.HttpContext.Items.Add("user", platformUser);
        }
    }
}
