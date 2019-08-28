using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Util.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Admin.Helpers
{
    public interface IPublishHelper
    {
        Task<bool> StartRelease(int appId, int orgId, string applyingVersion, string token);
        Task<bool> CheckDatabaseTemplate(int appId);
    }

    public class PublishHelper : IPublishHelper
    {
        private readonly string _organizationRedisKey = "primeapps_admin_organizations";
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IRedisHelper _redisHelper;
        private readonly IUnifiedStorage _storage;
        private readonly IPlatformRepository _platformRepository;
        private readonly IReleaseRepository _releaseRepository;

        public PublishHelper(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, IRedisHelper redisHelper, IUnifiedStorage storage,
            IPlatformRepository platformRepository, IReleaseRepository releaseRepository)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
            _redisHelper = redisHelper;
            _storage = storage;
            _platformRepository = platformRepository;
            _releaseRepository = releaseRepository;
        }

        public async Task<bool> StartRelease(int appId, int orgId, string applyingVersion, string token)
        {
            var studioClient = new StudioClient(_configuration, token, appId, orgId);
            var isThereTempDatabase = await CheckDatabaseTemplate(appId);

            var packages = await studioClient.PackageGetAll();
            var app = await studioClient.AppDraftGetById(appId);

            var appJObject = JObject.FromObject(app);
            var appName = $"app{appId}";
            var lastPackageVersion = packages.First().Version.ToString();
            var versions = new List<string>() { applyingVersion };

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

            var releaseModel = new Release()
            {
                AppId = appId,
                StartTime = DateTime.Now,
                Status = Model.Enums.ReleaseStatus.Running,
                Version = lastPackageVersion,
                //Settings=""
            };

            if (!isThereTempDatabase)//Temp Update
            {
                var runningPublish = await _releaseRepository.IsThereRunningProcess(appId);

                if (runningPublish)
                    throw new Exception($"Already have a running publish for app{appId} database!");

                foreach (var package in packages)
                {
                    if (package.Version != applyingVersion)
                    {
                        var release = await _releaseRepository.Get(appId, package.Version);
                        if (release == null)
                            versions.Add(package.Version);
                        else
                            break;
                    }
                }

                var releaseResult = await _releaseRepository.Create(releaseModel);

                if (releaseResult > 0)
                {
                    //TODO Method parameters have to change for current packages.
                    var releaseProcess = true;// await Model.Helpers.PublishHelper.Update(_configuration, _storage, appJObject, appName, currentPackages, false);

                    if (releaseProcess)
                        releaseModel.Status = Model.Enums.ReleaseStatus.Succeed;
                    else
                    {
                        //TODO
                        //If false, does the generated theme need to be deleted?
                        releaseModel.Status = Model.Enums.ReleaseStatus.Failed;

                    }

                    releaseResult = await _releaseRepository.Update(releaseModel);
                }
                else
                    throw new Exception($"Release record cannot create for app{appId} database!");

            }
            else //Temp Create
            {
                var releaseResult = await _releaseRepository.Create(releaseModel);

                if (releaseResult > 0)
                {
                    //TODO 
                    //var releaseProcess = await Model.Helpers.PublishHelper.Create(_configuration, _storage, appJObject, appName, lastPackageVersion, "", true);
                    //TODO
                    releaseModel.EndTime = DateTime.Now;

                    //if (releaseProcess)
                    //    releaseModel.Status = Model.Enums.ReleaseStatus.Succeed;
                    //else
                    //{
                    //TODO
                    //If false, does the generated theme need to be deleted?
                    //    releaseModel.Status = Model.Enums.ReleaseStatus.Failed;

                    //}

                    releaseResult = await _releaseRepository.Update(releaseModel);
                }
                else
                    throw new Exception($"Release record cannot create for app{appId} database!");
            }

            var studioClientId = _configuration.GetValue("AppSettings:ClientId", string.Empty);
            var studioApp = await _platformRepository.AppGetByName(studioClientId);

            await PrimeApps.Model.Helpers.PublishHelper.ApplyVersions(_configuration, _storage, JObject.Parse(appString), app.OrganizationId, $"app{appId}", versions, CryptoHelper.Decrypt(studioApp.Secret), true, 1, token);

            return false;
        }

        public async Task<bool> CheckDatabaseTemplate(int appId)
        {
            var PREConnectionString = _configuration.GetConnectionString("PlatformDBConnection");
            var appName = $"app{appId}";

            var query = $"SELECT datname FROM pg_database WHERE datname='{appName}'";
            var result = PosgresHelper.Read(PREConnectionString, null, query);

            if (result == null)
                return false;

            return true;
        }
    }
}
