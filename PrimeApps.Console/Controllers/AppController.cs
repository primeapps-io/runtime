using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Console.Constants;
using PrimeApps.Console.Helpers;
using PrimeApps.Model.Common.App;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Entities.Console;
using PrimeApps.Console.Services;
using System.Collections.Generic;

namespace PrimeApps.Console.Controllers
{
    [Route("api/app")]
    public class AppController : ApiBaseController
    {
        private IBackgroundTaskQueue Queue;
        private IConfiguration _configuration;
        private IPlatformUserRepository _platformUserRepository;
        private IAppDraftRepository _appDraftRepository;
        private IOrganizationRepository _organizationRepository;
        private IPermissionHelper _permissionHelper;
        private IGiteaHelper _giteaHelper;

        public AppController(IConfiguration configuration,
            IBackgroundTaskQueue queue,
            IPlatformUserRepository platformUserRepository,
            IAppDraftRepository appDraftRepository,
            IOrganizationRepository organizationRepository,
            IPermissionHelper permissionHelper,
            IGiteaHelper giteaHelper)
        {
            Queue = queue;
            _configuration = configuration;
            _platformUserRepository = platformUserRepository;
            _appDraftRepository = appDraftRepository;
            _organizationRepository = organizationRepository;

            _giteaHelper = giteaHelper;
            _permissionHelper = permissionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_platformUserRepository);
            SetCurrentUser(_appDraftRepository);
            SetCurrentUser(_organizationRepository);

            base.OnActionExecuting(context);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var app = await _appDraftRepository.Get(id);

            return Ok(app);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody] AppDraftModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var app = new AppDraft
            {
                Name = model.Name,
                Label = model.Label,
                Description = model.Description,
                Logo = model.Logo,
                OrganizationId = OrganizationId,
                TempletId = model.TempletId,
                Status = AppDraftStatus.Draft,
                Collaborators = new List<AppCollaborator>()
            };

            app.Collaborators.Add(new AppCollaborator { UserId = AppUser.Id, ProfileId = 1 });

            var result = await _appDraftRepository.Create(app);

            if (result < 0)
                return BadRequest("An error occurred while creating an app");

            await Postgres.CreateDatabaseWithTemplet(_configuration.GetConnectionString("TenantDBConnection"), app.Id, model.TempletId);
            Queue.QueueBackgroundWorkItem(token => _giteaHelper.CreateRepository(OrganizationId, model.Name, AppUser, Request.Cookies["gitea_token"]));

            return Ok(app);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] AppDraftModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var app = await _appDraftRepository.Get(id);
            app.Label = model.Label;
            app.Description = model.Description;
            app.Logo = model.Logo;
            app.Status = model.Status;

            var result = _appDraftRepository.Update(app);

            return Ok(result);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var app = await _appDraftRepository.Get(id);
            var result = await _appDraftRepository.Delete(app);

            return Ok(result);
        }

        [Route("get_all"), HttpPost]
        public async Task<IActionResult> Organizations([FromBody] JObject request)
        {
            var search = "";
            var page = 0;
            var status = AppDraftStatus.NotSet;

            if (request != null)
            {
                if (!request["search"].IsNullOrEmpty())
                    search = request["search"].ToString();

                if (request["page"].IsNullOrEmpty())
                    page = (int)request["page"];

                if (!request["status"].IsNullOrEmpty())
                    status = (AppDraftStatus)int.Parse(request["status"].ToString());
            }


            var organizations = await _appDraftRepository.GetAllByUserId(AppUser.Id, search, page, status);

            return Ok(organizations);
        }

        [Route("is_unique_name"), HttpGet]
        public async Task<IActionResult> IsUniqueName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest(ModelState);

            var app = await _appDraftRepository.Get(name);

            return app == null ? Ok(true) : Ok(false);
        }

        [Route("get_collaborators/{id:int}"), HttpGet]
        public async Task<IActionResult> GetAppCollaborators(int id)
        {
            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var app = await _appDraftRepository.GetAppCollaborators(id);

            return Ok(app);
        }
    }
}
