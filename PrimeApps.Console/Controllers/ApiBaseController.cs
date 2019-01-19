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
        public static int OrganizationId { get; set; }
        public void SetContext(ActionExecutingContext context)
        {
            OrganizationId = SetOrganization(context);
        }
    }
}
