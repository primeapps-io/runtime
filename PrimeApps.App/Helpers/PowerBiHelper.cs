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
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;
using Microsoft.PowerBI.Api.V1;
using Microsoft.PowerBI.Api.V1.Models;

namespace PrimeApps.App.Helpers
{
    public class PowerBiHelper
    {
        private static string _powerBiApiEndpoint = "https://api.powerbi.com";
        private static string _powerBiEmbedUrl = "https://embedded.powerbi.com/appTokenReportEmbed?reportId={0}";

        public static async Task<Microsoft.PowerBI.Api.V1.Models.Workspace> CreateWorkspace(IConfiguration configuration)
        {
            using (var client = CreateClient(configuration))
            {
                return await client.Workspaces.PostWorkspaceAsync(configuration.GetSection("AppSettings")["PowerbiWorkspaceCollection"]);
            }
        }

        public static async Task<Microsoft.PowerBI.Api.V1.Models.Report> GetReportByName(int tenantId, string name, IConfiguration configuration)
        {
            Model.Entities.Platform.PlatformWarehouse warehouse;
            using (PlatformDBContext dbContext = new PlatformDBContext(configuration))
            {
                using (PlatformWarehouseRepository warehouseRepo = new PlatformWarehouseRepository(dbContext, configuration))
                {
                    warehouse = await warehouseRepo.GetByTenantId(tenantId);
                }
            }

            using (var client = CreateClient(configuration))
            {
                var reportsResponse = await client.Reports.GetReportsAsync(configuration.GetSection("AppSettings")["PowerbiWorkspaceCollection"], warehouse.PowerbiWorkspaceId);
                var report = reportsResponse.Value.SingleOrDefault(x => x.Name == name);

                return report;
            }
        }

        public static async Task<List<ReportViewModel>> GetReports(int tenantId, ICollection<Analytic> analytics, IConfiguration configuration)
        {
            var reports = new List<ReportViewModel>();
            Model.Entities.Platform.PlatformWarehouse warehouse;

            using (var dbContext = new PlatformDBContext(configuration))
            {
                using (var warehouseRepo = new PlatformWarehouseRepository(dbContext, configuration))
                {
                    warehouse = await warehouseRepo.GetByTenantId(tenantId);

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

                var embedToken = PowerBIToken.CreateReportEmbedToken(configuration.GetSection("AppSettings")["PowerbiWorkspaceCollection"], warehouse.PowerbiWorkspaceId, analytic.PowerBiReportId, TimeSpan.FromDays(15));
                var accessToken = embedToken.Generate(configuration.GetSection("AppSettings")["PowerbiAccessKey"]);

                report.AccessToken = accessToken;
                report.EmbedUrl = string.Format(_powerBiEmbedUrl, analytic.PowerBiReportId);

                reports.Add(report);
            }

            return reports;
        }

        public static async Task<Microsoft.PowerBI.Api.V1.Models.Import> ImportPbix(string pbixUrl, string reportName, int tenantId, IConfiguration configuration)
        {
            Model.Entities.Platform.PlatformWarehouse warehouse;
            using (PlatformDBContext dbContext = new PlatformDBContext(configuration))
            {
                using (PlatformWarehouseRepository warehouseRepo = new PlatformWarehouseRepository(dbContext, configuration))
                {
                    warehouse = await warehouseRepo.GetByTenantId(tenantId);
                }
            }

            using (var webClient = new WebClient())
            {
                using (var fileStream = webClient.OpenRead(pbixUrl))
                {
                    using (var client = CreateClient(configuration))
                    {
                        client.HttpClient.Timeout = TimeSpan.FromMinutes(5);
                        client.HttpClient.DefaultRequestHeaders.Add("ActivityId", Guid.NewGuid().ToString());

                        var import = client.Imports.PostImportWithFile(configuration.GetSection("AppSettings")["PowerbiWorkspaceCollection"], warehouse.PowerbiWorkspaceId, fileStream, reportName);

                        return import;
                    }
                }
            }
        }

        public static async Task UpdateConnectionString(int analyticId, int tenantId, IConfiguration configuration)
        {
            Model.Entities.Platform.PlatformWarehouse warehouse;
            using (PlatformDBContext dbContext = new PlatformDBContext(configuration))
            {
                using (PlatformWarehouseRepository warehouseRepo = new PlatformWarehouseRepository(dbContext, configuration))
                {
                    warehouse = await warehouseRepo.GetByTenantId(tenantId);
                }
            }

            using (var client = CreateClient(configuration))
            {
                var workspaceCollection = configuration.GetSection("AppSettings")["PowerbiWorkspaceCollection"];
                var datasets = await client.Datasets.GetDatasetsAsync(workspaceCollection, warehouse.PowerbiWorkspaceId);
                var dataset = datasets.Value.Single(x => x.Name == analyticId.ToString());
                var datasources = await client.Datasets.GetGatewayDatasourcesAsync(workspaceCollection, warehouse.PowerbiWorkspaceId, dataset.Id);

                var delta = new GatewayDatasource
                {
                    CredentialType = "Basic",
                    BasicCredentials = new BasicCredentials
                    {
                        Username = configuration.GetSection("AppSettings")["WarehouseMasterUser"],
                        Password = configuration.GetSection("AppSettings")["WarehouseMasterPassword"]
                    }
                };

                await client.Gateways.PatchDatasourceAsync(workspaceCollection, warehouse.PowerbiWorkspaceId, datasources.Value[0].GatewayId, datasources.Value[0].Id, delta);
            }
        }

        public static async Task DeleteReport(int tenantId, int analyticId, IConfiguration configuration)
        {
            Model.Entities.Platform.PlatformWarehouse warehouse;
            using (PlatformDBContext dbContext = new PlatformDBContext(configuration))
            {
                using (PlatformWarehouseRepository warehouseRepo = new PlatformWarehouseRepository(dbContext, configuration))
                {
                    warehouse = await warehouseRepo.GetByTenantId(tenantId);
                }
            }

            using (var client = CreateClient(configuration))
            {
                var workspaceCollection = configuration.GetSection("AppSettings")["PowerbiWorkspaceCollection"];
                var datasets = await client.Datasets.GetDatasetsAsync(workspaceCollection, warehouse.PowerbiWorkspaceId);
                var reports = datasets.Value.Where(x => x.Name == analyticId.ToString());

                foreach (var report in reports)
                {
                    var result = await client.Datasets.DeleteDatasetByIdAsync(workspaceCollection, warehouse.PowerbiWorkspaceId, report.Id);
                }
            }
        }

        private static PowerBIClient CreateClient(IConfiguration configuration)
        {
            var credentials = new TokenCredentials(configuration.GetSection("AppSettings")["PowerbiAccessKey"], "AppKey");
            var client = new PowerBIClient(credentials)
            {
                BaseUri = new Uri(_powerBiApiEndpoint)
            };

            return client;
        }
    }
}