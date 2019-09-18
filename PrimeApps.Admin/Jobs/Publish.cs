using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using PrimeApps.Admin.ActionFilters;
using PrimeApps.Admin.Helpers;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Util.Storage;
using PublishHelper = PrimeApps.Model.Helpers.PublishHelper;

namespace PrimeApps.Admin.Jobs
{
    public interface IPublish
    {
        Task PackageApply(int appId, int orgId, string token, int userId, string appUrl, string authUrl, bool useSsl);
        Task UpdateTenants(int appId, int orgId, int userId, IList<int> tenants, string token);
    }

    public class Publish : IPublish
    {
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;
        private readonly IUnifiedStorage _storage;

        public Publish(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, IUnifiedStorage storage)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _storage = storage;
        }

        [QueueCustom]
        public async Task PackageApply(int appId, int orgId, string token, int userId, string appUrl, string authUrl, bool useSsl)
        {
            var studioClient = new StudioClient(_configuration, token, appId, orgId);
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var platformDbContext = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();

                using (var releaseRepository = new ReleaseRepository(platformDbContext, _configuration)) //, cacheHelper))
                using (var platformRepository = new PlatformRepository(platformDbContext, _configuration)) //, cacheHelper))
                using (var applicationRepository = new ApplicationRepository(platformDbContext, _configuration)) //, cacheHelper))
                {
                    applicationRepository.CurrentUser = platformRepository.CurrentUser = releaseRepository.CurrentUser = new CurrentUser {UserId = userId};

                    var app = await studioClient.AppDraftGetById(appId);

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

                    var packages = await studioClient.PackageGetAll();
                    var lastPackageVersion = packages.Last().Version;
                    var versions = new List<string>();

                    foreach (var package in packages)
                    {
                        if (package.Version != lastPackageVersion)
                        {
                            var release = await releaseRepository.Get(appId, package.Version);
                            if (release == null)
                                versions.Add(package.Version);
                            else
                                break;
                        }
                    }

                    versions.Add(lastPackageVersion);

                    var lastRecord = await releaseRepository.GetLast();
                    var firstRelease = await releaseRepository.FirstTime(appId);
                    var platformApp = await applicationRepository.Get(appId);

                    var currentReleaseId = lastRecord?.Id ?? 0;


                    /*   var releaseModel = new Release
                       {
                           Id = currentReleaseId + 1,
                           Status = ReleaseStatus.Running,
                           AppId = appId,
                           Version = ""
                       };
   
                       try
                       {
                           await releaseRepository.Create(releaseModel);
                       }
                       catch (Exception e)
                       {
                           Console.WriteLine(e);
                           throw;
                       }*/


                    var releases = await PublishHelper.ApplyVersions(_configuration, _storage, JObject.Parse(appString), orgId, $"app{appId}", versions, platformApp == null, firstRelease, currentReleaseId, token, appUrl, authUrl, useSsl);

                    foreach (var release in releases)
                    {
                        /* if (release.Id == currentReleaseId + 1)
                         {
                             releaseModel.Version = release.Version;
                             releaseModel.EndTime = release.EndTime;
                             releaseModel.Status = release.Status;
                             await releaseRepository.Update(releaseModel);
                         }
                         else*/

                        await releaseRepository.Create(release);
                    }
                }
            }
        }

        [QueueCustom]
        public async Task UpdateTenants(int appId, int orgId, int userId, IList<int> tenants, string token)
        {
            var studioClient = new StudioClient(_configuration, token, appId, orgId);
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var platformDbContext = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();

                using (var releaseRepository = new ReleaseRepository(platformDbContext, _configuration)) //, cacheHelper))
                using (var platformRepository = new PlatformRepository(platformDbContext, _configuration)) //, cacheHelper))
                {
                    platformRepository.CurrentUser = releaseRepository.CurrentUser = new CurrentUser {UserId = userId};

                    var packages = await studioClient.PackageGetAll();
                    var lastPackageVersion = packages.Last().Version;
                    var versions = new List<string>();


                    var lastRecord = await releaseRepository.GetLast();

                    foreach (var tenantObj in tenants.OfType<object>().Select((id, index) => new {id, index}))
                    {
                        versions = new List<string>();
                        foreach (var package in packages)
                        {
                            if (package.Version != lastPackageVersion)
                            {
                                var release = await releaseRepository.Get(appId, package.Version, int.Parse(tenantObj.id.ToString()));
                                if (release == null)
                                    versions.Add(package.Version);
                            }
                        }

                        versions.Add(lastPackageVersion);

                        foreach (var versionObj in versions.OfType<object>().Select((value, index) => new {value, index}))
                        {
                            var dbName = $"tenant{tenantObj.id}";
                            var version = versionObj.value.ToString();

                            var release = await releaseRepository.Get(appId, version, int.Parse(tenantObj.id.ToString()));

                            if (release == null)
                            {
                                var releaseModel = new Release()
                                {
                                    Status = ReleaseStatus.Running,
                                    TenantId = int.Parse(tenantObj.id.ToString()),
                                    StartTime = DateTime.Now,
                                    Version = version,
                                    AppId = appId
                                };

                                try
                                {
                                    await releaseRepository.Create(releaseModel);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    throw;
                                }

                                var result = await PublishHelper.UpdateTenant(version, dbName, _configuration, _storage, appId, orgId, lastRecord?.Id ?? 0, token);

                                if (result)
                                {
                                    releaseModel.EndTime = DateTime.Now;
                                    releaseModel.Status = ReleaseStatus.Succeed;

                                    await releaseRepository.Update(releaseModel);
                                }
                                else
                                {
                                    releaseModel.EndTime = DateTime.Now;
                                    releaseModel.Status = ReleaseStatus.Failed;

                                    await releaseRepository.Update(releaseModel);
                                    break;
                                }
                            }
                            else
                            {
                                if (release.Status == ReleaseStatus.Failed)
                                {
                                    var result = await PublishHelper.UpdateTenant(version, dbName, _configuration, _storage, appId, orgId, lastRecord?.Id ?? 0, token);

                                    if (result)
                                    {
                                        await releaseRepository.Update(new Release()
                                        {
                                            Id = release.Id,
                                            EndTime = DateTime.Now,
                                            Status = ReleaseStatus.Succeed
                                        });
                                    }
                                    else
                                        break;
                                }
                            }
                        }
                    }

                    var rootPath = _configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);
                    Directory.Delete(Path.Combine(rootPath, "packages", $"app{appId}"), true);
                }
            }
        }
    }
}