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
        private IDeploymentFunctionRepository _deploymentFunctionRepository;
        private IDeploymentHelper _deploymentHelper;

        public DeploymentComponentController(IBackgroundTaskQueue queue,
            IConfiguration configuration,
            IDeploymentFunctionRepository deploymentFunctionRepository,
            IDeploymentHelper deploymentHelper)
        {
            Queue = queue;
            _configuration = configuration;
            _deploymentFunctionRepository = deploymentFunctionRepository;
            _deploymentHelper = deploymentHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_deploymentFunctionRepository, PreviewMode, AppId, TenantId);
            base.OnActionExecuting(context);
        }

        [Route("count/{id}"), HttpGet]
        public async Task<IActionResult> Count(int id)
        {
            var count = await _deploymentFunctionRepository.Count(id);

            return Ok(count);
        }

        [Route("find/{id}"), HttpPost]
        public async Task<IActionResult> Find(int id, [FromBody]PaginationModel paginationModel)
        {
            var deployments = await _deploymentFunctionRepository.Find(id, paginationModel); ;

            return Ok(deployments);
        }

        [Route("get/{id}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var deployment = await _deploymentFunctionRepository.Get(id);

            if (deployment == null)
                return BadRequest();

            return Ok(deployment);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]DeploymentFunctionBindingModel deployment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentBuildNumber = await _deploymentFunctionRepository.CurrentBuildNumber() + 1;

            var deploymentObj = new DeploymentFunction()
            {
                BuildNumber = currentBuildNumber,
                Version = currentBuildNumber.ToString(),
                StartTime = DateTime.Now,
                Status = DeploymentStatus.Running
            };

            var createResult = await _deploymentFunctionRepository.Create(deploymentObj);

            if (createResult < 0)
                return BadRequest("An error occurred while creating an build.");


            return Ok(deploymentObj);
        }

        [Route("update/{id}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]DeploymentFunctionBindingModel deployment)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var deploymentObj = await _deploymentFunctionRepository.Get(id);

            if (deployment == null)
                return BadRequest("Function deployment not found.");

            deploymentObj.Status = deployment.Status;
            deploymentObj.Version = deployment.Version;
            deploymentObj.StartTime = deployment.StartTime;
            deploymentObj.EndTime = deployment.EndTime;

            var result = await _deploymentFunctionRepository.Update(deploymentObj);

            if (result < 0)
                return BadRequest("An error occurred while update function deployment.");

            return Ok(result);
        }

        [Route("delete/{id}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var function = await _deploymentFunctionRepository.Get(id);

            if (function == null)
                return BadRequest();

            var result = await _deploymentFunctionRepository.Delete(function);

            if (result < 0)
                return BadRequest("An error occurred while deleting an function deployment.");

            return Ok(result);
        }
    }
}
