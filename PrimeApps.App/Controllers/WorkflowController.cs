using System;
using System.Threading.Tasks;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Repositories.Interfaces;
using System.Net;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.ActionFilters;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PrimeApps.App.Controllers
{
    [Route("api/workflow"), Authorize]
    public class WorkflowController : ApiBaseController
    {
        private IWorkflowRepository _workflowRepository;
        private IModuleRepository _moduleRepository;
        private IPicklistRepository _picklistRepository;

	    private IWorkflowHelper _workflowHelper;

        public WorkflowController(IWorkflowRepository workflowRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IWorkflowHelper workflowHelper)
        {
            _workflowRepository = workflowRepository;
            _moduleRepository = moduleRepository;
            _picklistRepository = picklistRepository;
	        _workflowHelper = workflowHelper;
        }

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_workflowRepository);
			SetCurrentUser(_moduleRepository);
			SetCurrentUser(_picklistRepository);

			base.OnActionExecuting(context);
		}

		[Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var workflowEntity = await _workflowRepository.GetById(id);

            if (workflowEntity == null)
                return NotFound();

            if (workflowEntity.SendNotification != null && workflowEntity.SendNotification.RecipientsArray.Length > 0)
                workflowEntity.SendNotification.RecipientList = await _workflowRepository.GetRecipients(workflowEntity);

            if (workflowEntity.SendNotification != null && workflowEntity.SendNotification.CCArray != null && workflowEntity.SendNotification.CCArray.Length > 0)
                workflowEntity.SendNotification.CCList = await _workflowRepository.GetCC(workflowEntity);

            if (workflowEntity.SendNotification != null && workflowEntity.SendNotification.BccArray != null && workflowEntity.SendNotification.BccArray.Length > 0)
                workflowEntity.SendNotification.BccList = await _workflowRepository.GetBcc(workflowEntity);

            if (workflowEntity.CreateTask != null && workflowEntity.CreateTask.Owner > -1)
                workflowEntity.CreateTask.OwnerUser = await _workflowRepository.GetTaskOwner(workflowEntity);

            return Ok(workflowEntity);
        }

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var worflowEntities = await _workflowRepository.GetAllBasic();

            return Ok(worflowEntities);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]WorkflowBindingModel workflow)
        {
            if (workflow.Actions == null || (workflow.Actions.SendNotification == null && workflow.Actions.CreateTask == null && workflow.Actions.FieldUpdate == null && workflow.Actions.WebHook == null))
                ModelState.AddModelError("request._actions", "At least one action required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var workflowEntity = await _workflowHelper.CreateEntity(workflow, AppUser.TenantLanguage);
            var result = await _workflowRepository.Create(workflowEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
			//throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

			var uri = new Uri(Request.GetDisplayUrl());
			return Created(uri.Scheme+ "://" + uri.Authority + "/api/view/get/" + workflowEntity.Id, workflowEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<dynamic> Update(int id, [FromBody]WorkflowBindingModel workflow)
        {
            var workflowEntity = await _workflowRepository.GetById(id);

            if (workflowEntity == null)
                return NotFound();

            var currentFilterIds = workflowEntity.Filters.Select(x => x.Id).ToList();
            await _workflowHelper.UpdateEntity(workflow, workflowEntity, AppUser.TenantLanguage);
            await _workflowRepository.Update(workflowEntity, currentFilterIds);

            await _workflowRepository.DeleteLogs(id);

            return Ok(workflowEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var workflowEntity = await _workflowRepository.GetById(id);

            if (workflowEntity == null)
                return NotFound();

            await _workflowRepository.DeleteSoft(workflowEntity);
            await _workflowRepository.DeleteLogs(id);

            return Ok();
        }
    }
}