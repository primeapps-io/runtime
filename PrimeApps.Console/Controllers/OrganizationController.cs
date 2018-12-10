using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Organization;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Console.Controllers
{
    [Route("api/organization")]
    public class OrganizationController : ApiBaseOrganizationController
    {
        private IConfiguration _configuration;
        private IOrganizationRepository _organizationRepository;
        private IOrganizationUserRepository _organizationUserRepository;
        private IAppDraftRepository _appDraftRepository;
        private IPlatformUserRepository _platformUserRepository;

        public OrganizationController(IConfiguration configuration, IOrganizationRepository organizationRepository, IOrganizationUserRepository organizationUserRepository, IPlatformUserRepository platformUserRepository, IAppDraftRepository applicationDraftRepository)
        {
            _organizationRepository = organizationRepository;
            _appDraftRepository = applicationDraftRepository;
            _platformUserRepository = platformUserRepository;
            _organizationUserRepository = organizationUserRepository;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContextAsync(context);
            //SetCurrentUser(_platformUserRepository);
            base.OnActionExecuting(context);
        }

        [Route("get"), HttpGet]
        public IActionResult Get()
        {
            var organizations =  _organizationRepository.Get(AppUser.Id, AppUser.OrganizationId);

            return Ok(organizations);
        }

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var organization = await _organizationRepository.GetAll(AppUser.OrganizationId, AppUser.Id);
            var organizationApps = await _appDraftRepository.GetByOrganizationId(AppUser.OrganizationId);

            var organizationDTO = new Organization
            {
                id = organization.Id,
                name = organization.Name,
                icon = organization.Icon,
                ownerId = organization.OwnerId,
                teams = organization.Teams,
                apps = organizationApps,
                users = new List<OrganizationUser>()
            };

            foreach (var user in organization.OrganizationUsers)
            {
                var platformUser = await _platformUserRepository.GetSettings(user.UserId);

                organizationDTO.users.Add(new OrganizationUser { id = user.Id, role = user.Role, email = platformUser.Email, firstName = platformUser.FirstName, lastName = platformUser.LastName, createdAt = platformUser.CreatedAt });
            }

            return Ok(organizationDTO);
        }
    }
}
