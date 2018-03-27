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
            var claimsPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;
            var claimsIdentity = (ClaimsIdentity)claimsPrincipal?.Identity;

            if (claimsIdentity == null)
                return false;

            return claimsIdentity.IsAuthenticated && claimsIdentity.Name.EndsWith("@ofisim.com");
#endif
        }
    }
}