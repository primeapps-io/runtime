using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;

namespace PrimeApps.Studio.Controllers
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

		[Route("count/{id:int}"), HttpGet]
		public async Task<IActionResult> Count(int id)
		{
			var count = await _dependencyRepository.Count(id);

			if (count < 1)
				return Ok(null);

			return Ok(count);
		}
		[Route("find/{id:int}"), HttpPost]
		public async Task<IActionResult> Find(int id, [FromBody]PaginationModel paginationModel)
		{
			var dependencies = await _dependencyRepository.Find(id, paginationModel);

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

		[Route("get_by_id/{id:int}"), HttpGet]
		public async Task<Dependency> GetById(int id)
		{
			return await _dependencyRepository.GetById(id);
		}

	}
}
