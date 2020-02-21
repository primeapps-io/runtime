using System.Linq;
using System.Security.Claims;
using System.Threading;
using Hangfire.Dashboard;

namespace PrimeApps.App.ActionFilters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
#if DEBUG
            return true;

#else
            var httpContext = context.GetHttpContext();

            return httpContext != null && httpContext.User.Identity.IsAuthenticated;
#endif
        }
    }
}