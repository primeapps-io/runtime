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

        public ComponentController(IBackgroundTaskQueue queue, IComponentRepository componentRepository, IDeploymentHelper deploymentHelper, IDeploymentComponentRepository deploymentComponentRepository, IComponentHelper componentHelper, IModuleRepository moduleRepository, IConfiguration configuration)
        {
            Queue = queue;
            _deploymentHelper = deploymentHelper;
            _deploymentComponentRepository = deploymentComponentRepository;
            _componentRepository = componentRepository;
            _moduleRepository = moduleRepository;
            _componentHelper = componentHelper;
            _configuration = configuration;
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
            var count = await _componentRepository.Count();

            return Ok(count);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var module = await _moduleRepository.GetById(model.ModuleId);

            if (module == null)
                return BadRequest("Module id is not valid.");

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
                Status = PublishStatus.Draft,
                Label = model.Label
            };

            var result = await _componentRepository.Create(component);

            if (result < 0)
                return BadRequest("An error occurred while creating an component");

            _componentHelper.CreateSample(Request.Cookies["gitea_token"], AppUser.Email, (int)AppId, model);

            return Ok(component.Id);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]ComponentModel model)
        {
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
            var component = await _componentRepository.Get(id);

            if (component == null)
                return Forbid("Component not found!");

            await _componentRepository.Delete(component);

            return Ok();
        }

        [Route("all_files_names/{id:int}"), HttpGet]
        public async Task<IActionResult> AllFileNames(int id)
        {
            var component = await _componentRepository.Get(id);

            if (component == null)
                return BadRequest("Component is not exist.");

            var nameList = await _componentHelper.GetAllFileNames(Request.Cookies["gitea_token"], AppUser.Email, (int)AppId, component.Name);

            return Ok(nameList);
        }

        [Route("deploy/{id:int}"), HttpGet]
        public async Task<IActionResult> Deploy(int id)
        {
            var component = await _componentRepository.Get(id);

            if (component == null)
                return NotFound("Component is not found.");

            var currentBuildNumber = await _deploymentComponentRepository.CurrentBuildNumber(component.Id) + 1;

            var deployment = new DeploymentComponent()
            {
                ComponentId = component.Id,
                Status = DeploymentStatus.Running,
                Version = currentBuildNumber.ToString(),
                BuildNumber = currentBuildNumber,
                StartTime = DateTime.Now
            };

            var result = await _deploymentComponentRepository.Create(deployment);

            if (result < 1)
                return BadRequest("An error occurred while creating an deployment.");

            Queue.QueueBackgroundWorkItem(token => _deploymentHelper.StartComponentDeployment(component, Request.Cookies["gitea_token"], AppUser.Email, (int)AppId, deployment.Id));

            return Ok();
        }
    }
}