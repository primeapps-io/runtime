using System;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Repositories.Interfaces;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.App.Extensions;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.App.Controllers
{
    [Route("api/action_button"), Authorize]

    public class ActionButtonController : ApiBaseController
    {
        private IActionButtonRepository _actionButtonRepository;
        private IActionButtonHelper _actionButtonHelper;
        private IComponentRepository _componentRepository;
        private IConfiguration _configuration;

        public ActionButtonController(IActionButtonRepository actionButtonRepository, IActionButtonHelper actionButtonHelper, IComponentRepository componentRepository, IConfiguration configuration)
        {
            _actionButtonRepository = actionButtonRepository;
            _actionButtonHelper = actionButtonHelper;
            _componentRepository = componentRepository;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_actionButtonRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_componentRepository, PreviewMode, TenantId, AppId);

            base.OnActionExecuting(context);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> GetActionButtons(int id)
        {
            var actionButtons = await _actionButtonRepository.GetByModuleId(id);
            var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
            previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

       
//            if (Request.IsLocal() || previewMode == "app")
//                await _actionButtonHelper.ProcessScriptFiles(actionButtons, _componentRepository);

            return Ok(actionButtons);
        }
    }
}