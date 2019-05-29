using System;
using System.Net;
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
        private IAppDraftRepository _appDraftRepository;
        private IHistoryDatabaseRepository _historyDatabaseRepository;
        private IHistoryStorageRepository _historyStorageRepository;
        private IPermissionHelper _permissionHelper;
        private IPublishHelper _publishHelper;

        public PublishController(IPublishHelper publishHelper,
            IPublishRepository publishRepository,
            IDeploymentRepository deploymentRepository,
            IAppDraftRepository appDraftRepository,
            IHistoryDatabaseRepository historyDatabaseRepository,
            IHistoryStorageRepository historyStorageRepository,
            IPermissionHelper permissionHelper,
            IBackgroundTaskQueue queue)
        {
            _publishHelper = publishHelper;
            _deploymentRepository = deploymentRepository;
            _publishRepository = publishRepository;
            _historyDatabaseRepository = historyDatabaseRepository;
            _historyStorageRepository = historyStorageRepository;
            _appDraftRepository = appDraftRepository;
            _permissionHelper = permissionHelper;
            _queue = queue;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_deploymentRepository);
            SetCurrentUser(_historyDatabaseRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_historyStorageRepository, PreviewMode, TenantId, AppId);
            base.OnActionExecuting(context);
        }

        [HttpGet]
        [Route("get_last_deployment")]
        public async Task<IActionResult> GetLastDeployment()
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "publish", RequestTypeEnum.Create))
                return StatusCode(403);

            var result = await _publishRepository.GetLastDeployment((int)AppId);

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
                Status = DeploymentStatus.Running,
                Settings = new JObject
                {
                    ["type"] = (int)model.Type,
                    ["clear_all_records"] = model.ClearAllRecords,
                    ["enable_registration"] = model.EnableRegistration
                }.ToString()
            };

            await _deploymentRepository.Create(deploymentObj);

            var dbHistory = await _historyDatabaseRepository.GetLast();
            if (dbHistory != null)
            {
                dbHistory.Tag = deploymentObj.Version;
                await _historyDatabaseRepository.Update(dbHistory);
            }

            var storageHistory = await _historyStorageRepository.GetLast();
            if (storageHistory != null)
            {
                storageHistory.Tag = deploymentObj.Version;
                await _historyStorageRepository.Update(storageHistory);
            }

            var app = await _appDraftRepository.Get((int)AppId);
            app.Setting.EnableRegistration = model.EnableRegistration;
            await _appDraftRepository.Update(app);

            _queue.QueueBackgroundWorkItem(token => _publishHelper.Create((int)AppId, model.ClearAllRecords, dbName, version, deploymentObj.Id));

            return Ok(deploymentObj.Id);
        }
    }
}