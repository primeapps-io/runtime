using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Console.ActionFilters;
using PrimeApps.Model.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Console.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer"), CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]

    public class ApiBaseController : BaseController
    {
        public void SetContext(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(context.HttpContext.User.FindFirst("email").Value))
                context.Result = new UnauthorizedResult();

            var platformUser = SetContextUser();

            if (!context.HttpContext.Request.Headers.TryGetValue("X-Organization-Id", out var organizationIdValues))
                context.Result = new UnauthorizedResult();

            var organizationId = 0;

            if (organizationIdValues.Count == 0 || string.IsNullOrWhiteSpace(organizationIdValues[0]) || !int.TryParse(organizationIdValues[0], out organizationId))
                context.Result = new UnauthorizedResult();

            if (organizationId < 1)
                context.Result = new UnauthorizedResult();

            var organizationRepository = (IOrganizationRepository)context.HttpContext.RequestServices.GetService(typeof(IOrganizationRepository));
            var check = organizationRepository.IsOrganizationAvaliable(platformUser.Id, organizationId);

            if (!check)
                context.Result = new UnauthorizedResult();

            context.HttpContext.Items.Add("organization_id", organizationId);
        }
    }
}
