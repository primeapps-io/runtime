using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        private IApplicationRepository _applicationRepository;

        public AccountController(IApplicationRepository applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            var appInfo = await _applicationRepository.Get(Request.Host.Value);

            Response.Cookies.Delete("tenant_id");
            await HttpContext.SignOutAsync();

            return StatusCode(200, new {redirectUrl = Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/Account/Logout?returnUrl=" + Request.Scheme + "://" + appInfo.Setting.AppDomain});
        }
    }
}