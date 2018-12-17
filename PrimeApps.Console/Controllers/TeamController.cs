using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Console.Constants;
using PrimeApps.Console.Helpers;
using PrimeApps.Model.Common.Team;
using PrimeApps.Model.Entities.Console;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;

namespace PrimeApps.Console.Controllers
{
    [Route("api/team"), Authorize]
    public class TeamController : ApiBaseController
    {
        private IConfiguration _configuration;
        private IOrganizationRepository _organizationRepository;
        private IOrganizationUserRepository _organizationUserRepository;
        private IAppDraftRepository _appDraftRepository;
        private IPlatformUserRepository _platformUserRepository;
        private ITeamRepository _teamRepository;

        private IPermissionHelper _permissionHelper;

        public TeamController(IConfiguration configuration, IOrganizationRepository organizationRepository, IOrganizationUserRepository organizationUserRepository, IPlatformUserRepository platformUserRepository, IAppDraftRepository applicationDraftRepository, ITeamRepository teamRepository, IPermissionHelper permissionHelper)
        {
            _organizationRepository = organizationRepository;
            _appDraftRepository = applicationDraftRepository;
            _platformUserRepository = platformUserRepository;
            _organizationUserRepository = organizationUserRepository;
            _teamRepository = teamRepository;
            _configuration = configuration;

            _permissionHelper = permissionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(context.HttpContext.User.FindFirst("email").Value))
                context.Result = new UnauthorizedResult();

            SetContextUser();
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody] TeamModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, AppUser.OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);
            
            var result = await _teamRepository.Create(new Team { Name = model.Name, OrganizationId = AppUser.OrganizationId, Icon = model.Icon });

            return Ok(result);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] TeamModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, AppUser.OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var team = await _teamRepository.Get(id);
            team.Name = model.Name;
            team.Icon = model.Icon;

            var result = _teamRepository.Update(team);

            return Ok(result);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, AppUser.OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var team = await _teamRepository.Get(id);
            var result = await _teamRepository.Delete(team);

            return Ok(result);
        }

        [Route("get_by_user"), HttpGet]
        public async Task<IActionResult> GetByUserAsync()
        {
            var teams = await _teamRepository.GetByUserId(AppUser.Id);

            return Ok(teams);
        }

    }
}
