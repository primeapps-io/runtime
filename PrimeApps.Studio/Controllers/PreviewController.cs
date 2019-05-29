using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/preview")]
    public class PreviewController : DraftBaseController
    {
        private IConfiguration _configuration; 

        public PreviewController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);

            base.OnActionExecuting(context);
        }

        [Route("key"), HttpGet]
        public IActionResult Key()
        {
            if (AppId == null)
                return BadRequest("AppId can not be null");

            var token = CryptoHelper.Encrypt("app_id=" + AppId);
            
            Response.Cookies.Append("app_id", AppId.ToString());
            
            return Ok(token);
        }
    }
}
