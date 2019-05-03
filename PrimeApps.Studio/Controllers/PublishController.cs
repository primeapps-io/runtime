using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/publish")]
    public class PublishController : DraftBaseController
    {
        private IPublishRepository _publishRepository;
        private IPermissionHelper _permissionHelper;

        public PublishController(IPublishRepository publishRepository, IPermissionHelper permissionHelper)
        {
            _publishRepository = publishRepository;
            _permissionHelper = permissionHelper;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_publishRepository, PreviewMode, TenantId, AppId);
            base.OnActionExecuting(context);
        }

        [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "publish", RequestTypeEnum.Create))
                return StatusCode(403);

            var result = _publishRepository.CleanUp();

            return Ok(result);
        }
    }
}