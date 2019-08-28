using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using PrimeApps.Admin.Helpers;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Util.Storage;

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
        private readonly IAppDraftRepository _appDraftRepository;
        private readonly IPlatformRepository _platformRepository;
        private readonly IPlatformUserRepository _platformUserRepository;
        private readonly IReleaseRepository _releaseRepository;
        private readonly IUnifiedStorage _storage;

        public PackageController(IConfiguration configuration, IHttpContextAccessor context, IOrganizationHelper organizationHelper, IPublishHelper publishHelper,
            IApplicationRepository applicationRepository, IPackageRepository packageRepository, IAppDraftRepository appDraftRepository, IPlatformRepository platformRepository, IUnifiedStorage storage,
            IPlatformUserRepository platformUserRepository, IReleaseRepository releaseRepository)
        {
            _configuration = configuration;
            _context = context;
            _organizationHelper = organizationHelper;
            _publishHelper = publishHelper;
            _applicationRepository = applicationRepository;
            _platformUserRepository = platformUserRepository;
            _appDraftRepository = appDraftRepository;
            _platformRepository = platformRepository;
            _releaseRepository = releaseRepository;
            _storage = storage;
        }


        [Route("package")]
        public async Task<IActionResult> Index(int? appId, int? orgId, bool applying)
        {
            var user = _platformUserRepository.Get(HttpContext.User.FindFirst("email").Value);
            var token = await _context.HttpContext.GetTokenAsync("access_token");
            var organizations = await _organizationHelper.Get(user.Id, token);

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

                var studioClient = new StudioClient(_configuration, token, (int)appId, selectedOrg.Id);
                var lastPackage = await studioClient.PackageLastDeployment();
                ViewBag.lastPackage = lastPackage;

                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        [Route("apply")]
        public async Task<IActionResult> Apply(int id, string applyingVersion, List<Package> packages)
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var app = await _appDraftRepository.Get(id);
            var studioClient = new StudioClient(_configuration, token, id, app.OrganizationId);

            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            var appString = JsonConvert.SerializeObject(app, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            var versions = new List<string>() { applyingVersion };

            foreach (var package in packages)
            {
                if (package.Version != applyingVersion)
                {
                    var release = await _releaseRepository.Get(id, package.Version);
                    if (release == null)
                        versions.Add(package.Version);
                    else
                        break;
                }
            }

            var studioClientId = _configuration.GetValue("AppSettings:ClientId", string.Empty);

            var studioApp = await _platformRepository.AppGetByName(studioClientId);

            await PrimeApps.Model.Helpers.PublishHelper.ApplyVersions(_configuration, _storage, JObject.Parse(appString), app.OrganizationId, $"app{id}", versions, CryptoHelper.Decrypt(studioApp.Secret), true, 1, token);

            return RedirectToAction("Index", "Package", new { appId = app.Id, orgId = app.OrganizationId, applying = true });
        }

        /*[Route("publish")]
        public async Task<IActionResult> Publish(int appId)
        {
            if (appId < 0)
                return RedirectToAction("Index", "Package");

            //TODO

            return Ok();
        }*/

        [Route("log")]
        public async Task<ActionResult<string>> Log(int id, int appId, int orgId)
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var studioClient = new StudioClient(_configuration, token, appId, orgId);

            var package = await studioClient.PackageGetById(id);
            var app = await _appDraftRepository.Get(appId);

            if (package == null || app == null)
                return BadRequest();

            var dbName = $"app{appId}";

            var path = _configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);
            var text = "";

            if (!System.IO.File.Exists($"{path}\\releases\\{dbName}\\{package.Version}\\log.txt"))
                return Ok("Your logs have been deleted...");

            using (var fs = new FileStream($"{path}\\releases\\{dbName}\\{package.Version}\\log.txt", FileMode.Open,
                FileAccess.Read, FileShare.ReadWrite))
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