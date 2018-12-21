using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Console.Constants;
using PrimeApps.Console.Helpers;
using PrimeApps.Model.Common.Team;
using PrimeApps.Model.Entities.Console;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Console.Controllers
{
    [Route("api/team")]
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
            SetContext(context);

            SetCurrentUser(_organizationRepository);
            SetCurrentUser(_appDraftRepository);
            SetCurrentUser(_platformUserRepository);
            SetCurrentUser(_organizationUserRepository);
            SetCurrentUser(_teamRepository);

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

            var team = await _teamRepository.GetByTeamId(id);
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

            var team = await _teamRepository.GetByTeamId(id);
            var result = await _teamRepository.Delete(team);

            return Ok(result);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var team = await _teamRepository.GetByTeamId(id);
            var ids = new List<int>();

            foreach (var user in team.TeamUsers)
                ids.Add(user.UserId);

            var JTeam = new JArray();//JObject.FromObject(team);


            var platformUsers = await _platformUserRepository.GetByIds(ids);

            foreach (var user in team.TeamUsers)
            {
                var result = platformUsers.Where(x => x.Id == user.UserId).FirstOrDefault();

                var data = new JObject();
                data["TeamId"] = user.TeamId;
                data["UserId"] = user.UserId;

                data["FirstName"] = result.FirstName;
                data["LastName"] = result.LastName;
                data["Email"] = result.Email;

                JTeam.Add(data);
            }

            return Ok(JTeam);
        }

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var teams = await _teamRepository.GetAll();

            return Ok(teams);
        }

        [Route("get_by_user"), HttpGet]
        public async Task<IActionResult> GetByUserAsync()
        {
            var teams = await _teamRepository.GetByUserId(AppUser.Id);

            return Ok(teams);
        }

        [Route("team_user_add/{id:int}"), HttpPost]
        public async Task<IActionResult> TeamUserAdd(int userId, [FromBody] TeamModel teamModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var team = await _teamRepository.GetByTeamId(teamModel.Id);

            if (team == null)
                return NotFound();

            var teamUser = new TeamUser
            {
                TeamId = team.Id,
                UserId = userId
            };

            var result = await _teamRepository.UserTeamAdd(teamUser);

            return Ok(result);
        }

        [Route("team_user_delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> TeamUserDelete(int userId, [FromBody] TeamModel teamModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var teamUser = await _teamRepository.GetTeamUser(userId, teamModel.Id);

            if (teamUser == null)
                return NotFound();

            var result = await _teamRepository.UserTeamDelete(teamUser);

            return Ok(result);
        }
    }
}
