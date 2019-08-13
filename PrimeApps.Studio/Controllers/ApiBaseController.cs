using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Studio.ActionFilters;

namespace PrimeApps.Studio.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer"), CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class ApiBaseController : BaseController
    {
        public static int OrganizationId { get; set; }

        public void SetContext(ActionExecutingContext context)
        {
            OrganizationId = SetOrganization(context);
        }
    }
}