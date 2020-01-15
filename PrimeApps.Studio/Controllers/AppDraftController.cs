using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;

namespace PrimeApps.Studio.Controllers
{
	[Route("api/app_draft")]
	public class AppDraftController : Controller
	{
		private IAppDraftRepository _appDraftRepository;

		public AppDraftController(IAppDraftRepository appDraftRepository)
		{
			_appDraftRepository = appDraftRepository;
		}

		[Route("get_app_settings/{id:int}"), HttpGet]
		public async Task<IActionResult> GetAppSettings(int id)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var res = await _appDraftRepository.GetSettings(id);
			var app = new AppDraftSetting();

			if (res != null)
			{
				app.AuthTheme = res.AuthTheme;
				app.AppTheme = res.AppTheme;
                app.Options = res.Options;
			}

			return Ok(app);
		}
	}
}
