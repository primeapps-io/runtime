using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Admin.Helpers;
using PrimeApps.Admin.Models;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IOrganizationHelper _organizationHelper;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IPlatformUserRepository _platformUserRepository;

        public HomeController(IOrganizationHelper organizationHelper, IApplicationRepository applicationRepository, IPlatformUserRepository platformUserRepository)
        {
            _organizationHelper = organizationHelper;
            _applicationRepository = applicationRepository;
            _platformUserRepository = platformUserRepository;
        }

        [Route("")]
        public async Task<IActionResult> Index(int? id)
        {
            var user = _platformUserRepository.Get(HttpContext.User.FindFirst("email").Value);
            var token = await HttpContext.GetTokenAsync("access_token");

            var organizations = await _organizationHelper.Get(user.Id, token);
            var titleText = "PrimeApps Admin";

            ViewBag.Title = titleText;
            ViewBag.Organizations = organizations;
            ViewBag.User = user;

            if (id != null)
            {
                var selectedOrg = organizations.FirstOrDefault(x => x.Id == id);
                if (selectedOrg == null)
                {
                    ViewBag.ActiveOrganizationId = organizations[0].Id.ToString();
                    return RedirectToAction("Index", new {id = organizations[0].Id});
                }

                ViewBag.Title = selectedOrg.Name + " - " + titleText;
                ViewBag.ActiveOrganizationId = id.ToString();
            }

            else if (organizations.Any())
                ViewBag.ActiveOrganizationId = organizations[0].Id.ToString();

            return View();
        }

        public IActionResult ReloadCahce()
        {
            var result = _organizationHelper.ReloadOrganization();

            return RedirectToAction("Index", "Home");
        }

        [Route("Logout")]
        public async Task<IActionResult> Logout(int? id)
        {
            var appInfo = await _applicationRepository.Get(Request.Host.Value);
            await HttpContext.SignOutAsync();

            return Redirect(Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/Account/Logout?returnUrl=" + Request.Scheme + "://" + appInfo.Setting.AppDomain);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}