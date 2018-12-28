using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Rest;
using System.Linq;
using PrimeApps.App.Models.ViewModel.Analytics;
using System.Net;
using Hangfire;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;
using Microsoft.PowerBI.Api.V1;
using Microsoft.PowerBI.Api.V1.Models;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Common.Cache;

namespace PrimeApps.App.Helpers
{
    public interface IPowerBiHelper
    {
        Task<Microsoft.PowerBI.Api.V1.Models.Workspace> CreateWorkspace();
        Task<Microsoft.PowerBI.Api.V1.Models.Report> GetReportByName(UserItem appUser, string name);
        Task<List<ReportViewModel>> GetReports(UserItem appUser, ICollection<Analytic> analytics);
        Task<Microsoft.PowerBI.Api.V1.Models.Import> ImportPbix(string pbixUrl, string reportName, UserItem appUser);
        Task UpdateConnectionString(int analyticId, UserItem appUser);
        Task DeleteReport(UserItem appUser, int analyticId);
    }

    public class PowerBiHelper : IPowerBiHelper
    {
        private IServiceScopeFactory _serviceScopeFactory;
        private IConfiguration _configuration;

        private static string _powerBiApiEndpoint = "https://api.powerbi.com";
        private static string _powerBiEmbedUrl = "https://embedded.powerbi.com/appTokenReportEmbed?reportId={0}";

        public PowerBiHelper(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<Microsoft.PowerBI.Api.V1.Models.Workspace> CreateWorkspace()
        {
            using (var client = CreateClient())
            {
                return await client.Workspaces.PostWorkspaceAsync(_configuration.GetSection("AppSettings")["PowerbiWorkspaceCollection"]);
            }
        }

        public async Task<Microsoft.PowerBI.Api.V1.Models.Report> GetReportByName(UserItem appUser, string name)
        {
            Model.Entities.Platform.PlatformWarehouse warehouse;
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                var cacheHelper = _scope.ServiceProvider.GetRequiredService<CacheHelper>();

                using (var _warehouseRepo = new PlatformWarehouseRepository(platformDatabaseContext, _configuration, cacheHelper))
                {
                    _warehouseRepo.CurrentUser = new CurrentUser { TenantId = appUser.TenantId, UserId = appUser.Id };
                    warehouse = await _warehouseRepo.GetByTenantId(appUser.TenantId);
                }
            }

            using (var client = CreateClient())
            {
                var reportsResponse = await client.Reports.GetReportsAsync(_configuration.GetSection("AppSettings")["PowerbiWorkspaceCollection"], warehouse.PowerbiWorkspaceId);
                var report = reportsResponse.Value.SingleOrDefault(x => x.Name == name);

                return report;
            }
        }

        public async Task<List<ReportViewModel>> GetReports(UserItem appUser, ICollection<Analytic> analytics)
        {
            var reports = new List<ReportViewModel>();
            Model.Entities.Platform.PlatformWarehouse warehouse;

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                var cacheHelper = _scope.ServiceProvider.GetRequiredService<CacheHelper>();

                using (var _warehouseRepo = new PlatformWarehouseRepository(platformDatabaseContext, _configuration, cacheHelper))
                {
                    _warehouseRepo.CurrentUser = new CurrentUser { TenantId = appUser.TenantId, UserId = appUser.Id };
                    warehouse = await _warehouseRepo.GetByTenantId(appUser.TenantId);

                    if (warehouse == null)
                        return null;
                }
            }


            foreach (var analytic in analytics)
            {
                if (string.IsNullOrEmpty(analytic.PowerBiReportId))
                    continue;

                var report = new ReportViewModel();
                report.Id = analytic.Id;
                report.Label = analytic.Label;
                report.ReportId = analytic.PowerBiReportId;
                report.MenuIcon = analytic.MenuIcon;

                var embedToken = PowerBIToken.CreateReportEmbedToken(_configuration.GetSection("AppSettings")["PowerbiWorkspaceCollection"], warehouse.PowerbiWorkspaceId, analytic.PowerBiReportId, TimeSpan.FromDays(15));
                var accessToken = embedToken.Generate(_configuration.GetSection("AppSettings")["PowerbiAccessKey"]);

                report.AccessToken = accessToken;
                report.EmbedUrl = string.Format(_powerBiEmbedUrl, analytic.PowerBiReportId);

                reports.Add(report);
            }

