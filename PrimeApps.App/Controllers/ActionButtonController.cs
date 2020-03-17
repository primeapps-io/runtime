using PrimeApps.App.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace PrimeApps.App.Controllers
{
    [Route("api/action_button"), Authorize]
    public class ActionButtonController : ApiBaseController
    {
        private IActionButtonRepository _actionButtonRepository;
        private IComponentRepository _componentRepository;
        private IActionButtonHelper _actionButtonHelper;
        private IEnvironmentHelper _environmentHelper;
        private IConfiguration _configuration;

        public ActionButtonController(IActionButtonRepository actionButtonRepository, IActionButtonHelper actionButtonHelper, IEnvironmentHelper environmentHelper,
             IConfiguration configuration, IComponentRepository componentRepository)
        {
            _actionButtonRepository = actionButtonRepository;
            _actionButtonHelper = actionButtonHelper;
            _environmentHelper = environmentHelper;
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
            var actionButtons = await _actionButtonRepository.GetByModuleId(id, AppUser.Language);
            actionButtons = _environmentHelper.DataFilter(actionButtons.ToList());

            await _actionButtonHelper.ProcessScriptFiles(actionButtons, _componentRepository);

            return Ok(actionButtons);
        }
    }
}