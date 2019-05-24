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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Common;
using Newtonsoft.Json.Serialization;
using PrimeApps.Studio.Storage;

namespace PrimeApps.Studio.Helpers
{
    public interface IPublishHelper
    {
        Task Create(int appId, bool clearAllRecords, string dbName, int version, int deploymentId);
    }

    public class PublishHelper : IPublishHelper
    {
        private CurrentUser _currentUser;
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;
        private IHttpContextAccessor _context;

        public PublishHelper(IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            IHttpContextAccessor context)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _context = context;
            _currentUser = UserHelper.GetCurrentUser(_context);
        }

        public async Task Create(int appId, bool clearAllRecords, string dbName, int version, int deploymentId)
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
                    var result = await Model.Helpers.PublishHelper.Create(JObject.Parse(appString), CryptoHelper.Decrypt(studioApp.Secret), clearAllRecords, dbName, version, deploymentId, _configuration);
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