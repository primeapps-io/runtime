using System;
using System.Text;
using System.Threading.Tasks;
using Devart.Data.PostgreSql;
using LibGit2Sharp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Services;
using Sentry.Protocol;

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
            IGiteaHelper giteaHelper)
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

            if (((JArray)model["app_ids"]).Count > 0)
            {
                var repoInfo = await _giteaHelper.GetRepositoryInfo(model["repo_name"].ToString(), OrganizationId);

                if (repoInfo != null)
                {
                    var localPath = _giteaHelper.CloneRepository(repoInfo["clone_url"].ToString(), repoInfo["name"].ToString());

                    foreach (var id in model["app_ids"])
                    {
                        var result = int.TryParse(id.ToString(), out var parsedId);

                        if (!result)
                            return BadRequest("App id: " + id + " can not parsed in app_ids.");

                        var app = await _appDraftRepository.Get(parsedId);

                        if (app.OrganizationId != OrganizationId)
                            return BadRequest("App " + id + " is not belong your organization.");

                        var giteaDirectory = _configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);

                        var localFolder = giteaDirectory + repoInfo["name"] + "\\" + "database";

                        //var dump = GetSqlDump($"app{id}");

                        var connectionString = _configuration.GetConnectionString("StudioDBConnection");
                        string dump = "";
                        try
                        {
                            var npgsqlConnection = new NpgsqlConnectionStringBuilder(connectionString);

                            var connString = $"host={npgsqlConnection.Host};port={npgsqlConnection.Port};user id={npgsqlConnection.Username};password={npgsqlConnection.Password};database=app{id};";
                            ErrorHandler.LogMessage(connString, SentryLevel.Info);

                            var connection = new PgSqlConnection(connString);

                            connection.Open();
                            var dumpConnection = new PgSqlDump {Connection = connection, Schema = "public", IncludeDrop = false};
                            dumpConnection.Backup();
                            connection.Close();

                            dump = dumpConnection.DumpText;
                        }
                        catch (Exception ex)
                        {
                            ErrorHandler.LogMessage(ex.InnerException.Message, SentryLevel.Info);
                            throw ex;
                            return BadRequest(ex);
                        }

                        if (string.IsNullOrEmpty(dump))
                            return BadRequest("Dump string can not be null.");

                        using (var fs = System.IO.File.Create($"{localFolder}\\app{id}.sql"))
                        {
                            var info = new UTF8Encoding(true).GetBytes(dump);
                            // Add some information to the file.
                            fs.Write(info, 0, info.Length);
                            //System.IO.File.WriteAllText (@"D:\path.txt", contents, Encoding.UTF8);
                        }
                    }

                    using (var repo = new Repository(localPath))
                    {
                        //System.IO.File.WriteAllText(localPath, sample);
                        Commands.Stage(repo, "*");

                        var signature = new Signature(
                            new Identity("system", "system@primeapps.io"), DateTimeOffset.Now);

                        var status = repo.RetrieveStatus();

                        if (!status.IsDirty)
                        {
                            _giteaHelper.DeleteDirectory(localPath);
                            return BadRequest("Unhandled exception. Repo status is dirty.");
                        }

                        // Commit to the repository
                        var commit = repo.Commit("Database dump", signature, signature);
                        _giteaHelper.Push(repo);

                        repo.Dispose();
                        _giteaHelper.DeleteDirectory(localPath);
                        return Ok();
                    }
                }
            }

            return BadRequest("app_ids should not be empty.");
        }
    }
}