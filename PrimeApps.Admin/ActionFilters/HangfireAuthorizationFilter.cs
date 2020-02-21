using System.Security.Claims;//Don't delete this. It's used in release mode
using System.Threading;//Don't delete this. It's used in release mode
using Hangfire.Dashboard;

namespace PrimeApps.Admin.ActionFilters
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