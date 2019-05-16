using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using PrimeApps.Studio.Services;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/deployment")]
    public class DeploymentController : DraftBaseController
    {
        private IBackgroundTaskQueue Queue;
        private IConfiguration _configuration;
        private IDeploymentRepository _deploymentRepository;
        private IPermissionHelper _permissionHelper;
        private IWebSocketHelper _webSocketHelper;

        public DeploymentController(IBackgroundTaskQueue queue,
            IConfiguration configuration,
            IDeploymentRepository deploymentRepository,
            IPermissionHelper permissionHelper,
            IWebSocketHelper webSocketHelper)
        {
            Queue = queue;
            _configuration = configuration;
            _deploymentRepository = deploymentRepository;
            _permissionHelper = permissionHelper;
            _webSocketHelper = webSocketHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            base.OnActionExecuting(context);
        }
        
        [Route("log/{id}"), HttpGet]
        public async Task<IActionResult> Log(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment", RequestTypeEnum.View))
                return StatusCode(403);

            var deployment = await _deploymentRepository.Get(id);

            if (deployment == null)
                return BadRequest();

            return Ok(deployment);
        }

        [Route("log_stream/{id}"), HttpGet]
        public async Task<IActionResult> LogStream(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment", RequestTypeEnum.View))
                return StatusCode(403);

            var deployment = await _deploymentRepository.Get(id);

            if (deployment == null)
                return BadRequest();

            var context = ControllerContext.HttpContext;
            var isSocketRequest = context.WebSockets.IsWebSocketRequest;

            if (isSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await _webSocketHelper.LogStream(context, webSocket, id);
            }
            else
            {
                context.Response.StatusCode = 400;
            }

            return Ok(deployment);
        }

        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment", RequestTypeEnum.View))
                return StatusCode(403);

            var count = await _deploymentRepository.Count((int)AppId);

            return Ok(count);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment", RequestTypeEnum.View))
                return StatusCode(403);

            var deployments = await _deploymentRepository.Find((int)AppId, paginationModel);

            return Ok(deployments);
        }

        [Route("get/{id}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment", RequestTypeEnum.View))
                return StatusCode(403);

            var deployment = await _deploymentRepository.Get(id);

            if (deployment == null)
                return BadRequest();

            return Ok(deployment);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]DeploymentBindingModel deployment)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment", RequestTypeEnum.Create))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentBuildNumber = await _deploymentRepository.CurrentBuildNumber((int)AppId) + 1;

            var deploymentObj = new Deployment()
            {
                Version = currentBuildNumber.ToString(),
                StartTime = DateTime.Now,
                Status = DeploymentStatus.Running
            };

            var createResult = await _deploymentRepository.Create(deploymentObj);

            if (createResult < 0)
                return BadRequest("An error occurred while creating an build.");


            return Ok(deploymentObj);
        }

        [Route("update/{id}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]DeploymentBindingModel deployment)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var deploymentObj = await _deploymentRepository.Get(id);

            if (deployment == null)
                return BadRequest("Component deployment not found.");

            deploymentObj.Status = deployment.Status;
            deploymentObj.Version = deployment.Version;
            deploymentObj.StartTime = deployment.StartTime;
            deploymentObj.EndTime = deployment.EndTime;

            var result = await _deploymentRepository.Update(deploymentObj);

            if (result < 0)
                return BadRequest("An error occurred while update component deployment.");

            return Ok(result);
        }

        /*[Route("delete/{id}"), HttpDelete]
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
        }*/
    }
}