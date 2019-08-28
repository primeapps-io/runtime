using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using PrimeApps.Admin.Helpers;
using PrimeApps.Admin.Models;
using PrimeApps.Model.Common.Organization;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _context;
        private readonly IOrganizationHelper _organizationHelper;
        private readonly IApplicationRepository _applicationRepository;

        public HomeController(IHttpContextAccessor context, IOrganizationHelper organizationHelper, IApplicationRepository applicationRepository)
        {
            _context = context;
            _organizationHelper = organizationHelper;
            _applicationRepository = applicationRepository;
        }


        [Route("")]
        public async Task<IActionResult> Index(int? id)
        {
            var platformUserRepository = (IPlatformUserRepository)HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));
            var user = platformUserRepository.Get(HttpContext.User.FindFirst("email").Value);
            var token = await _context.HttpContext.GetTokenAsync("access_token");

            var organizations = await _organizationHelper.Get(user.Id,token);
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
                    return RedirectToAction("Index", new { id = organizations[0].Id });
                }

                ViewBag.Title = selectedOrg.Name + " - " + titleText;
                ViewBag.ActiveOrganizationId = id.ToString();
            }

            else if (organizations.Any())
                ViewBag.ActiveOrganizationId = organizations[0].Id.ToString();

            return View();
        }

        //[Route("get_release")]
        //public async Task<IActionResult> GetReleaseOfApp(int id)
        //{
            
        //}

        public async Task<IActionResult> ReloadCahce()
        {
            var result = await _organizationHelper.ReloadOrganization();

            if (result)
                return RedirectToAction("Index", "Home");
            else
                return Ok();
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}