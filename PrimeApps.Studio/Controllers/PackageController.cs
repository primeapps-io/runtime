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
    [Route("api/package")]
    public class PackageController : DraftBaseController
    {
        private IConfiguration _configuration;
        private IBackgroundTaskQueue _queue;
        private IPackageRepository _packageRepository;
        private IAppDraftRepository _appDraftRepository;
        private IHistoryDatabaseRepository _historyDatabaseRepository;
        private IHistoryStorageRepository _historyStorageRepository;
        private IPermissionHelper _permissionHelper;
        private IReleaseHelper _releaseHelper;

        public PackageController(IConfiguration configuration,
            IReleaseHelper releaseHelper,
            IPackageRepository packageRepository,
            IAppDraftRepository appDraftRepository,
            IHistoryDatabaseRepository historyDatabaseRepository,
            IHistoryStorageRepository historyStorageRepository,
            IPermissionHelper permissionHelper,
            IBackgroundTaskQueue queue)
        {
            _configuration = configuration;
            _releaseHelper = releaseHelper;
            _packageRepository = packageRepository;
            _historyDatabaseRepository = historyDatabaseRepository;
            _historyStorageRepository = historyStorageRepository;
            _appDraftRepository = appDraftRepository;
            _permissionHelper = permissionHelper;
            _queue = queue;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_packageRepository);
            SetCurrentUser(_historyDatabaseRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_historyStorageRepository, PreviewMode, TenantId, AppId);
            base.OnActionExecuting(context);
        }

        [Route("log/{id}"), HttpGet]
        public async Task<IActionResult> Log(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "package", RequestTypeEnum.View))
                return StatusCode(403);

            var package = await _packageRepository.Get(id);

            if (package == null)
                return BadRequest();

            var dbName = PreviewMode + (PreviewMode == "tenant" ? TenantId : AppId);

            var path = _configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);
            var text = "";

            if (!System.IO.File.Exists($"{path}\\releases\\{dbName}\\{package.Version}\\log.txt"))
                return Ok("Your logs have been deleted...");

            using (var fs = new FileStream($"{path}\\releases\\{dbName}\\{package.Version}\\log.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "package", RequestTypeEnum.View))
                return StatusCode(403);

            var count = await _packageRepository.Count((int)AppId);

            return Ok(count);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "package", RequestTypeEnum.View))
                return StatusCode(403);

            var deployments = await _packageRepository.Find((int)AppId, paginationModel);

            return Ok(deployments);
        }

        [Route("get/{id}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "package", RequestTypeEnum.View))
                return StatusCode(403);

            var deployment = await _packageRepository.Get(id);

            if (deployment == null)
                return BadRequest();

            return Ok(deployment);
        }

        [Route("get_all/{appId}"), HttpGet]
        public async Task<IActionResult> GetAll(int appId)
        {
            if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "package", RequestTypeEnum.View))
                return StatusCode(403);

            var packages = await _packageRepository.GetAll(appId);

            if (packages.Count <= 0)
                return NotFound();

            return Ok(packages);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody]JObject model)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "package", RequestTypeEnum.Create))
                return StatusCode(403);

            var anyProcess = await _packageRepository.GetActiveProcess((int)AppId);

            if (anyProcess != null)
                return Conflict("Already have a running publish.");

            var dbName = PreviewMode + (PreviewMode == "tenant" ? TenantId : AppId);

            var currentBuildNumber = await _packageRepository.Count((int)AppId);
            var version = currentBuildNumber + 1;

            var app = await _appDraftRepository.Get((int)AppId);
            var appOptions = JObject.Parse(app.Setting?.Options);

            var releaseModel = new Package()
            {
                AppId = (int)AppId,
                Version = version.ToString(),
                Revision = version,
                StartTime = DateTime.Now,
                Status = ReleaseStatus.Running,
                Settings = new JObject
                {
                    ["clear_all_records"] = appOptions["clear_all_records"],
                    ["enable_registration"] = appOptions["enable_registration"]
                }.ToString()
            };

            await _packageRepository.Create(releaseModel);

            /*
             * Set version number to history_database and history_storage tables tag column.
             */

            var dbHistory = await _historyDatabaseRepository.GetLast();
            if (dbHistory != null && dbHistory.Tag != (int.Parse(releaseModel.Version) - 1).ToString())
            {
                dbHistory.Tag = releaseModel.Version;
                await _historyDatabaseRepository.Update(dbHistory);
            }

            var storageHistory = await _historyStorageRepository.GetLast();
            if (storageHistory != null && storageHistory.Tag != (int.Parse(releaseModel.Version) - 1).ToString())
            {
                storageHistory.Tag = releaseModel.Version;
                await _historyStorageRepository.Update(storageHistory);
            }

            if (await _releaseHelper.IsFirstRelease(version))
                _queue.QueueBackgroundWorkItem(token => _releaseHelper.All((int)AppId, (bool)appOptions["clear_all_records"], dbName, version, releaseModel.Id));
            else
            {
                List<HistoryDatabase> historyDatabase = null;
                List<HistoryStorage> historyStorages = null;

                if (dbHistory != null && dbHistory.Tag != (int.Parse(releaseModel.Version) - 1).ToString())
                    historyDatabase = await _historyDatabaseRepository.GetDiffs(currentBuildNumber.ToString());

                if (storageHistory != null && storageHistory.Tag != (int.Parse(releaseModel.Version) - 1).ToString())
                    historyStorages = await _historyStorageRepository.GetDiffs(currentBuildNumber.ToString());

                _queue.QueueBackgroundWorkItem(token => _releaseHelper.Diffs(historyDatabase, historyStorages, (int)AppId, dbName, version, releaseModel.Id));
            }

            return Ok(releaseModel.Id);
        }
    }
}