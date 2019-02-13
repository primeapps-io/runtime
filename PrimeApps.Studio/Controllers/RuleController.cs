using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/rule")]
    public class RuleController : DraftBaseController
    {
        private IConfiguration _configuration;
        private IWorkflowRepository _workflowRepository;
        private IModuleRepository _moduleRepository;
        private IPicklistRepository _picklistRepository;
        private IWorkflowHelper _workflowHelper;

        public RuleController(IConfiguration configuration, IWorkflowRepository workflowRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IWorkflowHelper workflowHelper)
        {
            _configuration = configuration;

            _workflowRepository = workflowRepository;
            _moduleRepository = moduleRepository;
            _picklistRepository = picklistRepository;
            _workflowHelper = workflowHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);

            SetCurrentUser(_workflowRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_moduleRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_picklistRepository, PreviewMode, AppId, TenantId);
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

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var rules = await _workflowRepository.Find(paginationModel);

            return Ok(rules);
        }

        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            var count = await _workflowRepository.Count();

            return Ok(count);
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
                throw new ApplicationException(HttpStatusCode.InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/view/get/" + workflowEntity.Id, workflowEntity);
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

            if (workflow.DeleteLogs)
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