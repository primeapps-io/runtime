using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Console.Helpers;
using PrimeApps.Console.Models;
using PrimeApps.Model.Common.Organization;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace PrimeApps.Console.Controllers
{
    [Route("api/user"), Authorize(AuthenticationSchemes = "Bearer"), ActionFilters.CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class UserController : BaseController
    {
        private IConfiguration _configuration;
        private IPlatformUserRepository _platformUserRepository;
        private IAppDraftRepository _appDraftRepository;
        private IOrganizationRepository _organizationRepository;
        private ITeamRepository _teamRepository;

        public UserController(IConfiguration configuration, IPlatformUserRepository platformUserRepository, IAppDraftRepository appDraftRepository, IOrganizationRepository organizationRepository, ITeamRepository teamRepository)
        {
            _platformUserRepository = platformUserRepository;
            _appDraftRepository = appDraftRepository;
            _organizationRepository = organizationRepository;
            _teamRepository = teamRepository;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContextUser();

            SetCurrentUser(_platformUserRepository);
            SetCurrentUser(_appDraftRepository);
            SetCurrentUser(_organizationRepository);
            SetCurrentUser(_teamRepository);

        }

        [Route("me"), HttpGet]
        public async Task<IActionResult> Me()
        {
            var user = await _platformUserRepository.GetWithSettings(AppUser.Email);

            var me = new ConsoleUser
            {
                Id = AppUser.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.GetFullName(),
                Phone = user.Setting.Phone
            };

            return Ok(me);
        }

        [Route("apps"), HttpPost]
        public async Task<IActionResult> Apps([FromBody]JObject request)
        {
            var search = "";
            var page = 0;
            var status = AppDraftStatus.NotSet;

            if (request != null)
            {
                if (!request["search"].IsNullOrEmpty())
                    search = request["search"].ToString();

                if (!request["page"].IsNullOrEmpty())
                    page = (int)request["page"];

                if (!request["status"].IsNullOrEmpty())
                    status = (AppDraftStatus)int.Parse(request["status"].ToString());
            }

            var apps = await _appDraftRepository.GetAllByUserId(AppUser.Id, search, page, status);

            return Ok(apps);
        }

        [Route("organizations"), HttpGet]
        public async Task<IActionResult> Organizations()
        {
            var organizationUsers = await _organizationRepository.GetByUserId(AppUser.Id);

            if (organizationUsers.Count < 1)
                return Ok(null);

            List<OrganizationModel> organizations = new List<OrganizationModel>();

            foreach (var organizationUser in organizationUsers)
            {
                var organization = new OrganizationModel
                {
                    Id = organizationUser.Organization.Id,
                    Label = organizationUser.Organization.Label,
                    Name = organizationUser.Organization.Name,
                    OwnerId = organizationUser.Organization.OwnerId,
                    Default = organizationUser.Organization.Default,
                    Icon = organizationUser.Organization.Icon,
                    CreatedAt = organizationUser.Organization.CreatedAt,
                    CreatedById = organizationUser.Organization.CreatedById,
                    Role = organizationUser.Role
                };

                organizations.Add(organization);
            }

            return Ok(organizations);
        }

        [Route("edit"), HttpPut]
        public async Task<IActionResult> PlatformUserUpdate([FromBody]PlatformUser user)
        {
            var platformUser = _platformUserRepository.GetByEmail(user.Email);

            platformUser = UserHelper.UpdatePlatformUser(platformUser, user);
            await _platformUserRepository.UpdateAsync(platformUser);

            return Ok(platformUser);
        }
    }
}
