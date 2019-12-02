using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.Component;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Services;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/script")]
    public class ScriptController : DraftBaseController
    {
        private IBackgroundTaskQueue Queue;
        private IDeploymentHelper _deploymentHelper;
        private IComponentHelper _componentHelper;
        private IScriptRepository _scriptRepository;
        private IConfiguration _configuration;
        private IDeploymentComponentRepository _deploymentComponentRepository;
        private IModuleRepository _moduleRepository;
        private IPermissionHelper _permissionHelper;

        public ScriptController(IBackgroundTaskQueue queue, IDeploymentHelper deploymentHelper, IComponentHelper componentHelper,
            IScriptRepository scriptRepository, IConfiguration configuration, IDeploymentComponentRepository deploymentComponentRepository, IModuleRepository moduleRepository, IPermissionHelper permissionHelper)
        {
            Queue = queue;
            _deploymentHelper = deploymentHelper;
            _componentHelper = componentHelper;
            _scriptRepository = scriptRepository;
            _configuration = configuration;
            _deploymentComponentRepository = deploymentComponentRepository;
            _moduleRepository = moduleRepository;
            _permissionHelper = permissionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_scriptRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_deploymentComponentRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_moduleRepository, PreviewMode, AppId, TenantId);

            base.OnActionExecuting(context);
        }


        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "script", RequestTypeEnum.View))
                return StatusCode(403);

            var count = await _scriptRepository.Count();

            return Ok(count);
        }

        [Route("find")]
        public async Task<IActionResult> Find(ODataQueryOptions<Component> queryOptions)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "script", RequestTypeEnum.View))
                return StatusCode(403);

            var scripts = await _scriptRepository.Find();

            var queryResults = (IQueryable<Component>)queryOptions.ApplyTo(scripts.AsQueryable());
            return Ok(new PageResult<Component>(queryResults, Request.ODataFeature().NextLink, Request.ODataFeature().TotalCount));
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "script", RequestTypeEnum.View))
                return StatusCode(403);

            var script = await _scriptRepository.Get(id);

            if (script == null)
                return BadRequest();

            return Ok(script);
        }

        [Route("get_by_name/{name}"), HttpGet]
        public async Task<IActionResult> GetByName(string name)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "script", RequestTypeEnum.View))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var script = await _scriptRepository.GetByName(name);

            if (script == null)
                return BadRequest();

            return Ok(script);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]ComponentModel model)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "script", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var module = await _moduleRepository.GetById(model.ModuleId);

            if (module == null)
                return BadRequest("Module id is not valid.");

            var scriptName = model.Name.Trim().Replace(" ", "-");
            var checkName = await _scriptRepository.IsUniqueName(scriptName);

            if (checkName)
                return Conflict();

            if (model.Environments == null)
            {
                model.Environments = new List<EnvironmentType>()
                {
                    EnvironmentType.Development
                };
            }
            else if (model.Environments.Count < 1)
                model.Environments.Add(EnvironmentType.Development);

            var script = new Component
            {
                Name = scriptName,
                Content = model.Content,
                ModuleId = model.ModuleId,
                Type = ComponentType.Script,
                Place = model.Place,
                Order = model.Order,
                Status = PublishStatusType.Draft,
                Label = model.Label,
                Environment = model.EnvironmentValues,
                CustomUrl = model.CustomUrl
            };

            if (string.IsNullOrEmpty(model.CustomUrl))
            {
                var sampleCreated = await _componentHelper.CreateSampleScript((int)AppId, model, OrganizationId);

                if (!sampleCreated)
                    return BadRequest("Script not created.");
            }

            var result = await _scriptRepository.Create(script);

            if (result < 0)
                return BadRequest("An error occurred while creating an script");

            return Ok(script.Id);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]ComponentModel model)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "script", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var script = await _scriptRepository.Get(id);

            if (script == null)
                return Forbid("Script not found!");


            if (model.Environments == null)
            {
                model.Environments = new List<EnvironmentType>()
                {
                    EnvironmentType.Development
                };
            }
            else if (model.Environments.Count < 1)
                model.Environments.Add(EnvironmentType.Development);

            script.Content = model.Content ?? script.Content;
            script.ModuleId = model.ModuleId != 0 ? model.ModuleId : script.ModuleId;
            script.Type = ComponentType.Script;
            script.Place = model.Place != ComponentPlace.NotSet ? model.Place : script.Place;
            script.Order = model.Order != 0 ? model.Order : script.Order;
            script.Status = model.Status;
            script.Label = model.Label;
            script.Environment = model.EnvironmentValues;
            script.CustomUrl = model.CustomUrl;

            var result = await _scriptRepository.Update(script);

            if (result < 0)
                return BadRequest("An error occurred while update script.");

            return Ok(result);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "script", RequestTypeEnum.Delete))
                return StatusCode(403);

            var script = await _scriptRepository.Get(id);

            if (script == null)
                return Forbid("Script not found!");

            var result = await _scriptRepository.Delete(script);

            if (result < 1)
                return BadRequest("An error occurred while delete script.");

            return Ok(result);
        }

        [Route("is_unique_name"), HttpGet]
        public async Task<IActionResult> IsUniqueName(string name)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "script", RequestTypeEnum.View))
                return StatusCode(403);

            if (string.IsNullOrEmpty(name))
                return BadRequest(ModelState);

            var result = await _scriptRepository.IsUniqueName(name);

            return result ? Ok(false) : Ok(true);
        }

        [Route("deploy/{name}")]
        public async Task<IActionResult> Deploy(string name)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "script", RequestTypeEnum.Create))
                return StatusCode(403);

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest();

            var script = await _scriptRepository.GetByName(name);

            if (script == null)
                return NotFound("Script is not found");

            var availableForDeployment = _deploymentComponentRepository.AvailableForDeployment(script.Id);

            if (!availableForDeployment)
                return Conflict("Already have a running deployment");

            var currentBuildNumber = await _deploymentComponentRepository.CurrentBuildNumber(script.Id) + 1;

            var deployment = new DeploymentComponent
            {
                ComponentId = script.Id,
                Status = ReleaseStatus.Running,
                Version = currentBuildNumber.ToString(),
                BuildNumber = currentBuildNumber,
                StartTime = DateTime.Now
            };

            var result = await _deploymentComponentRepository.Create(deployment);

            if (result < 1)
                return BadRequest("An error occured while creating an deployment");

            Queue.QueueBackgroundWorkItem(token => _deploymentHelper.StartScriptDeployment(script, (int)AppId, deployment.Id, OrganizationId));

            return Ok();
        }
    }
}