using System;
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

            if (!int.TryParse(request["app_id"].ToString(), out var appIdParsed))
                return BadRequest("app_id must be a integer.");

            request["app_id"] = appIdParsed;
            var organizationAppIds = _appDraftRepository.GetAppIdsByOrganizationId(OrganizationId);

            if (!organizationAppIds.Contains(appIdParsed))
                return Unauthorized();

            var jobId = BackgroundJob.Enqueue<Dump>(dump => dump.Create(request));

            return Ok("Database dump process has been started. You'll be notified when finished. Job: " + jobId);
        }

        [Route("restore"), HttpPost, Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Restore([FromBody]JObject request)
        {
            if (request["app_id"].IsNullOrEmpty())
                return BadRequest("app_id is required.");

            if (!int.TryParse(request["app_id"].ToString(), out var appIdParsed))
                return BadRequest("app_id must be a integer.");

            if (!request["app_id_target"].IsNullOrEmpty())
            {
                if (!int.TryParse(request["app_id_target"].ToString(), out var appIdTargetParsed))
                    return BadRequest("app_id_target must be a integer.");

                request["app_id_target"] = appIdTargetParsed;
            }

            request["app_id"] = appIdParsed;
            var organizationAppIds = _appDraftRepository.GetAppIdsByOrganizationId(OrganizationId);

            if (!organizationAppIds.Contains(appIdParsed))
                return Unauthorized();

            var jobId = BackgroundJob.Enqueue<Dump>(dump => dump.Restore(request));

            return Ok("Database dump restore process has been started. You'll be notified when finished. Job: " + jobId);
        }

        [Route("download")]
        public IActionResult Download([FromQuery]int appId)
        {
            var dumpDirectory = _configuration.GetValue("AppSettings:DumpDirectory", string.Empty);

            return PhysicalFile(Path.Combine(dumpDirectory, $"app{appId}.dmp"), "text/plain", $"app{appId}.dmp");
        }

        [Route("test"), HttpPost, Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Test([FromBody]JObject request)
        {
            var dumpDirectory = _configuration.GetValue("AppSettings:DumpDirectory", string.Empty);
            var postgresPath = _configuration.GetValue("AppSettings:PostgresPath", string.Empty);
            var dbConnection = _configuration.GetConnectionString("PlatformDBConnection");

            switch ((string)request["command"])
            {
                case "create":
                    PosgresHelper.Create(dbConnection, (string)request["database_name"], postgresPath, dumpDirectory);
                    break;
                case "drop":
                    PosgresHelper.Drop(dbConnection, (string)request["database_name"], postgresPath, dumpDirectory);
                    break;
                case "restore":
                    PosgresHelper.Restore(dbConnection, (string)request["database_name"], postgresPath, dumpDirectory, (string)request["target_database_name"], dumpDirectory);
                    break;
            }

            return Ok();
        }

        [Route("publish"), HttpPost, Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Publish([FromBody]JObject request)
        {

            var dumpDirectory = _configuration.GetValue("AppSettings:DumpDirectory", string.Empty);

            if (request["app"].IsNullOrEmpty())
                return BadRequest("app is required.");

            if (request["environment"].IsNullOrEmpty())
                return BadRequest("environment is required.");

            var app = (string)request["app"];

            switch ((string)request["environment"])
            {
                case "test":                    
                    _posgresHelper.Drop("PlatformDBConnectionTest", app, dumpDirectory);
                    _posgresHelper.Create("PlatformDBConnectionTest", app, dumpDirectory);
                    _posgresHelper.Restore("PlatformDBConnectionTest", app, dumpDirectory, app, dumpDirectory);
                    break;
                case "prod":
                    _posgresHelper.Drop("PlatformDBConnection", app, dumpDirectory);
                    _posgresHelper.Create("PlatformDBConnection", app, dumpDirectory);
                    _posgresHelper.Restore("PlatformDBConnection", app, dumpDirectory, app, dumpDirectory);
                    break;
            }



            return Ok();

        }

    }
}