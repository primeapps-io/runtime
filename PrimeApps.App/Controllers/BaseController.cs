using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Cache;
using PrimeApps.App.ActionFilters;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.App.Controllers
{
    [Authorize, AuthorizeTenant, RequireHttps, ResponseCache(CacheProfileName = "Nocache")]

    public class BaseController : Controller
    {
        private UserItem _appUser;

        /// <summary>
        /// Contains basic information related to authorized user.
        /// </summary>
        public UserItem AppUser
        {
            get
            {
                if (_appUser == null && HttpContext.Items?["user"] != null)
                {
                    _appUser = GetUser();
                }

                return _appUser;
            }
        }

        private UserItem GetUser()
        {
            var platformUser = (PlatformUser)HttpContext.Items["user"];
            var tenant = platformUser.TenantsAsUser.Single();

            var appUser = new UserItem
            {
                Id = platformUser.Id,
                AppId = tenant.AppId,
                TenantId = tenant.Id,
                TenantGuid = tenant.GuidId,
                TenantLanguage = tenant.Setting.Language,
                Email = platformUser.Email,
                UserName = platformUser.FirstName + " " + platformUser.LastName,
                Culture = tenant.Setting.Language == "en" ? "en-US" : "tr-TR",
                Currency = platformUser.Currency
            };

            if(tenant.License.)

            return appUser;
        }
    }
}
