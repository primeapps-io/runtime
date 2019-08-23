using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Util.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Admin.Helpers
{
    public interface IPublishHelper
    {
        Task<bool> StartRelease(int appId);
        Task<bool> CheckDatabaseTemplate(int appId);
    }

    public class PublishHelper : IPublishHelper
    {
        private readonly string _organizationRedisKey = "primeapps_admin_organizations";
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IRedisHelper _redisHelper;
        private readonly IUnifiedStorage _storage;

        public PublishHelper(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, IRedisHelper redisHelper, IUnifiedStorage storage)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
            _redisHelper = redisHelper;
            _storage = storage;
        }

        public async Task<bool> StartRelease(int appId)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var platformDbContext = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                var studioContext = scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                using (var packageRepository = new PackageRepository(studioContext, _configuration))
                using (var appDraftRepository = new AppDraftRepository(studioContext, _configuration))
                using (var releaseRepository = new ReleaseRepository(platformDbContext, _configuration))
                {
                    var isThereTempDatabase = await CheckDatabaseTemplate(appId);

                    var packages = await packageRepository.GetAll(appId);
                    var app = await appDraftRepository.Get(appId);
                    var appJObject = JObject.FromObject(app);
                    var appName = $"app{appId}";
                    var lastPackageVersion = Convert.ToInt32(packages.Last().Version.ToString());
                     
                    var releaseModel = new Release()
                    {
                        AppId = appId,
                        StartTime = DateTime.Now,
                        Status = Model.Enums.ReleaseStatus.Running,
                        Version = lastPackageVersion.ToString(),
                        //Settings=""
                    };


                    if (!isThereTempDatabase)//Temp Update
                    {
                        var runningPublish = await releaseRepository.IsThereRunningProcess(appId);

                        if (runningPublish)
                            throw new Exception($"Already have a running publish for app{appId} database!");

                        var lastReleaseVersion = await releaseRepository.GetLastVersion(appId);

                        if (lastReleaseVersion > 0)
                        {
                            var currentPackages = new List<Package>();
                            for (var a = lastReleaseVersion + 1; lastPackageVersion < a; a++)
                            {
                                var package = packages.Where(x => x.Version == a.ToString()).FirstOrDefault();

                                if (package != null)
                                    currentPackages.Add(package);
                            }

                            var releaseResult = await releaseRepository.Create(releaseModel);

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

                                releaseResult = await releaseRepository.Update(releaseModel); 
                            }
                            else
                                throw new Exception($"Release record cannot create for app{appId} database!"); 
                        }
                        else
                        {
                            //TODO  Error case. if there, it have to rollback for database template
                        }
                    }
                    else //Temp Create
                    { 

                        var releaseResult = await releaseRepository.Create(releaseModel);

                        if (releaseResult > 0)
                        {
                            //TODO 
                            var releaseProcess = await Model.Helpers.PublishHelper.Create(_configuration, _storage, appJObject, appName, lastPackageVersion, "", true);
                            //TODO
                            releaseModel.EndTime = DateTime.Now;

                            if (releaseProcess)
                                releaseModel.Status = Model.Enums.ReleaseStatus.Succeed;
                            else
                            {
                                //TODO
                                //If false, does the generated theme need to be deleted?
                                releaseModel.Status = Model.Enums.ReleaseStatus.Failed;

                            }

                            releaseResult = await releaseRepository.Update(releaseModel); 
                        }
                        else
                            throw new Exception($"Release record cannot create for app{appId} database!");
                    }
                }
            }

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
