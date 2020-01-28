using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Admin.Helpers;
using PrimeApps.Admin.Services;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Admin.Controllers
{
	public class MigrationsController : BaseController
	{
		private readonly IConfiguration _configuration;
		private readonly IOrganizationHelper _organizationHelper;
		private IBackgroundTaskQueue _queue;
		private IMigrationHelper _migrationHelper;
		private IPlatformRepository _platformRepository;


		public MigrationsController(IConfiguration configuration, IOrganizationHelper organizationHelper, IBackgroundTaskQueue queue, IMigrationHelper migrationHelper, IPlatformRepository platformRepository)
		{
			_configuration = configuration;
			_organizationHelper = organizationHelper;
			_queue = queue;
			_migrationHelper = migrationHelper;
			_platformRepository = platformRepository;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContextUser();
			SetCurrentUser(_platformRepository);
			base.OnActionExecuting(context);
		}

		public async Task<IActionResult> Index(int? id)
		{
			var token = await HttpContext.GetTokenAsync("access_token");
			if (token != null)
			{
				var organizations = await _organizationHelper.Get(token);
				var user = await _organizationHelper.GetUser(token);

				ViewBag.Organizations = organizations;
				ViewBag.User = user;
				ViewBag.ProcessActive = false;
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
			else
				return RedirectToAction("Index", "Home");
		}

		[Route("applymigrations")]
		public async Task<IActionResult> ApplyMigrationsAsync(int orgId)
		{
			var apps = await _platformRepository.GetAllApp();
			if (apps == null) 
				return Ok();
			
			var ids = apps.Where(x => x.Name != "primeapps_app" && x.Name != "primeapps_studio" && x.Name != "primeapps_preview").Select(app => app.Id).ToList();

			_queue.QueueBackgroundWorkItem(q => _migrationHelper.ApplyMigrations(ids));

			return Ok();
		}
	}
}