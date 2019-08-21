using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Admin.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Admin.Controllers
{
    [Authorize]
    public class PackageController : Controller
    {
        private readonly IHttpContextAccessor _context;
        private readonly IOrganizationHelper _organizationHelper;
        private readonly IApplicationRepository _applicationRepository;

        public PackageController(IHttpContextAccessor context, IOrganizationHelper organizationHelper, IApplicationRepository applicationRepository)
        {
            _context = context;
            _organizationHelper = organizationHelper;
            _applicationRepository = applicationRepository;
        }

        public async Task<IActionResult> Index(int? appId, int? orgId)
        {
            var platformUserRepository = (IPlatformUserRepository)HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));
            var packageRepository = (IPackageRepository)HttpContext.RequestServices.GetService(typeof(IPackageRepository));
            var user = platformUserRepository.Get(HttpContext.User.FindFirst("email").Value);
            var organizations = await _organizationHelper.Get(user.Id);

            ViewBag.Organizations = organizations;
            ViewBag.User = user;

            if (appId != null)
            {
                var selectedOrg = organizations.FirstOrDefault(x => x.Id == orgId);
                if (selectedOrg != null)
                {
                    ViewBag.ActiveOrganizationId = selectedOrg.Id.ToString();
                    ViewBag.CurrentApp = selectedOrg.Apps.FirstOrDefault(x => x.Id == appId);
                }

                var allVersions = await packageRepository.GetAll((int)appId);
                var lastVersion = allVersions.OrderByDescending(x => x.Id).FirstOrDefault(x => x.AppId == appId);

                ViewBag.allVersions = allVersions;
                ViewBag.lastVersion = lastVersion;

                return View();
            }

            return RedirectToAction("Index", "Home"); 
        }

        public async Task<IActionResult> Publish(int appId, int orgId)
        {


            return Ok();
        }
    }
}