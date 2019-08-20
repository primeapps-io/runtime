using System;
using System.Threading.Tasks;
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

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "component", RequestTypeEnum.View))
                return StatusCode(403);

            var components = await _componentRepository.Find(paginationModel);

            return Ok(components);
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

            component = new Component
            {
                Name = componentName,
                Content = model.Content,
                ModuleId = model.ModuleId,
                Type = ComponentType.Component,
                Place = model.Place,
                Order = model.Order,
                Status = PublishStatusType.Draft,
                Label = model.Label
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

            var component = await _componentRepository.Get(id);

            if (component == null)
                return Forbid("Component not found!");

            component.Name = model.Name ?? component.Name;
            component.Content = model.Content ?? component.Content;
            component.ModuleId = model.ModuleId != 0 ? model.ModuleId : component.ModuleId;
            component.Type = ComponentType.Component;
            component.Place = model.Place != ComponentPlace.NotSet ? model.Place : component.Place;
            component.Order = model.Order != 0 ? model.Order : component.Order;
            component.Status = model.Status;
            component.Label = model.Label;

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
    }
}