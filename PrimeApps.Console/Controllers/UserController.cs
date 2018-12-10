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
    [Route("api/user")]
    public class UserController : ApiBaseController
    {
        private IConfiguration _configuration;
        private IPlatformUserRepository _platformUserRepository;
        private IAppDraftRepository _applicationDraftRepository;
        private IOrganizationRepository _organizationRepository;

        public UserController(IConfiguration configuration, IPlatformUserRepository platformUserRepository, IAppDraftRepository applicationDraftRepository, IOrganizationRepository organizationRepository)
        {
            _platformUserRepository = platformUserRepository;
            _applicationDraftRepository = applicationDraftRepository;
            _organizationRepository = organizationRepository;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            //SetCurrentUser(_platformUserRepository);
            base.OnActionExecuting(context);
        }

        [Route("me"), HttpGet]
        public async Task<IActionResult> Me()
        {
            var user = await _platformUserRepository.GetWithSettings(AppUser.Email);

            var me = new ConsoleUser
            {
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                fullName = user.GetFullName(),
                phone = user.Setting.Phone
            };
            
            
            return Ok(me);
        }

        [Route("organizations"), HttpGet]
        public async Task<IActionResult> Organizations()
        {
            var organizations = await _organizationRepository.GetByUserId(AppUser.Id);

            return Ok(organizations);
        }
    }
}
