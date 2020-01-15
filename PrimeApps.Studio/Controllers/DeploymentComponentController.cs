using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
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
        private IPermissionHelper _permissionHelper;

        public DeploymentComponentController(IBackgroundTaskQueue queue,
            IConfiguration configuration,
            IDeploymentComponentRepository deploymentComponentRepository,
            IDeploymentHelper deploymentHelper, IPermissionHelper permissionHelper)
        {
            Queue = queue;
            _configuration = configuration;
            _deploymentComponentRepository = deploymentComponentRepository;
            _deploymentHelper = deploymentHelper;
            _permissionHelper = permissionHelper;
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
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment_component", RequestTypeEnum.View))
                return StatusCode(403);

            var count = await _deploymentComponentRepository.Count(id);

            return Ok(count);
        }

        [Route("find/{id}")]
        public IActionResult Find(int id, ODataQueryOptions<DeploymentComponent> queryOptions)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment_component", RequestTypeEnum.View))
                return StatusCode(403);

            var deployments = _deploymentComponentRepository.Find(id);

            var queryResults = (IQueryable<DeploymentComponent>)queryOptions.ApplyTo(deployments, new ODataQuerySettings() { EnsureStableOrdering = false });
            return Ok(new PageResult<DeploymentComponent>(queryResults, Request.ODataFeature().NextLink, Request.ODataFeature().TotalCount));
        }

        [Route("get/{id}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment_component", RequestTypeEnum.View))
                return StatusCode(403);

            var deployment = await _deploymentComponentRepository.Get(id);

            if (deployment == null)
                return BadRequest();

            return Ok(deployment);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]DeploymentComponentBindingModel deployment)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment_component", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentBuildNumber = await _deploymentComponentRepository.CurrentBuildNumber(deployment.ComponentId) + 1;

            var deploymentObj = new DeploymentComponent()
            {
                BuildNumber = currentBuildNumber,
                Version = currentBuildNumber.ToString(),
                StartTime = DateTime.Now,
                Status = ReleaseStatus.Running
            };

            var createResult = await _deploymentComponentRepository.Create(deploymentObj);

            if (createResult < 0)
                return BadRequest("An error occurred while creating an build.");


            return Ok(deploymentObj);
        }

        [Route("update/{id}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]DeploymentComponentBindingModel deployment)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment_component", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var deploymentObj = await _deploymentComponentRepository.Get(id);

            if (deployment == null)
                return BadRequest("Component deployment not found.");

            deploymentObj.Status = deployment.Status;
            deploymentObj.Version = deployment.Version;
            deploymentObj.StartTime = deployment.StartTime;
            deploymentObj.EndTime = deployment.EndTime;

            var result = await _deploymentComponentRepository.Update(deploymentObj);

            if (result < 0)
                return BadRequest("An error occurred while update component deployment.");

            return Ok(result);
        }

        [Route("delete/{id}"), HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment_component", RequestTypeEnum.Delete))
                return StatusCode(403);

            var function = await _deploymentComponentRepository.Get(id);

            if (function == null)
                return BadRequest();

            var result = await _deploymentComponentRepository.Delete(function);

            if (result < 0)
                return BadRequest("An error occurred while deleting an component deployment.");

            return Ok(result);
        }
    }
}