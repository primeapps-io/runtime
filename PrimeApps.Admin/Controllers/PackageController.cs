using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using PrimeApps.Admin.Helpers;
using PrimeApps.Admin.Jobs;
using PrimeApps.Admin.Services;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Util.Storage;
using PublishHelper = PrimeApps.Admin.Helpers.PublishHelper;

namespace PrimeApps.Admin.Controllers
{
    [Authorize]
    public class PackageController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _context;
        private readonly IOrganizationHelper _organizationHelper;
        private readonly IPublishHelper _publishHelper;
        private readonly IPlatformRepository _platformRepository;
        private readonly IPlatformUserRepository _platformUserRepository;
        private readonly IReleaseRepository _releaseRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IUnifiedStorage _storage;
        private IBackgroundTaskQueue Queue;
        private readonly IPublish _publish;

        public PackageController(IBackgroundTaskQueue _queue, IConfiguration configuration, IHttpContextAccessor context, IOrganizationHelper organizationHelper, IPublishHelper publishHelper,
            IPlatformRepository platformRepository, IUnifiedStorage storage, IPlatformUserRepository platformUserRepository, IReleaseRepository releaseRepository, ITenantRepository tenantRepository,
            IPublish publish)
        {
            Queue = _queue;
            _configuration = configuration;
            _context = context;
            _organizationHelper = organizationHelper;
            _publishHelper = publishHelper;
            _platformUserRepository = platformUserRepository;
            _platformRepository = platformRepository;
            _releaseRepository = releaseRepository;
            _tenantRepository = tenantRepository;
            _storage = storage;
            _publish = publish;
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

        public async Task<IActionResult> Index(int? appId, int? orgId, bool applying = false)
        {
            var user = _platformUserRepository.Get(HttpContext.User.FindFirst("email").Value);
            var token = await HttpContext.GetTokenAsync("access_token");
            var organizations = await _organizationHelper.Get(user.Id, token);

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

                if (!applying)
                    ViewBag.ProcessActive = await _releaseRepository.IsThereRunningProcess((int)appId);
                else
                    ViewBag.ProcessActive = applying;
                
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        [Route("apply")]
        
        public async Task<IActionResult> Apply(int id, int orgId, string appUrl, string authUrl)
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            var runningPublish = await _releaseRepository.IsThereRunningProcess(id);

            if (runningPublish)
                throw new Exception($"Already have a running publish for app{id} database!");

            Queue.QueueBackgroundWorkItem(x => _publish.PackageApply(id, orgId, token, AppUser.Id, appUrl, authUrl, false));
            
            return Ok();
        }

        [Route("update_tenants")]
        public async Task<IActionResult> UpdateTenants(int appId, int orgId)
        {
            if (appId < 0)
                return RedirectToAction("Index", "Package");

            var token = await HttpContext.GetTokenAsync("access_token");
            var studioClient = new StudioClient(_configuration, token, appId, orgId);
            var app = await studioClient.AppDraftGetById(appId);

            if (app == null)
                return BadRequest();

            var available = await _releaseRepository.IsThereRunningProcess(appId);

            if (available)
                return BadRequest($"There is another process for application {app.Name}");

            var checkButton = await _publishHelper.IsActiveUpdateButton(appId);

            if (!checkButton)
                return BadRequest();

            var tenantIds = await _tenantRepository.GetByAppId(appId);

            if (tenantIds.Count < 1)
                return NotFound();

            Queue.QueueBackgroundWorkItem(x => _publish.UpdateTenants(appId, orgId, AppUser.Id, tenantIds, token));

            return RedirectToAction("Index", "Package", new {appId = appId, orgId = orgId, applying = true});
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