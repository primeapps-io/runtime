using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Aspose.Words.Lists;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
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
    [Route("api/release")]
    public class ReleaseController : DraftBaseController
    {
        private IConfiguration _configuration;
        private IBackgroundTaskQueue _queue;
        private IReleaseRepository _releaseRepository;
        private IAppDraftRepository _appDraftRepository;
        private IHistoryDatabaseRepository _historyDatabaseRepository;
        private IHistoryStorageRepository _historyStorageRepository;
        private IPermissionHelper _permissionHelper;
        private IReleaseHelper _releaseHelper;

        public ReleaseController(IConfiguration configuration,
            IReleaseHelper releaseHelper,
            IReleaseRepository releaseRepository,
            IAppDraftRepository appDraftRepository,
            IHistoryDatabaseRepository historyDatabaseRepository,
            IHistoryStorageRepository historyStorageRepository,
            IPermissionHelper permissionHelper,
            IBackgroundTaskQueue queue)
        {
            _configuration = configuration;
            _releaseHelper = releaseHelper;
            _releaseRepository = releaseRepository;
            _historyDatabaseRepository = historyDatabaseRepository;
            _historyStorageRepository = historyStorageRepository;
            _appDraftRepository = appDraftRepository;
            _permissionHelper = permissionHelper;
            _queue = queue;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_releaseRepository);
            SetCurrentUser(_historyDatabaseRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_historyStorageRepository, PreviewMode, TenantId, AppId);
            base.OnActionExecuting(context);
        }

        [Route("log/{id}"), HttpGet]
        public async Task<IActionResult> Log(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment", RequestTypeEnum.View))
                return StatusCode(403);

            var deployment = await _releaseRepository.Get(id);

            if (deployment == null)
                return BadRequest();

            var dbName = PreviewMode + (PreviewMode == "tenant" ? TenantId : AppId);

            var path = _configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);
            var text = "";

            if (!System.IO.File.Exists($"{path}\\releases\\{dbName}\\{deployment.Version}\\log.txt"))
                return Ok("Your logs have been deleted...");

            using (var fs = new FileStream($"{path}\\releases\\{dbName}\\{deployment.Version}\\log.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.Default))
            {
                text = ConvertHelper.ASCIIToHTML(sr.ReadToEnd());
                sr.Close();
                fs.Close();
            }

            return Ok(text);
        }

        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment", RequestTypeEnum.View))
                return StatusCode(403);

            var count = await _releaseRepository.Count((int)AppId);

            return Ok(count);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment", RequestTypeEnum.View))
                return StatusCode(403);

            var deployments = await _releaseRepository.Find((int)AppId, paginationModel);

            return Ok(deployments);
        }

        [Route("get/{id}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment", RequestTypeEnum.View))
                return StatusCode(403);

            var deployment = await _releaseRepository.Get(id);

            if (deployment == null)
                return BadRequest();

            return Ok(deployment);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody]JObject model)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "publish", RequestTypeEnum.Create))
                return StatusCode(403);

            var anyProcess = await _releaseRepository.GetActiveProcess((int)AppId);

            if (anyProcess != null)
                return Conflict("Already have a running publish.");

            var dbName = PreviewMode + (PreviewMode == "tenant" ? TenantId : AppId);

            var currentBuildNumber = await _releaseRepository.CurrentBuildNumber((int)AppId);
            var version = currentBuildNumber + 1;

            var app = await _appDraftRepository.Get((int)AppId);
            var appOptions = JObject.Parse(app.Setting?.Options);

            var releaseModel = new Release()
            {
                AppId = (int)AppId,
                Version = version.ToString(),
                StartTime = DateTime.Now,
                Status = ReleaseStatus.Running,
                Settings = new JObject
                {
                    ["clear_all_records"] = appOptions["clear_all_records"],
                    ["enable_registration"] = appOptions["enable_registration"],
                    ["type"] = model["type"]
                }.ToString()
            };

            await _releaseRepository.Create(releaseModel);

            List<HistoryDatabase> historyDatabase = null;
            List<HistoryStorage> historyStorages = null;

            var dbHistory = await _historyDatabaseRepository.GetLast();
            if (dbHistory != null && dbHistory.Tag != (int.Parse(releaseModel.Version) - 1).ToString())
            {
                dbHistory.Tag = releaseModel.Version;
                await _historyDatabaseRepository.Update(dbHistory);

                historyDatabase = await _historyDatabaseRepository.GetDiffs(currentBuildNumber.ToString());
            }

            var storageHistory = await _historyStorageRepository.GetLast();
            if (storageHistory != null && storageHistory.Tag != (int.Parse(releaseModel.Version) - 1).ToString())
            {
                storageHistory.Tag = releaseModel.Version;
                await _historyStorageRepository.Update(storageHistory);

                historyStorages = await _historyStorageRepository.GetDiffs(currentBuildNumber.ToString());
            }

            if (app.Status != PublishStatus.Published)
                _queue.QueueBackgroundWorkItem(token => _releaseHelper.Create((int)AppId, (bool)appOptions["clear_all_records"], model["type"].ToString() == "publish", dbName, version, releaseModel.Id));
            else
            {
                _queue.QueueBackgroundWorkItem(token => _releaseHelper.Update(historyDatabase, historyStorages, (int)AppId, model["type"].ToString() == "publish", dbName, version, releaseModel.Id));
            }

            return Ok(releaseModel.Id);
        }

        [Route("update/{id}"), HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody]DeploymentBindingModel deployment)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "deployment", RequestTypeEnum.Update))
                return StatusCode(403);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var deploymentObj = await _releaseRepository.Get(id);

            if (deployment == null)
                return BadRequest("Component deployment not found.");

            deploymentObj.Status = deployment.Status;
            deploymentObj.Version = deployment.Version;
            deploymentObj.StartTime = deployment.StartTime;
            deploymentObj.EndTime = deployment.EndTime;

            var result = await _releaseRepository.Update(deploymentObj);

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