using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Constants;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/action_button"), Authorize]

    public class ActionButtonController : DraftBaseController
    {
        private IActionButtonRepository _actionButtonRepository;
        private IPermissionHelper _permissionHelper;

        public ActionButtonController(IActionButtonRepository actionButtonRepository, IPermissionHelper permissionHelper)
        {
            _actionButtonRepository = actionButtonRepository;
            _permissionHelper = permissionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_actionButtonRepository, PreviewMode, TenantId, AppId);

            base.OnActionExecuting(context);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> GetActionButtons(int id)
        {
            var buttons = await _actionButtonRepository.GetByModuleId(id);

            if (buttons == null)
                return NotFound();

            return Ok(buttons);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]ActionButtonBindingModel actionbutton)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "action_button", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var actionButtonEntity = ActionButtonHelper.CreateEntity(actionbutton);
            var result = await _actionButtonRepository.Create(actionButtonEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var uri = new Uri(Request.GetDisplayUrl());
            //return Created(uri.Scheme + "://" + uri.Authority + "/api/action_button/get/" + actionButtonEntity.Id, actionButtonEntity);
            return Created(uri.Scheme + "://" + uri.Authority + "/api/action_button/get/" + actionButtonEntity.Id, actionButtonEntity);
        }


        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]ActionButtonBindingModel actionbutton)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "action_button", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var actionButtonEntity = await _actionButtonRepository.GetById(id);

            if (actionButtonEntity == null)
                return NotFound();

            ActionButtonHelper.UpdateEntity(actionbutton, actionButtonEntity);
            await _actionButtonRepository.Update(actionButtonEntity);

            return Ok(actionButtonEntity);
        }


        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "action_button", RequestTypeEnum.Delete))
                return StatusCode(403);

            var actionButtonEntity = await _actionButtonRepository.GetByIdBasic(id);

            if (actionButtonEntity == null)
                return NotFound();

            await _actionButtonRepository.DeleteSoft(actionButtonEntity);

            return Ok();
        }

        [Route("count/{id:int}"), HttpGet]
        public async Task<IActionResult> Count(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "action_button", RequestTypeEnum.Delete))
                return StatusCode(403);

            var count = await _actionButtonRepository.Count(id);

            return Ok(count);
        }

        [Route("find/{id:int}")]
        public async Task<IActionResult> Find(int id, ODataQueryOptions<ActionButton> queryOptions)
        {
            var dependencies = await _actionButtonRepository.Find(id);
            var serializerSettings = new JsonSerializerSettings
            { 
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            var json = JsonConvert.SerializeObject(dependencies, serializerSettings);
            var newData = JsonConvert.DeserializeObject<ICollection<ActionButton>>(json);

            var queryResults = (IQueryable<ActionButton>)queryOptions.ApplyTo(newData.AsQueryable());

            return Ok(new PageResult<ActionButton>(queryResults, Request.ODataFeature().NextLink, Request.ODataFeature().TotalCount));
        }
    }
}