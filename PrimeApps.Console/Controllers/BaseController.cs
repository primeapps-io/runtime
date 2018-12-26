using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Console.Controllers
{
    public class BaseController : Controller
    {
        private UserItem _appUser;

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

        public void SetCurrentUser(IRepositoryBaseTenant repository)
        {
            if (AppUser != null)
            {
                repository.CurrentUser = new CurrentUser { UserId = AppUser.Id, TenantId = AppUser.TenantId };
            }
        }

        public void SetCurrentUser(IRepositoryBaseTenant repository, string DBMode, int? tenantId, int? appId)
        {
            if (AppUser != null)
            {
                repository.CurrentUser = new CurrentUser { UserId = AppUser.Id, TenantId = appId != null ? (int)appId : (int)tenantId, DBMode = DBMode };
            }
        }

        public void SetCurrentUser(IRepositoryBaseConsole repository)
        {
            if (AppUser != null)
            {
                repository.CurrentUser = new CurrentUser { UserId = AppUser.Id };
            }
        }

        public void SetCurrentUser(IRepositoryBasePlatform repository)
        {
            if (AppUser != null)
            {
                repository.CurrentUser = new CurrentUser { UserId = AppUser.Id };
            }
        }

        private UserItem GetUser()
        {
            var platformUser = (PlatformUser)HttpContext.Items["user"];

            var configuration = (IConfiguration)HttpContext.RequestServices.GetService(typeof(IConfiguration));
            var applicationRepository = (IApplicationRepository)HttpContext.RequestServices.GetService(typeof(IApplicationRepository));

            var appInfo = applicationRepository.GetByName(configuration.GetSection("AppSettings")["ClientId"]);

            var appUser = new UserItem
            {
                Id = platformUser.Id,
                Email = platformUser.Email,
                FullName = platformUser.FirstName + " " + platformUser.LastName,
                Currency = platformUser.Setting?.Currency,
                Culture = platformUser.Setting?.Culture,
                Language = platformUser.Setting?.Language,
                TimeZone = platformUser.Setting?.TimeZone
            };

            return appUser;
        }

        public PlatformUser SetContextUser()
        {
            var email = HttpContext.User.FindFirst("email")?.Value;
            var platformUserRepository = (IPlatformUserRepository)HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));

            platformUserRepository.CurrentUser = new CurrentUser { UserId = 1 };

            var platformUser = platformUserRepository.GetByEmail(email);

            HttpContext.Items.Add("user", platformUser);

            return platformUser;
        }

        public void SetOrganization(ActionExecutingContext context)
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