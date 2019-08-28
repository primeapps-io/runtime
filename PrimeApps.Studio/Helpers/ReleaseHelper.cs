using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Studio.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Common;
using Newtonsoft.Json.Serialization;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Studio.Storage;
using PrimeApps.Util.Storage;

namespace PrimeApps.Studio.Helpers
{
    public interface IReleaseHelper
    {
        Task All(int appId, bool clearAllRecords, string dbName, string version, int deploymentId, List<HistoryStorage> historyStorages);

        Task Diffs(List<HistoryDatabase> historyDatabases, List<HistoryStorage> historyStorages, int appId, string dbName, string version, int deploymentId);
        Task<List<string>> CheckMissingFiles(int version, int appId);
        Task<List<JObject>> CheckMissingScripts(int version, int appId);
        Task<bool> IsFirstRelease(int version);
    }

    public class ReleaseHelper : IReleaseHelper
    {
        private CurrentUser _currentUser;
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;
        private IHttpContextAccessor _context;
        private IUnifiedStorage _storage;

        public ReleaseHelper(IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            IHttpContextAccessor context,
            IUnifiedStorage storage)
        {
            _storage = storage;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _context = context;
            _currentUser = UserHelper.GetCurrentUser(_context);
        }

        public async Task All(int appId, bool clearAllRecords, string dbName, string version, int deploymentId, List<HistoryStorage> historyStorages)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var studioDbContext = _scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                var platformDbContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();

                using (var platformRepository = new PlatformRepository(platformDbContext, _configuration))
                    //using (var deploymentRepository = new DeploymentRepository(studioDbContext, _configuration))
                using (var appDraftRepository = new AppDraftRepository(studioDbContext, _configuration))
                {
                    platformRepository.CurrentUser = appDraftRepository.CurrentUser = _currentUser;

                    var studioClientId = _configuration.GetValue("AppSettings:ClientId", string.Empty);

                    var app = await appDraftRepository.Get(appId);
                    var studioApp = await platformRepository.AppGetByName(studioClientId);

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

                    var result = await Model.Helpers.ReleaseHelper.All(JObject.Parse(appString), CryptoHelper.Decrypt(studioApp.Secret), clearAllRecords, dbName, version, _configuration, _storage, historyStorages);

                    /*var deployment = await deploymentRepository.Get(deploymentId);

                    if (deployment != null)
                    {
                        deployment.Status = result ? DeploymentStatus.Succeed : DeploymentStatus.Failed;
                        deployment.EndTime = DateTime.Now;

                        await deploymentRepository.Update(deployment);
                    }*/
                }
            }
        }

        public async Task Diffs(List<HistoryDatabase> historyDatabases, List<HistoryStorage> historyStorages, int appId, string dbName, string version, int deploymentId)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var studioDbContext = _scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                var platformDbContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();

                using (var platformRepository = new PlatformRepository(platformDbContext, _configuration))
                    //using (var deploymentRepository = new DeploymentRepository(studioDbContext, _configuration))
                using (var appDraftRepository = new AppDraftRepository(studioDbContext, _configuration))
                {
                    platformRepository.CurrentUser = appDraftRepository.CurrentUser = _currentUser;

                    var studioClientId = _configuration.GetValue("AppSettings:ClientId", string.Empty);

                    var app = await appDraftRepository.Get(appId);
                    var studioApp = await platformRepository.AppGetByName(studioClientId);

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

                    //var missingScripts = await CheckMissingScripts(version, appId);
                    //var missingFiles = await CheckMissingFiles(version, appId);


                    var result = await Model.Helpers.ReleaseHelper.Diffs(historyDatabases, historyStorages, JObject.Parse(appString), CryptoHelper.Decrypt(studioApp.Secret), dbName, version, deploymentId, _configuration, _storage);

                    /*var deployment = await deploymentRepository.Get(deploymentId);

                    if (deployment != null)
                    {
                        deployment.Status = result ? DeploymentStatus.Succeed : DeploymentStatus.Failed;
                        deployment.EndTime = DateTime.Now;

                        await deploymentRepository.Update(deployment);
                    }*/
                }
            }
        }


        public async Task<List<string>> CheckMissingFiles(int version, int appId)
        {
            var endLoop = false;
            var scripts = new List<string>();
            var PREConnectionString = _configuration.GetConnectionString("PlatformDBConnection");

            while (!endLoop)
            {
                version = version - 1;

                try
                {
                    var query = $"SELECT * FROM \"public\".history_storages WHERE \"public\".history_storages.tag = '{version.ToString()}'";
                    var result = PosgresHelper.Read(PREConnectionString, $"app{appId}", query);

                    if (result == null)
                    {
                        var bucketName = UnifiedStorage.GetPath("releases", "app", appId, version + "/");


                        //var objects = await _storage.GetObject(bucketName, "scripts.txt");

                        using (var response = await _storage.GetObject(bucketName, "scripts.txt"))
                        using (var responseStream = response.ResponseStream)
                        using (var reader = new StreamReader(responseStream))
                        {
                            var script = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(script))
                                scripts.Add(script);
                        }
                    }
                    else
                        endLoop = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            return scripts;
        }

        public async Task<List<JObject>> CheckMissingScripts(int version, int appId)
        {
            var endLoop = false;
            var scripts = new List<JObject>();
            var PREConnectionString = _configuration.GetConnectionString("PlatformDBConnection");

            while (!endLoop)
            {
                version = version - 1;

                var query = $"SELECT * FROM \"public\".history_database WHERE \"public\".history_database.tag = '{version.ToString()}'";
                var result = PosgresHelper.Read(PREConnectionString, $"app{appId}", query);

                if (result == null)
                {
                    var bucketName = UnifiedStorage.GetPath("releases", "app", appId, version + "/");
                    //var objects = await _storage.GetObject(bucketName, "scripts.txt");
                    using (var response = await _storage.GetObject(bucketName, "scripts.txt"))
                    using (var responseStream = response.ResponseStream)
                    using (var reader = new StreamReader(responseStream))
                    {
                        var script = reader.ReadToEnd();
                        if (!string.IsNullOrEmpty(script))
                        {
                            scripts.Add(new JObject {["is_dump"] = false, ["script"] = script});


                            var dumpExists = await _storage.ObjectExists(bucketName, $"app{appId}.dmp");
                            if (dumpExists)
                            {
                                using (var dumpResponse = await _storage.GetObject(bucketName, $"app{appId}.dmp"))
                                using (var dumpResponseStream = dumpResponse.ResponseStream)
                                using (var dumpReader = new StreamReader(dumpResponseStream))
                                {
                                    var dump = dumpReader.ReadToEnd();
                                    scripts.Add(new JObject {["is_dump"] = true, ["script"] = dump});
                                    endLoop = true;
                                }
                            }
                        }
                    }
                }
                else
                    endLoop = true;
            }

            return scripts;
        }

        public async Task<bool> IsFirstRelease(int version)
        {
            version = version - 1;

            if (version == 0)
                return true;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var studioDbContext = scope.ServiceProvider.GetRequiredService<StudioDBContext>();

                using (var packageRepository = new PackageRepository(studioDbContext, _configuration))
                {
                    var release = await packageRepository.GetByVersion(version);

                    if (release == null)
                        return true;

                    if (release.Status == ReleaseStatus.Succeed)
                        return false;

                    return await IsFirstRelease(version);
                }
            }
        }
    }
}