using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Common.Bpm;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Helpers;
using WorkflowCore.Interface;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;

namespace PrimeApps.App.Controllers
{
    [Route("api/bpm"), Authorize]
    public class BpmController : ApiBaseController
    {
        private IBpmRepository _bpmRepository;
        private IWorkflowHost _workflowHost;
        private IWorkflowRegistry _workflowRegistry;
        private IPersistenceProvider _workflowStore;
        private IDefinitionLoader _definitionLoader;
        private IConfiguration _configuration;

        public BpmController(IBpmRepository bpmRepository, IWorkflowHost workflowHost, IWorkflowRegistry workflowRegistry, IPersistenceProvider workflowStore, IDefinitionLoader definitionLoader, IConfiguration configuration)
        {
            _bpmRepository = bpmRepository;
            _configuration = configuration;
            _workflowHost = workflowHost;
            _workflowStore = workflowStore;
            _workflowRegistry = workflowRegistry;
            _definitionLoader = definitionLoader;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_bpmRepository);

            base.OnActionExecuting(context);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var bpmEntity = await _bpmRepository.Get(id);

            if (bpmEntity == null)
                return NotFound();

            return Ok(bpmEntity);
        }

        [Route("find"), HttpPost]
        public async Task<ICollection<BpmWorkflow>> Find([FromBody]BpmFindRequest request)
        {
            var bpmWorkflows = await _bpmRepository.Find(request);

            return bpmWorkflows;
        }

        [Route("count"), HttpPost]
        public async Task<int> Count([FromBody]BpmFindRequest request)
        {
            return await _bpmRepository.Count(request);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]BpmWorkflowBindingModel bpmWorkflow) //BpmWorkflowBindingModel bpmWorkflow)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var bpmWorkflowEntity = await BpmHelper.CreateEntity(bpmWorkflow);
            //var result = await _bpmRepository.Create(bpmWorkflowEntity);

            //if (result < 1)
            //    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            var str = bpmWorkflow.DefinitionJson.ToString();
            _definitionLoader.LoadDefinition(str);

            await _workflowHost.StartWorkflow(bpmWorkflow.DefinitionJson["Id"].ToString());

            var uri = new Uri(Request.GetDisplayUrl());
            return null;//Created(uri.Scheme + "://" + uri.Authority + "/api/bpm/get/" + bpmWorkflowEntity.Id, bpmWorkflowEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]BpmWorkflowBindingModel bpmWorkflow)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var bpmWorkflowEntity = await _bpmRepository.Get(id);

            if (bpmWorkflowEntity == null)
                return NotFound();

            await BpmHelper.UpdateEntity(bpmWorkflow, bpmWorkflowEntity);
            await _bpmRepository.Update(bpmWorkflowEntity);

            return Ok(bpmWorkflowEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var bpmWorkflowEntity = await _bpmRepository.Get(id);

            if (bpmWorkflowEntity == null)
                return NotFound();

            await _bpmRepository.DeleteSoft(bpmWorkflowEntity);

            return Ok();
        }

        [HttpPost("start_workflow/{id}")]
        [HttpPost("start_workflow/{id}/{version}")]
        public async Task<IActionResult> StartWorkflow(string id, int? version, [FromBody]JObject data)
        {
            string workflowId;
            var workflowDefinition = _workflowRegistry.GetDefinition(id, version);

            if (workflowDefinition == null)
                return BadRequest(string.Format("Workflow definition {0} for version {1} not found", id, version));

            if (!data.IsNullOrEmpty() && workflowDefinition.DataType != null)
            {
                var dataStr = JsonConvert.SerializeObject(data);
                var dataObj = JsonConvert.DeserializeObject(dataStr, workflowDefinition.DataType);
                workflowId = await _workflowHost.StartWorkflow(id, version, dataObj, AppUser.TenantId.ToString());
            }
            else
            {
                workflowId = await _workflowHost.StartWorkflow(id, version, null, AppUser.TenantId.ToString());
            }

            return Ok(workflowId);
        }

        [HttpPut("suspend_worflow/{id}")]
        public Task<bool> SuspendWorkflow(string id)
        {
            return _workflowHost.SuspendWorkflow(id);
        }

        [HttpPut("resume_worflow/{id}")]
        public Task<bool> ResumeWorkflow(string id)
        {
            return _workflowHost.ResumeWorkflow(id);
        }

        [HttpDelete("terminate_worflow/{id}")]
        public Task<bool> TerminateWorkflow(string id)
        {
            return _workflowHost.TerminateWorkflow(id);
        }

        [HttpPost("publish_event/{eventName}/{eventKey}")]
        public async Task<IActionResult> PublishEvent(string eventName, string eventKey, [FromBody]JObject eventData)
        {
            await _workflowHost.PublishEvent(eventName, eventKey, eventData);

            return Ok();
        }
    }
}
