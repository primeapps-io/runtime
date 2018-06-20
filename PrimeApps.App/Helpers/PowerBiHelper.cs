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
        private static string _workspaceCollection = ConfigurationManager.AppSettings["PowerbiWorkspaceCollection"];
        private static string _accessKey = ConfigurationManager.AppSettings["PowerbiAccessKey"];
        private static string _powerBiEmbedUrl = "https://embedded.powerbi.com/appTokenReportEmbed?reportId={0}";

        public static async Task<Microsoft.PowerBI.Api.V1.Models.Workspace> CreateWorkspace()
        {
            using (var client = CreateClient())
            {
                return await client.Workspaces.PostWorkspaceAsync(_workspaceCollection);
            }
        }

        public static async Task<Microsoft.PowerBI.Api.V1.Models.Report> GetReportByName(int tenantId, string name)
        {
            Model.Entities.Platform.PlatformWarehouse warehouse;
            using (PlatformDBContext dbContext = new PlatformDBContext())
            {
                using (PlatformWarehouseRepository warehouseRepo = new PlatformWarehouseRepository(dbContext))
                {
                    warehouse = await warehouseRepo.GetByTenantId(tenantId);
                }
            }

            using (var client = CreateClient())
            {
                var reportsResponse = await client.Reports.GetReportsAsync(_workspaceCollection, warehouse.PowerbiWorkspaceId);
                var report = reportsResponse.Value.SingleOrDefault(x => x.Name == name);

                return report;
            }
        }

        public static async Task<List<ReportViewModel>> GetReports(int tenantId, ICollection<Analytic> analytics)
        {
            var reports = new List<ReportViewModel>();
            Model.Entities.Platform.PlatformWarehouse warehouse;

            using (var dbContext = new PlatformDBContext())
            {
                using (var warehouseRepo = new PlatformWarehouseRepository(dbContext))
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

                var embedToken = PowerBIToken.CreateReportEmbedToken(_workspaceCollection, warehouse.PowerbiWorkspaceId, analytic.PowerBiReportId, TimeSpan.FromDays(15));
                var accessToken = embedToken.Generate(_accessKey);

                report.AccessToken = accessToken;
                report.EmbedUrl = string.Format(_powerBiEmbedUrl, analytic.PowerBiReportId);

                reports.Add(report);
            }

            return reports;
        }

        public static async Task<Microsoft.PowerBI.Api.V1.Models.Import> ImportPbix(string pbixUrl, string reportName, int tenantId)
        {
            Model.Entities.Platform.PlatformWarehouse warehouse;
            using (PlatformDBContext dbContext = new PlatformDBContext())
            {
                using (PlatformWarehouseRepository warehouseRepo = new PlatformWarehouseRepository(dbContext))
                {
                    warehouse = await warehouseRepo.GetByTenantId(tenantId);
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

                        var import = client.Imports.PostImportWithFile(_workspaceCollection, warehouse.PowerbiWorkspaceId, fileStream, reportName);

                        return import;
                    }
                }
            }
        }

        public static async Task UpdateConnectionString(int analyticId, int tenantId)
        {
            Model.Entities.Platform.PlatformWarehouse warehouse;
            using (PlatformDBContext dbContext = new PlatformDBContext())
            {
                using (PlatformWarehouseRepository warehouseRepo = new PlatformWarehouseRepository(dbContext))
                {
                    warehouse = await warehouseRepo.GetByTenantId(tenantId);
                }
            }

            using (var client = CreateClient())
            {
                var datasets = await client.Datasets.GetDatasetsAsync(_workspaceCollection, warehouse.PowerbiWorkspaceId);
                var dataset = datasets.Value.Single(x => x.Name == analyticId.ToString());
                var datasources = await client.Datasets.GetGatewayDatasourcesAsync(_workspaceCollection, warehouse.PowerbiWorkspaceId, dataset.Id);

                var delta = new GatewayDatasource
                {
                    CredentialType = "Basic",
                    BasicCredentials = new BasicCredentials
                    {
                        Username = ConfigurationManager.AppSettings["WarehouseMasterUser"],
                        Password = ConfigurationManager.AppSettings["WarehouseMasterPassword"]
                    }
                };

                await client.Gateways.PatchDatasourceAsync(_workspaceCollection, warehouse.PowerbiWorkspaceId, datasources.Value[0].GatewayId, datasources.Value[0].Id, delta);
            }
        }

        public static async Task DeleteReport(int tenantId, int analyticId)
        {
            Model.Entities.Platform.PlatformWarehouse warehouse;
            using (PlatformDBContext dbContext = new PlatformDBContext())
            {
                using (PlatformWarehouseRepository warehouseRepo = new PlatformWarehouseRepository(dbContext))
                {
                    warehouse = await warehouseRepo.GetByTenantId(tenantId);
                }
            }

            using (var client = CreateClient())
            {
                var datasets = await client.Datasets.GetDatasetsAsync(_workspaceCollection, warehouse.PowerbiWorkspaceId);
                var reports = datasets.Value.Where(x => x.Name == analyticId.ToString());

                foreach (var report in reports)
                {
                    var result = await client.Datasets.DeleteDatasetByIdAsync(_workspaceCollection, warehouse.PowerbiWorkspaceId, report.Id);
                }
            }
        }

        private static PowerBIClient CreateClient()
        {
            var credentials = new TokenCredentials(_accessKey, "AppKey");
            var client = new PowerBIClient(credentials)
            {
                BaseUri = new Uri(_powerBiApiEndpoint)
            };

            return client;
        }
    }
}