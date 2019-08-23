using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PrimeApps.Admin.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Admin.Controllers
{
    [Authorize]
    public class PackageController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _context;
        private readonly IOrganizationHelper _organizationHelper;
        private readonly IPublishHelper _publishHelper;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IAppDraftRepository _appDraftRepository;

        public PackageController(IConfiguration configuration, IHttpContextAccessor context, IOrganizationHelper organizationHelper, IPublishHelper publishHelper,
            IApplicationRepository applicationRepository, IPackageRepository packageRepository, IAppDraftRepository appDraftRepository)
        {
            _configuration = configuration;
            _context = context;
            _organizationHelper = organizationHelper;
            _publishHelper = publishHelper;
            _applicationRepository = applicationRepository;
            _packageRepository = packageRepository;
            _appDraftRepository = appDraftRepository;
        }

        [Route("package")]
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

                var allPackages = await packageRepository.GetAll((int)appId);
                var lastPackage = allPackages.LastOrDefault(x => x.AppId == appId);

                ViewBag.allPackages = allPackages;
                ViewBag.lastPackage = lastPackage;

                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        [Route("publish")]
        public async Task<IActionResult> Publish(int appId)
        {
            if (appId < 0)
                return RedirectToAction("Index", "Package");

            //TODO

            return Ok();
        }

        [Route("log")]
        public async Task<ActionResult<string>> Log(int id, int appId)
        {
            var package = await _packageRepository.Get(id);
            var app = await _appDraftRepository.Get(appId);

            if (package == null || app == null)
                return BadRequest();

            var dbName = $"app{appId}";

            var path = _configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);
            var text = "";

            if (!System.IO.File.Exists($"{path}\\releases\\{dbName}\\{package.Version}\\log.txt"))
                return Ok("Your logs have been deleted...");

            using (var fs = new FileStream($"{path}\\releases\\{dbName}\\{package.Version}\\log.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs, Encoding.Default))
            {
                text = ConvertHelper.ASCIIToHTML(sr.ReadToEnd());
                sr.Close();
                fs.Close();
            }

            return text;
        }
    }
}