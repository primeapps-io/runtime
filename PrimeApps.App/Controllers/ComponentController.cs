using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Enums;

namespace PrimeApps.App.Controllers
{
	[Route("api/component"), Authorize]
	public class ComponentController : ApiBaseController
	{
		private IComponentRepository _componentRepository;

		public ComponentController(IComponentRepository componentRepository)
		{
			_componentRepository = componentRepository;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_componentRepository, PreviewMode, TenantId, AppId);

			base.OnActionExecuting(context);
		}

		[Route("get_by_type"), HttpGet]
		public async Task<IActionResult> GetActionButtons([FromQuery(Name = "component_type")]ComponentType type)
		{
			var components = await _componentRepository.GetByType(type);

			return Ok(components);
		}
	}
}
