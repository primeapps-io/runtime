using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Constants;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using Sentry.Protocol;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/app_collaborator"), Authorize]
    public class AppCollaboratorController : DraftBaseController
    {
        private ICollaboratorsRepository _collaboratorRepository;
        private IPermissionHelper _permissionHelper;
        private IGiteaHelper _giteaHelper;
        public AppCollaboratorController(ICollaboratorsRepository collaboratorRepository,
            IPermissionHelper permissionHelper,
            IGiteaHelper giteaHelper)
        {
            _permissionHelper = permissionHelper;
            _collaboratorRepository = collaboratorRepository;
            _giteaHelper = giteaHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_collaboratorRepository);

            base.OnActionExecuting(context);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody] AppCollaborator item)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "app_collaborator", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (item == null)
                return NotFound();

            var result = await _collaboratorRepository.AppCollaboratorAdd(item);

            _giteaHelper.SyncCollaborators(OrganizationId, "create", (int)AppId, item.TeamId, item.UserId);

            return Ok(result);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] AppCollaborator item)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "app_collaborator", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var appCollaborator = await _collaboratorRepository.GetById(id);
            appCollaborator.Profile = item.Profile;

            var result = await _collaboratorRepository.Update(appCollaborator);

            return Ok(result);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "app_collaborator", RequestTypeEnum.Delete))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _permissionHelper.CheckUserRole(AppUser.Id, OrganizationId, OrganizationRole.Administrator))
                return Forbid(ApiResponseMessages.PERMISSION);

            var appCollaborator = await _collaboratorRepository.GetById(id);
            var result = await _collaboratorRepository.Delete(appCollaborator);
            
            _giteaHelper.SyncCollaborators(OrganizationId, "delete", (int)AppId, appCollaborator.TeamId, appCollaborator.UserId);

            return Ok(result);
        }
        
        [Route("get_user_profile"), HttpGet]
        public IActionResult GetUserProfile()
        {
            return Ok(UserProfile);
        }
    }
}