            return reports;
        }

        public async Task<Microsoft.PowerBI.Api.V1.Models.Import> ImportPbix(string pbixUrl, string reportName, UserItem appUser)
        {
            Model.Entities.Platform.PlatformWarehouse warehouse;

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();

                using (var _warehouseRepo = new PlatformWarehouseRepository(platformDatabaseContext, _configuration, cacheHelper))
                {
                    _warehouseRepo.CurrentUser = new CurrentUser { TenantId = appUser.TenantId, UserId = appUser.Id };
                    warehouse = await _warehouseRepo.GetByTenantId(appUser.TenantId);
                }
            }

            using (var webClient = new WebClient())
            {
                using (var fileStream = webClient.OpenRead(pbixUrl))
                {
                    using (var client = CreateClient())
                    {
                        client.HttpClient.Timeout = TimeSpan.FromMinutes(5);
                        client.HttpClient.DefaultRequestHeaders.Add("ActivityId", Guid.NewGuid().ToString());

                        var import = client.Imports.PostImportWithFile(_configuration.GetSection("AppSettings")["PowerbiWorkspaceCollection"], warehouse.PowerbiWorkspaceId, fileStream, reportName);

                        return import;
                    }
                }
            }
        }

        public async Task UpdateConnectionString(int analyticId, UserItem appUser)
        {
            Model.Entities.Platform.PlatformWarehouse warehouse;

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();

                using (var _warehouseRepo = new PlatformWarehouseRepository(platformDatabaseContext, _configuration, cacheHelper))
                {
                    _warehouseRepo.CurrentUser = new CurrentUser { TenantId = appUser.TenantId, UserId = appUser.Id };
                    warehouse = await _warehouseRepo.GetByTenantId(appUser.TenantId);
                }
            }

            using (var client = CreateClient())
            {
                var workspaceCollection = _configuration.GetSection("AppSettings")["PowerbiWorkspaceCollection"];
                var datasets = await client.Datasets.GetDatasetsAsync(workspaceCollection, warehouse.PowerbiWorkspaceId);
                var dataset = datasets.Value.Single(x => x.Name == analyticId.ToString());
                var datasources = await client.Datasets.GetGatewayDatasourcesAsync(workspaceCollection, warehouse.PowerbiWorkspaceId, dataset.Id);

                var delta = new GatewayDatasource
                {
                    CredentialType = "Basic",
                    BasicCredentials = new BasicCredentials
                    {
                        Username = _configuration.GetSection("AppSettings")["WarehouseMasterUser"],
                        Password = _configuration.GetSection("AppSettings")["WarehouseMasterPassword"]
                    }
                };

                await client.Gateways.PatchDatasourceAsync(workspaceCollection, warehouse.PowerbiWorkspaceId, datasources.Value[0].GatewayId, datasources.Value[0].Id, delta);
            }
        }

        public async Task DeleteReport(UserItem appUser, int analyticId)
        {
            Model.Entities.Platform.PlatformWarehouse warehouse;

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();

                using (var _warehouseRepo = new PlatformWarehouseRepository(platformDatabaseContext, _configuration, cacheHelper))
                {
                    _warehouseRepo.CurrentUser = new CurrentUser { TenantId = appUser.TenantId, UserId = appUser.Id };
                    warehouse = await _warehouseRepo.GetByTenantId(appUser.TenantId);
                }
            }

            using (var client = CreateClient())
            {
                var workspaceCollection = _configuration.GetSection("AppSettings")["PowerbiWorkspaceCollection"];
                var datasets = await client.Datasets.GetDatasetsAsync(workspaceCollection, warehouse.PowerbiWorkspaceId);
                var reports = datasets.Value.Where(x => x.Name == analyticId.ToString());

                foreach (var report in reports)
                {
                    var result = await client.Datasets.DeleteDatasetByIdAsync(workspaceCollection, warehouse.PowerbiWorkspaceId, report.Id);
                }
            }
        }

        private PowerBIClient CreateClient()
        {
            var credentials = new TokenCredentials(_configuration.GetSection("AppSettings")["PowerbiAccessKey"], "AppKey");
            var client = new PowerBIClient(credentials)
            {
                BaseUri = new Uri(_powerBiApiEndpoint)
            };

            return client;
        }
    }
}