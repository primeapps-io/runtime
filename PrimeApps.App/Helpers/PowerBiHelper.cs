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
				var powerbiWorkspaceCollection = _configuration.GetValue("AppSettings:PowerbiWorkspaceCollection", string.Empty);
				if (!string.IsNullOrEmpty(powerbiWorkspaceCollection))
				{
					return await client.Workspaces.PostWorkspaceAsync(powerbiWorkspaceCollection);
				}
				else
					return null;
			}
		}

		public async Task<Microsoft.PowerBI.Api.V1.Models.Report> GetReportByName(UserItem appUser, string name)
		{
			var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
			previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

			Model.Entities.Platform.PlatformWarehouse warehouse;
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
				var cacheHelper = _scope.ServiceProvider.GetRequiredService<CacheHelper>();

				using (var _warehouseRepo = new PlatformWarehouseRepository(platformDatabaseContext, _configuration, cacheHelper))
				{

					_warehouseRepo.CurrentUser = new CurrentUser { TenantId = previewMode == "app" ? appUser.AppId : appUser.TenantId, UserId = appUser.Id, PreviewMode = previewMode };

					warehouse = await _warehouseRepo.GetByTenantId(appUser.TenantId);
				}
			}

			using (var client = CreateClient())
			{
				var powerbiWorkspaceCollection = _configuration.GetValue("AppSettings:PowerbiWorkspaceCollection", string.Empty);
				var reportsResponse = new ODataResponseListReport();
				if (!string.IsNullOrEmpty(powerbiWorkspaceCollection))
				{
					reportsResponse = await client.Reports.GetReportsAsync(powerbiWorkspaceCollection, warehouse.PowerbiWorkspaceId);
				}
				var report = reportsResponse.Value.SingleOrDefault(x => x.Name == name);

				return report;
			}
		}

		public async Task<List<ReportViewModel>> GetReports(UserItem appUser, ICollection<Analytic> analytics)
		{
			var reports = new List<ReportViewModel>();
			Model.Entities.Platform.PlatformWarehouse warehouse;
			var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
			previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
				var cacheHelper = _scope.ServiceProvider.GetRequiredService<CacheHelper>();

				using (var _warehouseRepo = new PlatformWarehouseRepository(platformDatabaseContext, _configuration, cacheHelper))
				{

					_warehouseRepo.CurrentUser = new CurrentUser { TenantId = previewMode == "app" ? appUser.AppId : appUser.TenantId, UserId = appUser.Id, PreviewMode = previewMode };

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

				var powerbiWorkspaceCollection = _configuration.GetValue("AppSettings:PowerbiWorkspaceCollection", string.Empty);
				var embedToken = new PowerBIToken();
				if (!string.IsNullOrEmpty(powerbiWorkspaceCollection))
				{
					embedToken = PowerBIToken.CreateReportEmbedToken(powerbiWorkspaceCollection, warehouse.PowerbiWorkspaceId, analytic.PowerBiReportId, TimeSpan.FromDays(15));
				}
				var powerbiAccessKey = _configuration.GetValue("AppSettings:PowerbiAccessKey", string.Empty);
				var accessToken = "";
				if (!string.IsNullOrEmpty(powerbiAccessKey))
				{
					accessToken = embedToken.Generate(powerbiAccessKey);
				}
				report.AccessToken = accessToken;
				report.EmbedUrl = string.Format(_powerBiEmbedUrl, analytic.PowerBiReportId);

				reports.Add(report);
			}

			return reports;
		}

		public async Task<Microsoft.PowerBI.Api.V1.Models.Import> ImportPbix(string pbixUrl, string reportName, UserItem appUser)
		{
			Model.Entities.Platform.PlatformWarehouse warehouse;
			var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
			previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
				var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();

				using (var _warehouseRepo = new PlatformWarehouseRepository(platformDatabaseContext, _configuration, cacheHelper))
				{

					_warehouseRepo.CurrentUser = new CurrentUser { TenantId = previewMode == "app" ? appUser.AppId : appUser.TenantId, UserId = appUser.Id, PreviewMode = previewMode };

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
						var powerbiWorkspaceCollection = _configuration.GetValue("AppSettings:PowerbiWorkspaceCollection", string.Empty);
						var import = new Microsoft.PowerBI.Api.V1.Models.Import();
						if (!string.IsNullOrEmpty(powerbiWorkspaceCollection))
						{
							import = client.Imports.PostImportWithFile(powerbiWorkspaceCollection, warehouse.PowerbiWorkspaceId, fileStream, reportName);
						}
						return import;
					}
				}
			}
		}

		public async Task UpdateConnectionString(int analyticId, UserItem appUser)
		{
			Model.Entities.Platform.PlatformWarehouse warehouse;
			var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
			previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
				var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();

				using (var _warehouseRepo = new PlatformWarehouseRepository(platformDatabaseContext, _configuration, cacheHelper))
				{

					_warehouseRepo.CurrentUser = new CurrentUser { TenantId = previewMode == "app" ? appUser.AppId : appUser.TenantId, UserId = appUser.Id, PreviewMode = previewMode };

					warehouse = await _warehouseRepo.GetByTenantId(appUser.TenantId);
				}
			}

			using (var client = CreateClient())
			{
				var workspaceCollection = _configuration.GetValue("AppSettings:PowerbiWorkspaceCollection", string.Empty);
				if (!string.IsNullOrEmpty(workspaceCollection))
				{
					var datasets = await client.Datasets.GetDatasetsAsync(workspaceCollection, warehouse.PowerbiWorkspaceId);
					var dataset = datasets.Value.Single(x => x.Name == analyticId.ToString());
					var datasources = await client.Datasets.GetGatewayDatasourcesAsync(workspaceCollection, warehouse.PowerbiWorkspaceId, dataset.Id);

					var warehouseMasterUser = _configuration.GetValue("AppSettings:WarehouseMasterUser", string.Empty);
					var warehouseMasterPassword = _configuration.GetValue("AppSettings:WarehouseMasterPassword", string.Empty);

					var delta = new GatewayDatasource
					{
						CredentialType = "Basic",
						BasicCredentials = new BasicCredentials
						{
							Username = !string.IsNullOrEmpty(warehouseMasterUser) ? warehouseMasterUser : "",
							Password = !string.IsNullOrEmpty(warehouseMasterPassword) ? warehouseMasterPassword : ""
						}
					};

					await client.Gateways.PatchDatasourceAsync(workspaceCollection, warehouse.PowerbiWorkspaceId, datasources.Value[0].GatewayId, datasources.Value[0].Id, delta);
				}
			}
		}

		public async Task DeleteReport(UserItem appUser, int analyticId)
		{
			var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
			previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

			Model.Entities.Platform.PlatformWarehouse warehouse;

			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
				var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();

				using (var _warehouseRepo = new PlatformWarehouseRepository(platformDatabaseContext, _configuration, cacheHelper))
				{

					_warehouseRepo.CurrentUser = new CurrentUser { TenantId = previewMode == "app" ? appUser.AppId : appUser.TenantId, UserId = appUser.Id, PreviewMode = previewMode };

					warehouse = await _warehouseRepo.GetByTenantId(appUser.TenantId);
				}
			}

			using (var client = CreateClient())
			{
				var workspaceCollection = _configuration.GetValue("AppSettings:PowerbiWorkspaceCollection", string.Empty);
				if (!string.IsNullOrEmpty(workspaceCollection))
				{
					var datasets = await client.Datasets.GetDatasetsAsync(workspaceCollection, warehouse.PowerbiWorkspaceId);
					var reports = datasets.Value.Where(x => x.Name == analyticId.ToString());

					foreach (var report in reports)
					{
						var result = await client.Datasets.DeleteDatasetByIdAsync(workspaceCollection, warehouse.PowerbiWorkspaceId, report.Id);
					}
				}
			}
		}

		private PowerBIClient CreateClient()
		{
			var powerbiAccessKey = _configuration.GetValue("AppSettings:PowerbiAccessKey", string.Empty);
			var credentials = new TokenCredentials(null, null);
			if (!string.IsNullOrEmpty(powerbiAccessKey))
			{
				credentials = new TokenCredentials(powerbiAccessKey, "AppKey");
			}
			var client = new PowerBIClient(credentials)
			{
				BaseUri = new Uri(_powerBiApiEndpoint)
			};

			return client;
		}
	}
}