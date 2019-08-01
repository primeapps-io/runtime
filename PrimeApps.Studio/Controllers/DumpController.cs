using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Jobs;
using PrimeApps.Studio.Services;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/dump"), Authorize(AuthenticationSchemes = "Bearer"), ActionFilters.CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class DumpController : ApiBaseController
    {
        private IBackgroundTaskQueue Queue;
        private IConfiguration _configuration;
        private IOrganizationRepository _organizationRepository;
        private IOrganizationUserRepository _organizationUserRepository;
        private IAppDraftRepository _appDraftRepository;
        private IPlatformUserRepository _platformUserRepository;
        private IPlatformRepository _platformRepository;
        private IServiceScopeFactory _serviceScopeFactory;
        private IStudioUserRepository _studioUserRepository;
        private IApplicationRepository _applicationRepository;
        private ITeamRepository _teamRepository;
        private IGiteaHelper _giteaHelper;
        private IPermissionHelper _permissionHelper;
        private IOrganizationHelper _organizationHelper;
        private IHostingEnvironment _hostingEnvironment;

        public DumpController(IBackgroundTaskQueue queue,
            IConfiguration configuration,
            IOrganizationRepository organizationRepository,
            IOrganizationUserRepository organizationUserRepository,
            IPlatformUserRepository platformUserRepository,
            IAppDraftRepository applicationDraftRepository,
            ITeamRepository teamRepository,
            IApplicationRepository applicationRepository,
            IPlatformRepository platformRepository,
            IStudioUserRepository studioUserRepository,
            IServiceScopeFactory serviceScopeFactory,
            IPermissionHelper permissionHelper,
            IOrganizationHelper organizationHelper,
            IGiteaHelper giteaHelper,
            IHostingEnvironment hostingEnvironment)
        {
            Queue = queue;
            _organizationRepository = organizationRepository;
            _appDraftRepository = applicationDraftRepository;
            _platformUserRepository = platformUserRepository;
            _organizationUserRepository = organizationUserRepository;
            _serviceScopeFactory = serviceScopeFactory;
            _studioUserRepository = studioUserRepository;
            _applicationRepository = applicationRepository;
            _platformRepository = platformRepository;
            _teamRepository = teamRepository;
            _configuration = configuration;

            _giteaHelper = giteaHelper;
            _permissionHelper = permissionHelper;
            _organizationHelper = organizationHelper;
            _hostingEnvironment = hostingEnvironment;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_organizationRepository);
            SetCurrentUser(_appDraftRepository);
            SetCurrentUser(_platformUserRepository);
            SetCurrentUser(_organizationUserRepository);
            SetCurrentUser(_studioUserRepository);
            SetCurrentUser(_platformRepository);
            SetCurrentUser(_teamRepository);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]JObject model)
        {
            if (string.IsNullOrEmpty(model["app_ids"].ToString()) || !model["app_ids"].HasValues)
                return BadRequest("app_ids is required.");

            if (model["app_ids"].GetType() != typeof(JArray))
                return BadRequest("app_ids not in valid format.");

            if (string.IsNullOrEmpty(model["repo_name"].ToString()))
                return BadRequest("repo_name is required.");

            if (((JArray)model["app_ids"]).Count < 1)
                return BadRequest("app_ids should not be empty.");

            var repoInfo = await _giteaHelper.GetRepositoryInfo(model["repo_name"].ToString(), OrganizationId);

            if (repoInfo.IsNullOrEmpty())
                return BadRequest(model["repo_name"] + " not found.");

            foreach (var id in model["app_ids"])
            {
                var result = int.TryParse(id.ToString(), out var parsedId);

                if (!result)
                    return BadRequest("App id: " + id + " can not parsed in app_ids.");

                var app = await _appDraftRepository.Get(parsedId);

                //TODO: Perapole organization (perapole) and git account (adminsecurifycom) problem must be solved.
                //if (app.OrganizationId != OrganizationId)
                //    return BadRequest("App " + id + " does not belong to your organization.");
            }

            var giteaToken = _giteaHelper.GetToken();

            var jobId = BackgroundJob.Enqueue<Dump>(dump => dump.Run(model, repoInfo, giteaToken));

            return Ok("Database dump process has been started. You'll be notified when finished. Job: " + jobId);
        }
    }
}