using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using PrimeApps.Studio.Services;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/deployment_component")]
    public class DeploymentComponentController : DraftBaseController
    {
        private IBackgroundTaskQueue Queue;
        private IConfiguration _configuration;
        private IDeploymentComponentRepository _deploymentComponentRepository;
        private IDeploymentHelper _deploymentHelper;

        public DeploymentComponentController(IBackgroundTaskQueue queue,
            IConfiguration configuration,
            IDeploymentComponentRepository deploymentComponentRepository,
            IDeploymentHelper deploymentHelper)
        {
            Queue = queue;
            _configuration = configuration;
            _deploymentComponentRepository = deploymentComponentRepository;
            _deploymentHelper = deploymentHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_deploymentComponentRepository, PreviewMode, AppId, TenantId);
            base.OnActionExecuting(context);
        }

        [Route("count/{id}"), HttpGet]
        public async Task<IActionResult> Count(int id)
        {
            var count = await _deploymentComponentRepository.Count(id);

            return Ok(count);
        }

        [Route("find/{id}"), HttpPost]
        public async Task<IActionResult> Find(int id, [FromBody]PaginationModel paginationModel)
        {
            var deployments = await _deploymentComponentRepository.Find(id, paginationModel);

            return Ok(deployments);
        }

        [Route("get/{id}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var deployment = await _deploymentComponentRepository.Get(id);

            if (deployment == null)
                return BadRequest();

            return Ok(deployment);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]DeploymentFunctionBindingModel deployment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentBuildNumber = await _deploymentComponentRepository.CurrentBuildNumber() + 1;

            var deploymentObj = new DeploymentComponent()
            {
                BuildNumber = currentBuildNumber,
                Version = currentBuildNumber.ToString(),
                StartTime = DateTime.Now,
                Status = DeploymentStatus.Running
            };

            var createResult = await _deploymentComponentRepository.Create(deploymentObj);

            if (createResult < 0)
                return BadRequest("An error occurred while creating an build.");


            return Ok(deploymentObj);
        }

        [Route("update/{id}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]DeploymentFunctionBindingModel deployment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var deploymentObj = await _deploymentComponentRepository.Get(id);

            if (deployment == null)
                return BadRequest("Function deployment not found.");

            deploymentObj.Status = deployment.Status;
            deploymentObj.Version = deployment.Version;
            deploymentObj.StartTime = deployment.StartTime;
            deploymentObj.EndTime = deployment.EndTime;

            var result = await _deploymentComponentRepository.Update(deploymentObj);

            if (result < 0)
                return BadRequest("An error occurred while update function deployment.");

            return Ok(result);
        }

        [Route("delete/{id}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var function = await _deploymentComponentRepository.Get(id);

            if (function == null)
                return BadRequest();

            var result = await _deploymentComponentRepository.Delete(function);

            if (result < 0)
                return BadRequest("An error occurred while deleting an function deployment.");

            return Ok(result);
        }
    }
}