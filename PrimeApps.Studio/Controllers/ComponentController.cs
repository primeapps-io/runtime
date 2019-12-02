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
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Services;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/component")]
    public class ComponentController : DraftBaseController
    {
        private IBackgroundTaskQueue Queue;
        private IDeploymentHelper _deploymentHelper;
        private IModuleRepository _moduleRepository;
        private IComponentRepository _componentRepository;
        private IDeploymentComponentRepository _deploymentComponentRepository;
        private IComponentHelper _componentHelper;
        private IConfiguration _configuration;
        private IPermissionHelper _permissionHelper;

        public ComponentController(IBackgroundTaskQueue queue, IComponentRepository componentRepository, IDeploymentHelper deploymentHelper, IDeploymentComponentRepository deploymentComponentRepository, IComponentHelper componentHelper, IModuleRepository moduleRepository, IConfiguration configuration, IPermissionHelper permissionHelper)
        {
            Queue = queue;
            _deploymentHelper = deploymentHelper;
            _deploymentComponentRepository = deploymentComponentRepository;
            _componentRepository = componentRepository;
            _moduleRepository = moduleRepository;
            _componentHelper = componentHelper;
            _configuration = configuration;
            _permissionHelper = permissionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_componentRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_moduleRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_deploymentComponentRepository, PreviewMode, AppId, TenantId);
            base.OnActionExecuting(context);
        }

        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "component", RequestTypeEnum.View))
                return StatusCode(403);

            var count = await _componentRepository.Count();

            return Ok(count);
        }

        [Route("find")]
        public IActionResult Find(ODataQueryOptions<Component> queryOptions)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "component", RequestTypeEnum.View))
                return StatusCode(403);

            var components = _componentRepository.Find();

            var queryResults = (IQueryable<Component>)queryOptions.ApplyTo(components);
            return Ok(new PageResult<Component>(queryResults, Request.ODataFeature().NextLink, Request.ODataFeature().TotalCount));
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<Component> Get(int id)
        {
            return await _componentRepository.Get(id);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]ComponentModel model)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "component", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var module = await _moduleRepository.GetById(model.ModuleId);

            if (module == null)
                return BadRequest("Module id is not valid.");

            if (module.SystemType != SystemType.Component)
                return BadRequest("Module type is not component");

            var componentName = module.Name.Replace("_", "");
            var component = await _componentRepository.Get(componentName);

            if (component != null)
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

            component = new Component
            {
                Name = componentName,
                Content = model.Content,
                ModuleId = model.ModuleId,
                Type = ComponentType.Component,
                Place = model.Place,
                Order = model.Order,
                Status = PublishStatusType.Draft,
                Label = model.Label,
                Environment = model.EnvironmentValues
            };

            var sampleCreated = await _componentHelper.CreateSample((int)AppId, model, OrganizationId);

            if (!sampleCreated)
                return BadRequest("Component not created.");

            var result = await _componentRepository.Create(component);

            if (result < 0)
                return BadRequest("An error occurred while creating an component");

            return Ok(component.Id);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]ComponentModel model)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "component", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var component = new Component();

            if (model.Place == ComponentPlace.GlobalConfig)
                component = await _componentRepository.GetGlobalConfig();
            else
                component = await _componentRepository.Get(id);

            if (component == null)
                return Forbid("Component not found!");

            if (model.Environments == null)
            {
                model.Environments = new List<EnvironmentType>();
                model.Environments.Add(Model.Enums.EnvironmentType.Development);
            }
            else if (model.Environments.Count < 1)
                model.Environments.Add(EnvironmentType.Development);

            component.Name = model.Name ?? component.Name;
            component.Content = model.Content ?? component.Content;
            component.ModuleId = model.ModuleId != 0 ? model.ModuleId : component.ModuleId;
            component.Type = model.Type != ComponentType.Component ? model.Type : ComponentType.Component;
            component.Place = model.Place != ComponentPlace.NotSet ? model.Place : component.Place;
            component.Order = model.Order != 0 ? model.Order : component.Order;
            component.Status = model.Status;
            component.Label = model.Label;
            component.Environment = model.EnvironmentValues;




            await _componentRepository.Update(component);

            return Ok();
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "component", RequestTypeEnum.Delete))
                return StatusCode(403);

            var component = await _componentRepository.Get(id);

            if (component == null)
                return Forbid("Component not found!");

            await _componentRepository.Delete(component);

            return Ok();
        }

        [Route("all_files_names/{id:int}"), HttpGet]
        public async Task<IActionResult> AllFileNames(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "component", RequestTypeEnum.View))
                return StatusCode(403);

            var component = await _componentRepository.Get(id);

            if (component == null)
                return BadRequest("Component is not exist.");

            var nameList = await _componentHelper.GetAllFileNames((int)AppId, component.Name, OrganizationId);

            return Ok(nameList);
        }

        [Route("deploy/{id:int}"), HttpGet]
        public async Task<IActionResult> Deploy(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "component", RequestTypeEnum.Create))
                return StatusCode(403);

            var component = await _componentRepository.Get(id);

            if (component == null)
                return NotFound("Component is not found.");

            var availableForDeployment = _deploymentComponentRepository.AvailableForDeployment(component.Id);

            if (!availableForDeployment)
                return Conflict("Already have a running deployment");

            var currentBuildNumber = await _deploymentComponentRepository.CurrentBuildNumber(component.Id) + 1;

            var deployment = new DeploymentComponent()
            {
                ComponentId = component.Id,
                Status = ReleaseStatus.Running,
                Version = currentBuildNumber.ToString(),
                BuildNumber = currentBuildNumber,
                StartTime = DateTime.Now
            };

            var result = await _deploymentComponentRepository.Create(deployment);

            if (result < 1)
                return BadRequest("An error occurred while creating an deployment.");

            Queue.QueueBackgroundWorkItem(token => _deploymentHelper.StartComponentDeployment(component, (int)AppId, deployment.Id, OrganizationId));

            return Ok();
        }

        [Route("get_global_config"), HttpGet]
        public async Task<Component> GetGlobalConfig()
        {
            return await _componentRepository.GetGlobalConfig();
        }
    }
}