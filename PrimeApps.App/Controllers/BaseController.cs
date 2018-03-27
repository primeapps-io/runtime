using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Cache;

namespace PrimeApps.App.Controllers
{
    [Authorize, Microsoft.AspNetCore.Mvc.RequireHttps, ResponseCache(CacheProfileName = "Nocache")] 

    public class BaseController : Controller
    {
        private UserItem _appUser;
        private string _clientId;

        /// <summary>
        /// Contains basic information related to authorized user.
        /// </summary>
        public UserItem AppUser
        {
            get
            {
                if (_appUser == null && User.Identity != null && !string.IsNullOrEmpty(User.Identity.Name))
                {
                    _appUser = GetAppUserSync();
                }

                return _appUser;
            }
        }

        /// <summary>
        /// Client id that user currently authorized with the current access token
        /// </summary>
        public string ClientId
        {
            get
            {
                if (string.IsNullOrEmpty(_clientId))
                {
                    var principal = Request.GetRequestContext().Principal as ClaimsPrincipal;
                    _clientId = principal?.Claims.FirstOrDefault(c => c.Type == "client_id")?.Value;
                }

                return _clientId;
            }
        }

        /// <summary>
        /// Gets the cache object for application user directly from REDIS synchronously.
        /// </summary>
        /// <returns></returns>
        private UserItem GetAppUserSync()
        {
            UserItem result = null;

            if (User.Identity != null && !string.IsNullOrEmpty(User.Identity.Name))
            {
                var userId = AsyncHelpers.RunSync(() => Cache.ApplicationUser.GetId(User.Identity.Name));

                // try to get user object only if user id exists in session cache.
                if (userId != 0)
                    result = AsyncHelpers.RunSync(() => Cache.User.Get(userId));
                else
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized));

                // check token's tenant_id equals user's tenant_id
                var principal = Request.GetRequestContext().Principal as ClaimsPrincipal;
                var tenantId = principal?.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;

                int tokenTenantId;

                if (string.IsNullOrWhiteSpace(tenantId) || !int.TryParse(tenantId, out tokenTenantId))
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized));

                if (result.TenantId != tokenTenantId)
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized));

                var tenant = AsyncHelpers.RunSync(() => Cache.Tenant.Get(result.TenantId));

                if ((tenant.IsDeactivated || tenant.IsSuspended) && result.TenantId == result.Id)
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.PaymentRequired));
            }

            return result;
        }
    }
}
