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
            var localAddresses = new[] { "127.0.0.1", "::1", context.Request.LocalIpAddress.ToString() };
            
            if (localAddresses.Contains(context.Request.RemoteIpAddress))
                return true;

            var claimsPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;
            var claimsIdentity = (ClaimsIdentity)claimsPrincipal?.Identity;

            if (claimsIdentity == null)
                return false;

            return claimsIdentity.IsAuthenticated;
#endif
        }
    }
}