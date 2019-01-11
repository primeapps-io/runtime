using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Console.Controllers
{
    [Route("api/rule")]
    public class RuleController : DraftBaseController
    {
        private IConfiguration _configuration;
        private IWorkflowRepository _workflowRepository;
        private IModuleRepository _moduleRepository;
        private IPicklistRepository _picklistRepository;

        public RuleController(IConfiguration configuration, IWorkflowRepository workflowRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository)
        {
            _configuration = configuration;

            _workflowRepository = workflowRepository;
            _moduleRepository = moduleRepository;
            _picklistRepository = picklistRepository;
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

            var rules = await _workflowRepository.GetAllBasic();

            rules = rules.Skip(paginationModel.Offset * paginationModel.Limit).Take(paginationModel.Limit).ToList();

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Workflow).GetProperty(paginationModel.OrderColumn);

                if (paginationModel.OrderType == "asc")
                {
                    rules = rules.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                }
                else
                {
                    rules = rules.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
                }

            }

            return Ok(rules);
        }

        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        { 
            var count = await _workflowRepository.Count();

            return Ok(count);
        }
    }
}