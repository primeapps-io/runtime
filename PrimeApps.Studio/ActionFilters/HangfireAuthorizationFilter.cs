using System.Security.Claims;//Don't delete this. It's used in release mode
using System.Threading;//Don't delete this. It's used in release mode
using Hangfire.Dashboard;

namespace PrimeApps.Studio.ActionFilters
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