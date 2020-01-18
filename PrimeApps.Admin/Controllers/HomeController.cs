using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Admin.Helpers;
using PrimeApps.Admin.Models;
using PrimeApps.Admin.Services;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Admin.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
        private IOrganizationHelper _organizationHelper;
        private ITemplateRepository _templateRepository;
        private IHistoryDatabaseRepository _historyDatabaseRepository;
        private IApplicationRepository _applicationRepository;
        private IReleaseRepository _releaseRepository;
        private IHistoryStorageRepository _historyStorageRepository;
        private ITenantRepository _tenantRepository;
		private IBackgroundTaskQueue _queue;
		private IMigrationHelper _migrationHelper;

        public HomeController(IOrganizationHelper organizationHelper, IBackgroundTaskQueue queue, IMigrationHelper migrationHelper,
            ITemplateRepository templateRepository,
            IHistoryDatabaseRepository historyDatabaseRepository,
            IApplicationRepository applicationRepository,
            IReleaseRepository releaseRepository,
            IHistoryStorageRepository historyStorageRepository,
            ITenantRepository tenantRepository)
		{
			_organizationHelper = organizationHelper;
            _templateRepository = templateRepository;
            _historyDatabaseRepository = historyDatabaseRepository;
			_applicationRepository = applicationRepository;
            _releaseRepository = releaseRepository;
            _historyStorageRepository = historyStorageRepository;
            _tenantRepository = tenantRepository;
			_queue = queue;
			_migrationHelper = migrationHelper;
		}

		[Route("")]
		public async Task<IActionResult> Index(int? id)
		{
			var token = await HttpContext.GetTokenAsync("access_token");

			var organizations = await _organizationHelper.Get(token);
			var user = await _organizationHelper.GetUser(token);

			ViewBag.Organizations = organizations;
			ViewBag.User = user;

			if (id != null)
			{
				var selectedOrg = organizations.FirstOrDefault(x => x.Id == id);
				if (selectedOrg == null)
				{
					ViewBag.ActiveOrganizationId = organizations[0].Id.ToString();
					return RedirectToAction("Index", new { id = organizations[0].Id });
				}

				ViewBag.ActiveOrganizationId = id.ToString();
			}

			else if (organizations.Any())
				ViewBag.ActiveOrganizationId = organizations[0].Id.ToString();

			return View();
		}

		[Route("Reload")]
		public IActionResult Reload()
		{
			var result = _organizationHelper.ReloadOrganization();

			return RedirectToAction("Index", "Home");
		}

		[Route("Logout")]
		public async Task<IActionResult> Logout(int? id)
		{
			await HttpContext.SignOutAsync();

			return Redirect("https:///Account/Logout?returnUrl=" + Request.Scheme + "://" + Request.Host.Value);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		[HttpGet, Route("healthz")]
		public IActionResult Healthz()
		{
			return Ok();
		}

		[Route("migration")]
		public IActionResult Migration([FromQuery] string ids)
		{
			var isLocal = Request.Host.Value.Contains("localhost");
			var schema = Request.Scheme;
            var idsArr = ids.Split(",");

            BackgroundJob.Enqueue<MigrationHelper>(x => x.AppMigration(schema, isLocal, idsArr));

			return Ok();
		}

	}
}