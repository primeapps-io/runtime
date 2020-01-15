using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
		private readonly IOrganizationHelper _organizationHelper;
		private readonly IApplicationRepository _applicationRepository;
		private readonly IPlatformUserRepository _platformUserRepository;
		private IBackgroundTaskQueue _queue;
		private IMigrationHelper _migrationHelper;

		public HomeController(IOrganizationHelper organizationHelper, IBackgroundTaskQueue queue, IMigrationHelper migrationHelper, IApplicationRepository applicationRepository, IPlatformUserRepository platformUserRepository)
		{
			_organizationHelper = organizationHelper;
			_applicationRepository = applicationRepository;
			_platformUserRepository = platformUserRepository;
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
			_queue.QueueBackgroundWorkItem(token => _migrationHelper.AppMigration(schema, isLocal, ids.Split(",")));
			return Ok();
		}

	}
}