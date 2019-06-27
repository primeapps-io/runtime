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
        Task All(int appId, bool clearAllRecords, bool autoDistribute, string dbName, int version, int deploymentId);

        Task Diffs(List<HistoryDatabase> historyDatabases, List<HistoryStorage> historyStorages, int appId, bool goLive, string dbName, int version, int deploymentId);
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

        public async Task All(int appId, bool clearAllRecords, bool autoDistribute, string dbName, int version, int deploymentId)
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

                    var result = await Model.Helpers.ReleaseHelper.All(JObject.Parse(appString), CryptoHelper.Decrypt(studioApp.Secret), clearAllRecords, autoDistribute, dbName, version, _configuration, _storage);

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

        public async Task Diffs(List<HistoryDatabase> historyDatabases, List<HistoryStorage> historyStorages, int appId, bool goLive, string dbName, int version, int deploymentId)
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

                    var result = await Model.Helpers.ReleaseHelper.Diffs(historyDatabases, historyStorages, JObject.Parse(appString), CryptoHelper.Decrypt(studioApp.Secret), goLive, dbName, version, deploymentId, _configuration, _storage);

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
    }
}