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

namespace PrimeApps.Console.Controllers
{
    [Route("api/app")]
    public class AppController : ApiBaseController
    {
        private IConfiguration _configuration;
        private IPlatformUserRepository _platformUserRepository;
        private IAppDraftRepository _appDraftRepository;
        private IOrganizationRepository _organizationRepository;
        private IPermissionHelper _permissionHelper;

        public AppController(IConfiguration configuration, IPlatformUserRepository platformUserRepository, IAppDraftRepository appDraftRepository, IOrganizationRepository organizationRepository, IPermissionHelper permissionHelper)
        {
            _platformUserRepository = platformUserRepository;
            _appDraftRepository = appDraftRepository;
            _organizationRepository = organizationRepository;
            _configuration = configuration;

            _permissionHelper = permissionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            //SetCurrentUser(_platformUserRepository);
            base.OnActionExecuting(context);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody] AppDraftModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, AppUser.OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var result = await _appDraftRepository.Create(
                new AppDraft
                {
                    Name = model.Name,
                    Label = model.Label,
                    Description = model.Description,
                    Logo = model.Logo,
                    OrganizationId = AppUser.OrganizationId,
                    TempletId = model.TempletId,
                    Status = AppDraftStatus.Draft
                });

            return Ok(result);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] AppDraftModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, AppUser.OrganizationId, OrganizationRole.Administrator))
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

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, AppUser.OrganizationId, OrganizationRole.Administrator))
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
    }
}
