using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models; 

namespace PrimeApps.Studio.Controllers
{
    [Route("api/bpm")]
    public class BpmController : DraftBaseController
    {
        private IBpmRepository _bpmRepository;
        private IWorkflowCoreRepository _workflowCoreRepository;
        //private IWorkflowHost _workflowHost;
        //private IWorkflowRegistry _workflowRegistry;
        //private IPersistenceProvider _workflowStore;
        //private IDefinitionLoader _definitionLoader;
        private IConfiguration _configuration;
        private IPermissionHelper _permissionHelper;
        private IBpmHelper _bpmHelper;

        public BpmController(IConfiguration configuration, IBpmRepository bpmRepository,
            IWorkflowCoreRepository workflowCoreRepository, IBpmHelper bpmHelper, IPermissionHelper permissionHelper)
        {
            _configuration = configuration;
            _bpmRepository = bpmRepository;
            _workflowCoreRepository = workflowCoreRepository;
            //_workflowHost = workflowHost;
            //_definitionLoader = definitionLoader;
            _bpmHelper = bpmHelper;
            _permissionHelper = permissionHelper;
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

        [Route("find"), HttpPost]
        public async Task<ICollection<BpmWorkflow>> Find([FromBody]PaginationModel request)
        {
            var bpmWorkflows = await _bpmRepository.FindForStudio(request);

            return bpmWorkflows;
        }

        [Route("count"), HttpGet]
        public async Task<int> Count()
        {
            return await _bpmRepository.Count();
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]BpmWorkflowBindingModel bpmWorkflow)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "bpm", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var version = 1; //Static int value!
            var bpmWorkflowEntity = await _bpmHelper.CreateEntity(bpmWorkflow, AppUser.Language);

            //We must control to be same Workflow Code and version
            var response = await _bpmRepository.GetAll(bpmWorkflowEntity.Code, version);

            if (response.Count() > 0)
                throw new ApplicationException(HttpStatusCode.InternalServerError.ToString());

            var definitionJson = _bpmHelper.CreateDefinitionNew(bpmWorkflowEntity.Code, version, JObject.Parse(bpmWorkflow.DiagramJson));

            if (definitionJson.IsNullOrEmpty())
                return BadRequest();

            bpmWorkflowEntity.DefinitionJson = definitionJson.ToJsonString();

            //For Runtime
            //Load string JSON Data on WorkFlowEngine
            //var str = JsonConvert.SerializeObject(definitionJson);
            //var workflowDefinition = _definitionLoader.LoadDefinition(str);

            //if (workflowDefinition == null)
            //    throw new ApplicationException(HttpStatusCode.InternalServerError.ToString());

            var result = await _bpmRepository.Create(bpmWorkflowEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.InternalServerError.ToString());

            //var referance = _bpmHelper.ReferenceCreateToForBpmHost(AppUser);
            //await _workflowHost.StartWorkflow(bpmWorkflowEntity.Code.ToString(), reference: referance);

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/bpm/get/" + bpmWorkflowEntity.Id, bpmWorkflowEntity);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]BpmWorkflowBindingModel bpmWorkflow)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "bpm", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var bpmWorkflowEntity = await _bpmRepository.GetById(id);

            if (bpmWorkflowEntity == null)
                return NotFound();

            //var bpmRecord = _workflowRegistry.GetDefinition(bpmWorkflowEntity.Code, bpmWorkflowEntity.Version);
            //if (bpmRecord != null)
            //{
            //To increase the last version number of the record we want to update
            var searchResult = await _bpmRepository.GetAll(bpmWorkflowEntity.Code, active: bpmWorkflowEntity.Active);
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

            //For Runtime
            //////Load string JSON Data on WorkFlowEngine
            ////var str = JsonConvert.SerializeObject(bpmWorkflow.DefinitionJson);
            ////var workflowDefinition = _definitionLoader.LoadDefinition(str);

            ////if (workflowDefinition == null)
            ////    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            var currentFiltersIds = bpmWorkflowEntity.Filters.Select(q => q.Id).ToList();
            await _bpmHelper.UpdateEntity(bpmWorkflow, bpmWorkflowEntity, AppUser.TenantLanguage);
            await _bpmRepository.Update(bpmWorkflowEntity, currentFiltersIds);

            return Ok(bpmWorkflowEntity);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "bpm", RequestTypeEnum.Delete))
                return StatusCode(403);

            var bpmWorkflowEntity = await _bpmRepository.GetById(id);

            if (bpmWorkflowEntity == null)
                return NotFound();

            var result = await _bpmRepository.DeleteSoft(bpmWorkflowEntity);

            //For Runtime
            //var def = _workflowRegistry.GetDefinition(bpmWorkflowEntity.Code, bpmWorkflowEntity.Version);

            //TODO We should be delete from Definition List?

            return Ok(result);
        }
    }
}