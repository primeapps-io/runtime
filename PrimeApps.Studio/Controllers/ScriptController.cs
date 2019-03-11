using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public ScriptController(IBackgroundTaskQueue queue, IDeploymentHelper deploymentHelper, IComponentHelper componentHelper,
            IScriptRepository scriptRepository, IConfiguration configuration, IDeploymentComponentRepository deploymentComponentRepository)
        {
            Queue = queue;
            _deploymentHelper = deploymentHelper;
            _componentHelper = componentHelper;
            _scriptRepository = scriptRepository;
            _configuration = configuration;
            _deploymentComponentRepository = deploymentComponentRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_scriptRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_deploymentComponentRepository, PreviewMode, AppId, TenantId);

            base.OnActionExecuting(context);
        }


        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            var count = await _scriptRepository.Count();

            return Ok(count);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            var scripts = await _scriptRepository.Find(paginationModel); ;

            return Ok(scripts);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var script = await _scriptRepository.Get(id);

            if (script == null)
                return BadRequest();

            return Ok(script);
        }

        [Route("get_by_name/{name}"), HttpGet]
        public async Task<IActionResult> GetByName(string name)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var script = await _scriptRepository.GetByName(name);

            if (script == null)
                return BadRequest();

            return Ok(script);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody] ComponentModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var script = new Component
            {
                Name = model.Name,
                Content = model.Content,
                ModuleId = model.ModuleId,
                Type = ComponentType.Script,
                Place = model.Place,
                Order = model.Order,
                Status = PublishStatus.Draft,
                Label = model.Label
            };

            var result = await _scriptRepository.Create(script);

            if (result < 0)
                return BadRequest("An error occurred while creating an script");

            _componentHelper.CreateSampleScript(Request.Cookies["gitea_token"], AppUser.Email, (int)AppId, model);

            return Ok(script.Id);
        }

        [Route("update/{id:int}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]ComponentModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var script = await _scriptRepository.Get(id);

            if (script == null)
                return Forbid("Script not found!");

            script.Content = model.Content ?? script.Content;
            script.ModuleId = model.ModuleId != 0 ? model.ModuleId : script.ModuleId;
            script.Type = ComponentType.Script;
            script.Place = model.Place != ComponentPlace.NotSet ? model.Place : script.Place;
            script.Order = model.Order != 0 ? model.Order : script.Order;
            script.Status = model.Status;
            script.Label = model.Label;

            var result = await _scriptRepository.Update(script);

            if (result < 1)
                return BadRequest("An error occurred while update script.");

            return Ok(result);
        }

        [Route("delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var script = await _scriptRepository.Get(id);

            if (script == null)
                return Forbid("Script not found!");

            await _scriptRepository.Delete(script);

            return Ok();
        }

        [Route("is_unique_name"), HttpGet]
        public async Task<IActionResult> IsUniqueName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest(ModelState);

            var result = await _scriptRepository.IsUniqueName(name);

            return result ? Ok(false) : Ok(true);
        }

        [Route("deploy/{name}")]
        public async Task<IActionResult> Deploy(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest();

            var script = await _scriptRepository.GetByName(name);

            if (script == null)
                return NotFound("Script is not found");


            var currentBuildNumber = await _deploymentComponentRepository.CurrentBuildNumber(script.Id) + 1;

            var deployment = new DeploymentComponent
            {
                ComponentId = script.Id,
                Status = DeploymentStatus.Running,
                Version = currentBuildNumber.ToString(),
                BuildNumber = currentBuildNumber,
                StartTime = DateTime.Now
            };

            var result = await _deploymentComponentRepository.Create(deployment);

            if (result < 1)
                return BadRequest("An error occured while creating an deployment");

            Queue.QueueBackgroundWorkItem(token => _deploymentHelper.StartScriptDeployment(script, Request.Cookies["gitea_token"], AppUser.Email, (int)AppId, deployment.Id));

            return Ok();
        }
    }
}