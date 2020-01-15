using System.IO;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Jobs;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/dump"), ActionFilters.CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class DumpController : BaseController
    {
        public static int OrganizationId { get; set; }
        private IConfiguration _configuration;
        private IAppDraftRepository _appDraftRepository;
        private IHostingEnvironment _hostingEnvironment;

        public DumpController(IConfiguration configuration, IAppDraftRepository appDraftRepository, IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _appDraftRepository = appDraftRepository;
            _hostingEnvironment = hostingEnvironment;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.Path.Value != "/api/dump/download")
                OrganizationId = SetOrganization(context);
        }

        [Route("create"), HttpPost, Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Create([FromBody]JObject request)
        {
            if (request["app_id"].IsNullOrEmpty())
                return BadRequest("app_id is required.");

            if (!int.TryParse(request["app_id"].ToString(), out var appId))
                return BadRequest("app_id must be a integer.");

            request["app_id"] = appId;
            var organizationAppIds = _appDraftRepository.GetAppIdsByOrganizationId(OrganizationId);

            if (!organizationAppIds.Contains(appId))
                return Unauthorized();

            var jobId = BackgroundJob.Enqueue<Dump>(dump => dump.Create(request));

            return Ok("Database dump process has been started. You'll be notified when finished. Job: " + jobId);
        }

//        [Route("restore"), HttpPost, Authorize(AuthenticationSchemes = "Bearer")]
//        public IActionResult Restore([FromBody]JObject request)
//        {
//            if (request["app_id"].IsNullOrEmpty())
//                return BadRequest("app_id is required.");
//
//            if (!int.TryParse(request["app_id"].ToString(), out var appId))
//                return BadRequest("app_id must be a integer.");
//
//            if (request["environment"].IsNullOrEmpty())
//                return BadRequest("environment is required.");
//
//            var environment = (string)request["environment"];
//
//            if (environment != "test" && environment != "production")
//                return BadRequest("environment must be 'test' or 'production'.");
//
//            request["app_id"] = appId;
//            var organizationAppIds = _appDraftRepository.GetAppIdsByOrganizationId(OrganizationId);
//
//            if (!organizationAppIds.Contains(appId))
//                return Unauthorized();
//
//            var jobId = BackgroundJob.Enqueue<Dump>(dump => dump.Restore(request));
//
//            return Ok("Database dump restore process has been started. You'll be notified when finished. Job: " + jobId);
//        }

        [Route("download")]
        public IActionResult Download([FromQuery]int appId)
        {
            var dumpDirectory = DataHelper.GetDataDirectoryPath(_configuration, _hostingEnvironment);

            return PhysicalFile(Path.Combine(dumpDirectory, $"app{appId}.bak"), "text/plain", $"app{appId}.bak");
        }
    }
}