using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Services;
using PrimeApps.Model.Storage;
using Microsoft.AspNet.OData.Query;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;

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
		private IPackageHelper _packageHelper;
		private IUnifiedStorage _storage;
		private IHostingEnvironment _hostingEnvironment;

		public PackageController(IConfiguration configuration,
			IPackageHelper packageHelper,
			IPackageRepository packageRepository,
			IAppDraftRepository appDraftRepository,
			IHistoryDatabaseRepository historyDatabaseRepository,
			IHistoryStorageRepository historyStorageRepository,
			IPermissionHelper permissionHelper,
			IUnifiedStorage storage,
			IBackgroundTaskQueue queue,
			IHostingEnvironment hostingEnvironment)
		{
			_configuration = configuration;
			_packageHelper = packageHelper;
			_packageRepository = packageRepository;
			_historyDatabaseRepository = historyDatabaseRepository;
			_historyStorageRepository = historyStorageRepository;
			_appDraftRepository = appDraftRepository;
			_permissionHelper = permissionHelper;
			_storage = storage;
			_queue = queue;
			_hostingEnvironment = hostingEnvironment;
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

			var path = DataHelper.GetDataDirectoryPath(_configuration, _hostingEnvironment);
			var text = "";

			if (!System.IO.File.Exists(Path.Combine(path, "packages", dbName, package.Version, "log.txt")))
				return Ok("Your logs have been deleted...");

			using (var fs = new FileStream(Path.Combine(path, "packages", dbName, package.Version, "log.txt"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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

		[Route("find")]
		public IActionResult Find(ODataQueryOptions<Package> queryOptions)
		{
			if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "package", RequestTypeEnum.View))
				return StatusCode(403);

			var packages = _packageRepository.Find((int)AppId);

			var queryResults = (IQueryable<Package>)queryOptions.ApplyTo(packages, new ODataQuerySettings() { EnsureStableOrdering = false });
			return Ok(new PageResult<Package>(queryResults, Request.ODataFeature().NextLink, Request.ODataFeature().TotalCount));
		}

		[Route("get/{id}"), HttpGet]
		public async Task<IActionResult> Get(int id)
		{
			if (UserProfile != ProfileEnum.Manager && !_permissionHelper.CheckUserProfile(UserProfile, "package", RequestTypeEnum.View))
				return StatusCode(403);

			var package = await _packageRepository.Get(id);

			if (package == null)
				return BadRequest();

			return Ok(package);
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
				return Conflict("A packaging process is currently in progress.");

			var dbHistory = await _historyDatabaseRepository.GetLast();

			if (dbHistory?.Tag != null)
				return Conflict("Please modify your app to create a new package.");

			var dbName = PreviewMode + (PreviewMode == "tenant" ? TenantId : AppId);

			var currentBuildNumber = await _packageRepository.Count((int)AppId);
			var version = currentBuildNumber + 1;

			var app = await _appDraftRepository.Get((int)AppId);
			var appOptions = new JObject
			{
				["enable_ldap"] = false,
				["enable_registration"] = true,
				["enable_api_registration"] = true,
				["protect_modules"] = model["protectModules"],
				["selected_modules"] = model["selected_modules"]
			};

			if (app.Setting != null && !string.IsNullOrEmpty(app.Setting.Options))
			{
				appOptions = JObject.Parse(app.Setting.Options);
				appOptions["protect_modules"] = model["protectModules"];
				appOptions["selected_modules"] = model["selected_modules"];
				app.Setting.Options = appOptions.ToString();
				await _appDraftRepository.Update(app);
			}

			var packageModel = new Package()
			{
				AppId = (int)AppId,
				Version = version.ToString(),
				Revision = version,
				StartTime = DateTime.Now,
				Status = ReleaseStatus.Running,
				Settings = new JObject
				{
					["protect_modules"] = appOptions["protect_modules"],
					["selected_modules"] = appOptions["selected_modules"],
					["enable_ldap"] = appOptions["enable_ldap"],
					["enable_registration"] = appOptions["enable_registration"],
					["enable_api_registration"] = appOptions["enable_api_registration"]
				}.ToString()
			};

			await _packageRepository.Create(packageModel);

			/*
			 * Set version number to history_database and history_storage tables tag column.
			 */

			if (dbHistory != null && dbHistory.Tag == null)
			{
				dbHistory.Tag = packageModel.Version;
				await _historyDatabaseRepository.Update(dbHistory);
			}

			var storageHistory = await _historyStorageRepository.GetLast();
			if (storageHistory != null && storageHistory.Tag == null)
			{
				storageHistory.Tag = packageModel.Version;
				await _historyStorageRepository.Update(storageHistory);
			}

			if (await _packageRepository.IsFirstPackage((int)AppId))
			{
				var historyStorages = await _historyStorageRepository.GetAll();
				_queue.QueueBackgroundWorkItem(token => _packageHelper.All((int)AppId, model, dbName, version.ToString(), packageModel.Id, historyStorages));
			}
			else
			{
				List<HistoryDatabase> historyDatabase = null;
				List<HistoryStorage> historyStorages = null;

				if (dbHistory != null && dbHistory.Tag != (int.Parse(packageModel.Version) - 1).ToString())
					historyDatabase = await _historyDatabaseRepository.GetDiffs(currentBuildNumber.ToString());

				if (storageHistory != null && storageHistory.Tag != (int.Parse(packageModel.Version) - 1).ToString())
					historyStorages = await _historyStorageRepository.GetDiffs(currentBuildNumber.ToString());

				_queue.QueueBackgroundWorkItem(token => _packageHelper.Diffs(historyDatabase, historyStorages, (int)AppId, dbName, version.ToString(), packageModel.Id));
			}

			return Ok(packageModel.Id);
		}

		[HttpGet]
		[Route("get_active_process")]
		public async Task<IActionResult> GetActiveProcess()
		{
			if (!_permissionHelper.CheckUserProfile(UserProfile, "package", RequestTypeEnum.Create))
				return StatusCode(403);

			if (AppId == null)
				return Ok();

			var process = await _packageRepository.GetActiveProcess((int)AppId);

			return Ok(process);
		}

		[HttpGet]
		[Route("get_last")]
		public async Task<IActionResult> GetLastPackage()
		{
			if (!_permissionHelper.CheckUserProfile(UserProfile, "package", RequestTypeEnum.Create))
				return StatusCode(403);

			var result = await _packageRepository.GetLastPackage((int)AppId);

			return Ok(result);
		}
	}
}