using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Jobs;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/dump"), Authorize(AuthenticationSchemes = "Bearer"), ActionFilters.CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class DumpController : ApiBaseController
    {
        private IPosgresHelper _posgresHelper;
        private IConfiguration _configuration;

        public DumpController(IPosgresHelper posgresHelper, IConfiguration configuration)
        {
            _posgresHelper = posgresHelper;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
        }

        [Route("create"), HttpPost]
        public IActionResult Create([FromBody]JObject request)
        {
            if (string.IsNullOrEmpty(request["app_ids"].ToString()) || !request["app_ids"].HasValues)
                return BadRequest("app_ids is required.");

            if (request["app_ids"].GetType() != typeof(JArray))
                return BadRequest("app_ids not in valid format.");

            if (string.IsNullOrEmpty(request["repo_name"].ToString()))
                return BadRequest("repo_name is required.");

            if (((JArray)request["app_ids"]).Count < 1)
                return BadRequest("app_ids should not be empty.");

            foreach (var id in request["app_ids"])
            {
                var result = int.TryParse(id.ToString(), out var parsedId);

                if (!result)
                    return BadRequest("App id: " + id + " can not parsed in app_ids.");
            }

            var jobId = BackgroundJob.Enqueue<Dump>(dump => dump.Run(request));

            return Ok("Database dump process has been started. You'll be notified when finished. Job: " + jobId);
        }

        [Route("test"), HttpPost]
        public IActionResult Test([FromBody]JObject request)
        {
            var dumpDirectory = _configuration.GetValue("AppSettings:DumpDirectory", string.Empty);

            switch ((string)request["command"])
            {
                case "create":
                    _posgresHelper.Create("PlatformDBConnection", (string)request["database_name"], dumpDirectory);
                    break;
                case "drop":
                    _posgresHelper.Drop("PlatformDBConnection", (string)request["database_name"], dumpDirectory);
                    break;
                case "restore":
                    _posgresHelper.Restore("PlatformDBConnection", (string)request["database_name"], dumpDirectory, (string)request["target_database_name"], dumpDirectory);
                    break;
            }

            return Ok();
        }
    }
}