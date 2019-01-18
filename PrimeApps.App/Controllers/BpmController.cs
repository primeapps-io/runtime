using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Common.Bpm;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Helpers;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using WorkflowCore.Interface;
using System.Linq;

namespace PrimeApps.App.Controllers
{
    [Route("api/bpm"), Authorize]
    public class BpmController : ApiBaseController
    {
        private IBpmRepository _bpmRepository;
        private IWorkflowCoreRepository _workflowCoreRepository;
        private IWorkflowHost _workflowHost;
        private IWorkflowRegistry _workflowRegistry;
        private IPersistenceProvider _workflowStore;
        private IDefinitionLoader _definitionLoader;
        private IConfiguration _configuration;

        private IBpmHelper _bpmHelper;

        public BpmController(IBpmRepository bpmRepository,
            IWorkflowCoreRepository workflowCoreRepository,
            IBpmHelper bpmHelper,
            IWorkflowHost workflowHost,
            IWorkflowRegistry workflowRegistry,
            IPersistenceProvider workflowStore,
            IDefinitionLoader definitionLoader,
            IConfiguration configuration)
        {
            _bpmRepository = bpmRepository;
            _workflowCoreRepository = workflowCoreRepository;
            _bpmHelper = bpmHelper;
            _configuration = configuration;
            _workflowHost = workflowHost;
            _workflowStore = workflowStore;
            _workflowRegistry = workflowRegistry;
            _definitionLoader = definitionLoader;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_bpmRepository, PreviewMode, TenantId, AppId);

            base.OnActionExecuting(context);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var bpmEntity = await _bpmRepository.GetById(id);

            if (bpmEntity == null)
                return NotFound();

            return Ok(bpmEntity);
        }


        [Route("get/{code}"), HttpGet]
        public async Task<IActionResult> Get(string code)
        {
            var bpmEntity = await _bpmRepository.GetByCode(code);

            if (bpmEntity == null)
                return Ok(null);

            return Ok(bpmEntity);
        }

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll(string code = null, int? version = null, bool active = true, bool deleted = false)
        {
            var bpmEntity = await _bpmRepository.GetAll();

            if (bpmEntity.Count < 1)
                return Ok(null);

            return Ok(bpmEntity);
        }

        [Route("find"), HttpGet]
        public async Task<ICollection<BpmWorkflow>> Find([FromBody]BpmFindRequest request)
        {
            var bpmWorkflows = await _bpmRepository.Find(request);

            return bpmWorkflows;
        }

        [Route("count"), HttpPost]
        public async Task<int> Count([FromBody]BpmFindRequest request)
        {
            return await _bpmRepository.Count();
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]BpmWorkflowBindingModel bpmWorkflow)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var version = 1; //Static int value!
            var bpmWorkflowEntity = await _bpmHelper.CreateEntity(bpmWorkflow, AppUser.Language);

            //We must control to be same Workflow Code and version
            var response = await _bpmRepository.GetAll(bpmWorkflowEntity.Code, version);

            if (response.Count() > 0)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            var definitionJson = _bpmHelper.CreateDefinitionNew(bpmWorkflowEntity.Code, version, JObject.Parse(bpmWorkflow.DiagramJson));

            if (definitionJson.IsNullOrEmpty())
                return BadRequest();

            bpmWorkflowEntity.DefinitionJson = definitionJson.ToJsonString();

            //Load string JSON Data on WorkFlowEngine
            var str = JsonConvert.SerializeObject(definitionJson);
            var workflowDefinition = _definitionLoader.LoadDefinition(str);

            if (workflowDefinition == null)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            var result = await _bpmRepository.Create(bpmWorkflowEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            //var referance = _bpmHelper.ReferenceCreateToForBpmHost(AppUser);
            //await _workflowHost.StartWorkflow(bpmWorkflowEntity.Code.ToString(), reference: referance);

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/bpm/get/" + bpmWorkflowEntity.Id, bpmWorkflowEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]BpmWorkflowBindingModel bpmWorkflow)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var bpmWorkflowEntity = await _bpmRepository.GetById(id);

            if (bpmWorkflowEntity == null)
                return NotFound();

            //var bpmRecord = _workflowRegistry.GetDefinition(bpmWorkflowEntity.Code, bpmWorkflowEntity.Version);
            //if (bpmRecord != null)
            //{
            //To increase the last version number of the record we want to update
            var searchResult = await _bpmRepository.GetAll(bpmWorkflowEntity.Code);
            if (searchResult != null && searchResult.Count > 0)
            {
                var lastVersion = searchResult.OrderByDescending(q => q.Version).First().Version;
                var newVersion = lastVersion + 1;

                bpmWorkflowEntity.Version = newVersion;
                var definitionJson = _bpmHelper.CreateDefinitionNew(bpmWorkflowEntity.Code, newVersion, JObject.Parse(bpmWorkflow.DiagramJson));
                bpmWorkflow.DefinitionJson = definitionJson;

            }
            else
            {
                return NotFound();
            }
            //}

            //Load string JSON Data on WorkFlowEngine
            var str = JsonConvert.SerializeObject(bpmWorkflow.DefinitionJson);
            var workflowDefinition = _definitionLoader.LoadDefinition(str);

            if (workflowDefinition == null)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            var currentFiltersIds = bpmWorkflowEntity.Filters.Select(q => q.Id).ToList();
            await _bpmHelper.UpdateEntity(bpmWorkflow, bpmWorkflowEntity, AppUser.TenantLanguage);
            await _bpmRepository.Update(bpmWorkflowEntity, currentFiltersIds);

            return Ok(bpmWorkflowEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var bpmWorkflowEntity = await _bpmRepository.GetById(id);

            if (bpmWorkflowEntity == null)
                return NotFound();

            await _bpmRepository.DeleteSoft(bpmWorkflowEntity);
            var def = _workflowRegistry.GetDefinition(bpmWorkflowEntity.Code, bpmWorkflowEntity.Version);

            //TODO We should be delete from Definition List?

            return Ok();
        }

        [HttpPost("start_workflow/{id}")]
        [HttpPost("start_workflow/{id}/{version}")]
        public async Task<IActionResult> StartWorkflow(string id, int? version, [FromBody]JObject data)
        {
            string workflowId;
            var workflowDefinition = _workflowRegistry.GetDefinition(id, version);
            var referance = _bpmHelper.ReferenceCreateToForBpmHost(AppUser);

            if (workflowDefinition == null)
                return BadRequest(string.Format("Workflow definition {0} for version {1} not found", id, version));

            if (!data.IsNullOrEmpty() && workflowDefinition.DataType != null)
            {
                var dataStr = JsonConvert.SerializeObject(data);
                var dataObj = JsonConvert.DeserializeObject(dataStr, workflowDefinition.DataType);


                workflowId = await _workflowHost.StartWorkflow(id, version, dataObj, referance);
            }
            else
            {
                workflowId = await _workflowHost.StartWorkflow(id, version, null, referance);
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

        [Route("get_workflow_instances/{code}"), HttpGet]
        public IActionResult GetWorkflowInstances(string code)
        {
            var executionPointers = _workflowCoreRepository.GetWorkflowInstances(code);

            return Ok(executionPointers);
        }

        [Route("get_execution_pointers/{workflowInstanceId:int}"), HttpGet]
        public IActionResult GetExecutionPointers(int workflowInstanceId)
        {
            var executionPointers = _workflowCoreRepository.GetExecutionPointers(workflowInstanceId);

            return Ok(executionPointers);
        }
    }
}
