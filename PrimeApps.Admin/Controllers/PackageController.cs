using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using PrimeApps.Admin.Helpers;
using PrimeApps.Admin.Jobs;
using PrimeApps.Admin.Services;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Admin.Controllers
{
    [Authorize]
    public class PackageController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IOrganizationHelper _organizationHelper;
        private readonly IPublishHelper _publishHelper;
        private readonly IPlatformRepository _platformRepository;
        private readonly IPlatformUserRepository _platformUserRepository;
        private readonly IReleaseRepository _releaseRepository;
        private readonly ITenantRepository _tenantRepository;
        private IBackgroundTaskQueue _queue;
        private readonly IPublish _publish;
        private IHostingEnvironment _hostingEnvironment;

        public PackageController(IBackgroundTaskQueue queue, IConfiguration configuration, IOrganizationHelper organizationHelper, IPublishHelper publishHelper,
            IPlatformRepository platformRepository, IPlatformUserRepository platformUserRepository, IReleaseRepository releaseRepository, ITenantRepository tenantRepository,
            IPublish publish, IHostingEnvironment hostingEnvironment)
        {
            _queue = queue;
            _configuration = configuration;
            _organizationHelper = organizationHelper;
            _publishHelper = publishHelper;
            _platformUserRepository = platformUserRepository;
            _platformRepository = platformRepository;
            _releaseRepository = releaseRepository;
            _tenantRepository = tenantRepository;
            _publish = publish;
            _hostingEnvironment = hostingEnvironment;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContextUser();
            SetCurrentUser(_platformUserRepository);
            SetCurrentUser(_platformRepository);
            SetCurrentUser(_releaseRepository);
            SetCurrentUser(_tenantRepository);

            base.OnActionExecuting(context);
        }

        public async Task<IActionResult> Index(int? appId, int? orgId)
        {
            var user = _platformUserRepository.Get(HttpContext.User.FindFirst("email").Value);
            var token = await HttpContext.GetTokenAsync("access_token");
            var organizations = await _organizationHelper.Get(token);

            ViewBag.Organizations = organizations;
            ViewBag.User = user;

            if (appId != null)
            {
                ViewBag.UpdateButtonActive = await _publishHelper.IsActiveUpdateButton((int)appId);
                ViewBag.LastRelease = await _releaseRepository.GetLast((int)appId);

                var selectedOrg = organizations.FirstOrDefault(x => x.Id == orgId);
                if (selectedOrg != null)
                {
                    ViewBag.ActiveOrganizationId = selectedOrg.Id.ToString();
                    ViewBag.CurrentApp = selectedOrg.Apps.FirstOrDefault(x => x.Id == appId);
                }

                var studioClient = new StudioClient(_configuration, token, (int)appId, selectedOrg.Id);
                var lastPackage = await studioClient.PackageLastDeployment();
                ViewBag.LastPackage = lastPackage;


                var releaseId = HttpContext.Request.Cookies[$"app{appId}"];
                if (!string.IsNullOrEmpty(releaseId))
                {
                    var release = await _releaseRepository.Get(int.Parse(releaseId));
                    ViewBag.ProcessActive = release.Status == ReleaseStatus.Running;

                    ViewBag.ReleaseId = int.Parse(releaseId);
                }
                else
                {
                    ViewBag.ProcessActive = false;
                    ViewBag.ReleaseId = 0;
                }


                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        [Route("apply")]
        public async Task<IActionResult> Apply(int id, int orgId, string appUrl, string authUrl, bool useSsl)
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            var runningPublish = await _releaseRepository.IsThereRunningProcess(id);

            if (runningPublish)
                throw new Exception($"Already have a running publish for app{id} database!");

            var newRelease = new Release
            {
                StartTime = DateTime.Now,
                Status = ReleaseStatus.Running,
                Version = ""
            };

            await _releaseRepository.Create(newRelease);

            _queue.QueueBackgroundWorkItem(x => _publish.PackageApply(id, orgId, token, newRelease.Id, AppUser.Id, appUrl, authUrl, useSsl));

            return Ok(newRelease.Id);
        }

        [Route("updatetenants")]
        public async Task<IActionResult> UpdateTenants(int id, int orgId)
        {
            if (id < 0)
                return RedirectToAction("Index", "Package");

            var token = await HttpContext.GetTokenAsync("access_token");
            var studioClient = new StudioClient(_configuration, token, id, orgId);
            var app = await studioClient.AppDraftGetById(id);

            if (app == null)
                return BadRequest();

            var available = await _releaseRepository.IsThereRunningProcess(id);

            if (available)
                return BadRequest($"There is another process for application {app.Name}");

            var checkButton = await _publishHelper.IsActiveUpdateButton(id);

            if (!checkButton)
                return BadRequest();

            var tenantIds = await _tenantRepository.GetByAppId(id);

            if (tenantIds.Count == 0)
                return NotFound();


            var newRelease = new Release
            {
                StartTime = DateTime.Now,
                Status = ReleaseStatus.Running,
                Version = "pointer"
            };

            await _releaseRepository.Create(newRelease);

            _queue.QueueBackgroundWorkItem(x => _publish.UpdateTenants(id, orgId, AppUser.Id, tenantIds, token, newRelease.Id));

            return Ok(newRelease.Id);
        }

        [Route("log")]
        public async Task<ActionResult<string>> Log(int id, int appId, int orgId)
        {
            var token = await HttpContext.GetTokenAsync("access_token");
            var studioClient = new StudioClient(_configuration, token, appId, orgId);

            var package = await studioClient.PackageGetById(id);
            var app = await studioClient.AppDraftGetById(appId);

            if (package == null || app == null)
                return BadRequest();

            var dbName = $"app{appId}";

            var path = DataHelper.GetDataDirectoryPath(_configuration, _hostingEnvironment);
            var text = "";

            if (!System.IO.File.Exists(Path.Combine(path, "packages", dbName, package.Version, "log.txt")))
                return Ok("Your logs have been deleted...");

            using (var fs = new FileStream(Path.Combine(path, "packages", dbName, package.Version, "log.txt"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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