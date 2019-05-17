using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using PrimeApps.Studio.Services;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/publish")]
    public class PublishController : DraftBaseController
    {
        private IBackgroundTaskQueue _queue;
        private IPublishRepository _publishRepository;
        private IDeploymentRepository _deploymentRepository;
        private IPermissionHelper _permissionHelper;
        private IPublishHelper _publishHelper;

        public PublishController(IPublishHelper publishHelper, IPublishRepository publishRepository, IDeploymentRepository deploymentRepository, IPermissionHelper permissionHelper, IBackgroundTaskQueue queue)
        {
            _publishHelper = publishHelper;
            _deploymentRepository = deploymentRepository;
            _publishRepository = publishRepository;
            _permissionHelper = permissionHelper;
            _queue = queue;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_deploymentRepository);
            base.OnActionExecuting(context);
        }

        [HttpGet]
        [Route("get_last_deployment")]
        public IActionResult GetLastDeployment()
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "publish", RequestTypeEnum.Create))
                return StatusCode(403);

            var result = _publishRepository.GetLastDeployment((int)AppId);

            return Ok(result);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody]PublishCreateBindingModels model)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "publish", RequestTypeEnum.Create))
                return StatusCode(403);

            var deploymentAvailable = _deploymentRepository.AvailableForDeployment((int)AppId);

            if (!deploymentAvailable)
                return Conflict("Already have a running publish.");

            var dbName = PreviewMode + (PreviewMode == "tenant" ? TenantId : AppId);

            var version = await _deploymentRepository.CurrentBuildNumber((int)AppId) + 1;

            var deploymentObj = new Deployment()
            {
                AppId = (int)AppId,
                Version = version.ToString(),
                StartTime = DateTime.Now,
                Status = DeploymentStatus.Running
            };

            await _deploymentRepository.Create(deploymentObj);

            _queue.QueueBackgroundWorkItem(token => _publishHelper.Create((int)AppId, model.ClearAllRecords, dbName, version, deploymentObj.Id));

            return Ok(deploymentObj.Id);
        }
    }
}