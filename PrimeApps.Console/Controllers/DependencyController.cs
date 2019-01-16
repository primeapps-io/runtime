using PrimeApps.Console.Models;
using PrimeApps.Console.Helpers;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http.Extensions;
using System.Linq;
using PrimeApps.Model.Common;

namespace PrimeApps.Console.Controllers
{
	[Route("api/dependency")]
	public class DependencyController : DraftBaseController
	{
		private IDependencyRepository _dependencyRepository;
		private IProfileRepository _profileRepository;
		private ISettingRepository _settingRepository;
		private IConfiguration _configuration;
		private Warehouse _warehouse;

		private IModuleHelper _moduleHelper;

		public DependencyController(IDependencyRepository dependencyRepository, IProfileRepository profileRepository, ISettingRepository settingRepository, Warehouse warehouse, IModuleHelper moduleHelper, IConfiguration configuration)
		{
			_dependencyRepository = dependencyRepository;

			_profileRepository = profileRepository;
			_settingRepository = settingRepository;
			_warehouse = warehouse;
			_configuration = configuration;
			_moduleHelper = moduleHelper;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_dependencyRepository, PreviewMode, AppId, TenantId);
			SetCurrentUser(_profileRepository, PreviewMode, AppId, TenantId);
			SetCurrentUser(_settingRepository, PreviewMode, AppId, TenantId);

			base.OnActionExecuting(context);
		}

		[Route("count"), HttpGet]
		public async Task<IActionResult> Count()
		{
			var count = await _dependencyRepository.Count();

			if (count < 1)
				return Ok(null);

			return Ok(count);
		}
		[Route("find"), HttpPost]
		public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
		{
			var dependencies = await _dependencyRepository.Find(paginationModel);

			if (dependencies == null)
				return Ok(null);

			return Ok(dependencies);
		}

		[Route("get_all"), HttpGet]
		public async Task<ICollection<Dependency>> GetAll()
		{
			return await _dependencyRepository.GetAll();
		}

		[Route("get_all_deleted"), HttpGet]
		public async Task<ICollection<Dependency>> GetAllDeleted()
		{
			return await _dependencyRepository.GetAllDeleted();
		}

	}
}
