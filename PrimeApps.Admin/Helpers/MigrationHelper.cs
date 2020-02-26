using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using PrimeApps.Admin.ActionFilters;
using PrimeApps.Admin.Services;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Storage;
using Sentry;
using Sentry.Protocol;
using PrimeApps.Model.Entities.Studio;

namespace PrimeApps.Admin.Helpers
{
	public interface IMigrationHelper
	{
		Task<bool> AppMigration(string schema, bool isLocal, string ids);
		Task<bool> UpdateTenant(int id, string url, int lastTenantId);
		Task ApplyMigrations(List<int> ids);
		Task UpdateTenantModuleFields(int id);
	}

	public class MigrationHelper : IMigrationHelper
	{
		private CurrentUser _currentUser;
		private IConfiguration _configuration;
		private IUnifiedStorage _storage;
		private ITemplateRepository _templateRepository;
		private IHistoryDatabaseRepository _historyDatabaseRepository;
		private IApplicationRepository _applicationRepository;
		private IReleaseRepository _releaseRepository;
		private IHistoryStorageRepository _historyStorageRepository;
		private ITenantRepository _tenantRepository;
		private IModuleRepository _moduleRepository;
		private ISettingRepository _settingRepository;
		private IServiceScopeFactory _serviceScopeFactory;

		public MigrationHelper(IConfiguration configuration,
			IUnifiedStorage storage,
			ITemplateRepository templateRepository,
			IHistoryDatabaseRepository historyDatabaseRepository,
			IApplicationRepository applicationRepository,
			IReleaseRepository releaseRepository,
			IHistoryStorageRepository historyStorageRepository,
			ITenantRepository tenantRepository,
			IModuleRepository moduleRepository,
			ISettingRepository settingRepository,
			IServiceScopeFactory serviceScopeFactory)
		{
			_storage = storage;
			_configuration = configuration;
			_templateRepository = templateRepository;
			_historyDatabaseRepository = historyDatabaseRepository;
			_applicationRepository = applicationRepository;
			_releaseRepository = releaseRepository;
			_historyStorageRepository = historyStorageRepository;
			_tenantRepository = tenantRepository;
			_moduleRepository = moduleRepository;
			_settingRepository = settingRepository;
			_serviceScopeFactory = serviceScopeFactory;
		}

		[QueueCustom]
		public async Task<bool> AppMigration(string schema, bool isLocal, string ids)
		{
			var PREConnectionString = _configuration.GetConnectionString("PlatformDBConnection");
			var idsArr = ids.Split(",");

			foreach (var id in idsArr)
			{
				var app = await _applicationRepository.Get(int.Parse(id));

				if (app == null)
					return false;

				_currentUser = new CurrentUser { PreviewMode = "app", TenantId = app.Id, UserId = 1 };

				_templateRepository.CurrentUser = _historyStorageRepository.CurrentUser = _historyDatabaseRepository.CurrentUser = _currentUser;

				PostgresHelper.ChangeTemplateDatabaseStatus(PREConnectionString, $"app{app.Id}", true);

				try
				{
					//Bucket yoksa oluşturuyor.
					await _storage.CreateBucketIfNotExists($"app{app.Id}");

					var storageUrl = _configuration.GetValue("AppSettings:StorageUrl", string.Empty);

					//App tablosunda ki logo alanını kontrol ediyor.
					if (!string.IsNullOrEmpty(app.Logo) && app.Logo.Contains("http"))
					{
						var regex = new Regex(@"[\w-]+.(jpg|png|jpeg)");
						var match = regex.Match(app.Logo);
						if (match.Success)
						{
							var webClient = new WebClient();
							var imageBytes = webClient.DownloadData(app.Logo);
							Stream stream = new MemoryStream(imageBytes);

							await _storage.Upload($"app{app.Id}/app_logo", match.Value, stream);
							app.Logo = $"app{app.Id}/app_logo/{match.Value}";
						}
					}

					// Boş olan alanları default değerleriyle dolduruyor.
					if (string.IsNullOrEmpty(app.Setting.AppDomain))
						app.Setting.AppDomain = $"{app.Name}.primeapps.app";

					if (string.IsNullOrEmpty(app.Setting.AuthDomain))
						app.Setting.AuthDomain = "auth.primeapps.io";

					if (string.IsNullOrEmpty(app.Setting.Currency))
						app.Setting.Currency = "USD";

					if (string.IsNullOrEmpty(app.Setting.Culture))
						app.Setting.Culture = "en-US";

					if (string.IsNullOrEmpty(app.Setting.TimeZone))
						app.Setting.TimeZone = "America/New_York";

					if (string.IsNullOrEmpty(app.Setting.Language))
						app.Setting.Language = "en";

					//Auth theme alanını kontrol ediyor.
					if (!string.IsNullOrEmpty(app.Setting.AuthTheme))
					{
						try
						{
							var authTheme = JObject.Parse(app.Setting.AuthTheme);

							if (authTheme["logo"] != null && !string.IsNullOrEmpty(authTheme["logo"].ToString()) && authTheme["logo"].ToString().Contains("http"))
							{
								var regex = new Regex(@"[\w-]+.(jpg|png|jpeg)");
								var match = regex.Match(authTheme["logo"].ToString());
								if (match.Success)
								{
									var webClient = new WebClient();
									var imageBytes = webClient.DownloadData(authTheme["logo"].ToString());
									Stream stream = new MemoryStream(imageBytes);

									await _storage.Upload($"app{app.Id}/app_logo", match.Value, stream);

									var history = new HistoryStorage
									{
										MimeType = GetMimeType(match.Value),
										Path = $"app{app.Id}/app_logo/",
										Operation = "PUT",
										FileName = match.Value,
										UniqueName = match.Value,
										ExecutedAt = DateTime.Now,
										CreatedByEmail = "studio@primeapps.io" ?? ""
									};

									await _historyStorageRepository.Create(history);
									authTheme["logo"] = $"app{app.Id}/app_logo/{match.Value}";
								}
							}

							if (authTheme["favicon"] != null && !string.IsNullOrEmpty(authTheme["favicon"].ToString()) && authTheme["favicon"].ToString().Contains("http"))
							{
								var regex = new Regex(@"[\w-]+.(jpg|png|jpeg|ico)");
								var match = regex.Match(authTheme["favicon"].ToString());
								if (match.Success)
								{
									var webClient = new WebClient();
									var imageBytes = webClient.DownloadData(authTheme["favicon"].ToString());
									Stream stream = new MemoryStream(imageBytes);

									await _storage.Upload($"app{app.Id}/app_logo", match.Value, stream);

									var history = new HistoryStorage
									{
										MimeType = GetMimeType(match.Value),
										Path = $"app{app.Id}/app_logo/",
										Operation = "PUT",
										FileName = match.Value,
										UniqueName = match.Value,
										ExecutedAt = DateTime.Now,
										CreatedByEmail = "studio@primeapps.io" ?? ""
									};

									await _historyStorageRepository.Create(history);
									authTheme["favicon"] = $"app{app.Id}/app_logo/{match.Value}";
								}
							}

							if (authTheme["banner"] != null && !string.IsNullOrEmpty(authTheme["banner"].ToString()) && authTheme["banner"][0]["image"] != null && !string.IsNullOrEmpty(authTheme["banner"][0]["image"].ToString()) && authTheme["banner"][0]["image"].ToString().Contains("http"))
							{
								var regex = new Regex(@"[\w-]+.(jpg|png|jpeg)");
								var match = regex.Match(authTheme["banner"][0]["image"].ToString());
								if (match.Success)
								{
									var webClient = new WebClient();
									var imageBytes = webClient.DownloadData(authTheme["banner"][0]["image"].ToString());
									Stream stream = new MemoryStream(imageBytes);

									await _storage.Upload($"app{app.Id}/app_logo", match.Value, stream);

									var history = new HistoryStorage
									{
										MimeType = GetMimeType(match.Value),
										Path = $"app{app.Id}/app_logo/",
										Operation = "PUT",
										FileName = match.Value,
										UniqueName = match.Value,
										ExecutedAt = DateTime.Now,
										CreatedByEmail = "studio@primeapps.io" ?? ""
									};

									await _historyStorageRepository.Create(history);
									authTheme["banner"][0]["image"] = $"app{app.Id}/app_logo/{match.Value}";
								}
							}

							app.Setting.AuthTheme = JsonConvert.SerializeObject(authTheme);
						}
						catch (Exception e)
						{
							SentrySdk.CaptureException(e);
							//ErrorHandler.LogError(e, $"Migration auth theme error. App Id : {app.Id}");
						}
					}
					else
					{
						app.Setting.AuthTheme = new JObject()
						{
							["color"] = "#555198",
							["title"] = "PrimeApps",
							["banner"] = new JArray { new JObject { ["image"] = "", ["descriptions"] = "" } },
						}.ToJsonString();
					}

					//App theme alanı kontrol ediliyor.
					if (!string.IsNullOrEmpty(app.Setting.AppTheme))
					{
						try
						{
							var appTheme = JObject.Parse(app.Setting.AppTheme);

							if (appTheme["logo"] != null && !string.IsNullOrEmpty(appTheme["logo"].ToString()) && appTheme["logo"].ToString().Contains("http"))
							{
								var regex = new Regex(@"[\w-]+.(jpg|png|jpeg)");
								var match = regex.Match(appTheme["logo"].ToString());
								if (match.Success)
								{
									var webClient = new WebClient();
									var imageBytes = webClient.DownloadData(appTheme["logo"].ToString());
									Stream stream = new MemoryStream(imageBytes);

									await _storage.Upload($"app{app.Id}/app_logo", match.Value, stream);

									var history = new HistoryStorage
									{
										MimeType = GetMimeType(match.Value),
										Path = $"app{app.Id}/app_logo/",
										Operation = "PUT",
										FileName = match.Value,
										UniqueName = match.Value,
										ExecutedAt = DateTime.Now,
										CreatedByEmail = "studio@primeapps.io" ?? ""
									};

									await _historyStorageRepository.Create(history);
									appTheme["logo"] = $"app{app.Id}/app_logo/{match.Value}";
								}
							}

							if (appTheme["favicon"] != null && !string.IsNullOrEmpty(appTheme["favicon"].ToString()) && appTheme["favicon"].ToString().Contains("http"))
							{
								var regex = new Regex(@"[\w-]+.(jpg|png|jpeg|ico)");
								var match = regex.Match(appTheme["favicon"].ToString());
								if (match.Success)
								{
									var webClient = new WebClient();
									var imageBytes = webClient.DownloadData(appTheme["favicon"].ToString());
									Stream stream = new MemoryStream(imageBytes);

									await _storage.Upload($"app{app.Id}/app_logo", match.Value, stream);

									var history = new HistoryStorage
									{
										MimeType = GetMimeType(match.Value),
										Path = $"app{app.Id}/app_logo/",
										Operation = "PUT",
										FileName = match.Value,
										UniqueName = match.Value,
										ExecutedAt = DateTime.Now,
										CreatedByEmail = "studio@primeapps.io" ?? ""
									};

									await _historyStorageRepository.Create(history);
									appTheme["favicon"] = $"app{app.Id}/app_logo/{match.Value}";
								}
							}

							app.Setting.AppTheme = JsonConvert.SerializeObject(appTheme);
						}
						catch (Exception e)
						{
							SentrySdk.CaptureException(e);
							//ErrorHandler.LogError(e, $"Migration app theme error. App Id : {app.Id}");
						}
					}
					else
					{
						app.Setting.AppTheme = new JObject()
						{
							["color"] = "#555198",
							["title"] = "PrimeApps",
						}.ToJsonString();
					}
				}
				catch (Exception e)
				{
					SentrySdk.CaptureException(e);
					//ErrorHandler.LogError(e, $"Migration eror for app {app.Id}.");
				}

				var storageHistoryLast = await _historyStorageRepository.GetLast();
				if (storageHistoryLast != null)
				{
					storageHistoryLast.Tag = "1";
					await _historyStorageRepository.Update(storageHistoryLast);
				}

				var databaseHistoryLast = await _historyDatabaseRepository.GetLast();
				if (databaseHistoryLast != null)
				{
					databaseHistoryLast.Tag = "1";
					await _historyDatabaseRepository.Update(databaseHistoryLast);
				}

				var release = new Release()
				{
					AppId = app.Id,
					CreatedById = 1,
					CreatedAt = DateTime.Now,
					Deleted = false,
					Status = ReleaseStatus.Succeed,
					Version = "1",
					StartTime = DateTime.Now,
					EndTime = DateTime.Now
				};

				await _releaseRepository.Create(release);

				await _storage.AddHttpReferrerUrlToBucket($"app{app.Id}", $"{schema}://{app.Setting.AppDomain}", UnifiedStorage.PolicyType.TenantPolicy);
				await _storage.AddHttpReferrerUrlToBucket($"app{app.Id}", $"{schema}://{app.Setting.AuthDomain}", UnifiedStorage.PolicyType.TenantPolicy);

				await _applicationRepository.Update(app);

				var seqTables = new List<string>
						{
							"action_button_permissions_id_seq", "action_buttons_id_seq", "bpm_categories_id_seq",
							"bpm_record_filters_id_seq", "bpm_workflow_logs_id_seq", "bpm_workflows_id_seq",
							"calculations_id_seq", "charts_id_seq", "components_id_seq", "conversion_mappings_id_seq",
							"conversion_sub_modules_id_seq", "dashboard_id_seq", "dashlets_id_seq", "dependencies_id_seq",
							"deployments_component_id_seq", "deployments_function_id_seq", "documents_id_seq",
							"field_filters_id_seq", "field_permissions_id_seq", "fields_id_seq", "functions_id_seq",
							"helps_id_seq", "import_maps_id_seq", "imports_id_seq", "menu_id_seq", "menu_items_id_seq",
							"module_profile_settings_id_seq", "modules_id_seq", "notes_id_seq", "notifications_id_seq",
							"picklist_items_id_seq", "picklists_id_seq", "process_approvers_id_seq",
							"process_filters_id_seq", "processes_id_seq", "profile_permissions_id_seq",
							"profiles_id_seq", "relations_id_seq", "reminders_id_seq", "report_aggregations_id_seq",
							"report_categories_id_seq", "report_fields_id_seq", "report_filters_id_seq",
							"reports_id_seq", "roles_id_seq", "section_permissions_id_seq", "sections_id_seq",
							"settings_id_seq", "tags_id_seq", "template_permissions_id_seq", "templates_id_seq",
							"view_fields_id_seq", "view_filters_id_seq", "views_id_seq", "widgets_id_seq",
							"workflow_filters_id_seq", "workflows_id_seq"
						};

				foreach (var seqTable in seqTables)
				{
					PostgresHelper.Run(PREConnectionString, $"app{app.Id}", $"SELECT setval('{seqTable}', 11000, true); ");
				}

				PostgresHelper.ChangeTemplateDatabaseStatus(PREConnectionString, $"app{app.Id}", false);
				await UpdateTenants(app.Id, $"{schema}://{app.Setting.AppDomain}");
			}

			SentrySdk.CaptureMessage("Tenants update started.", SentryLevel.Info);
			//ErrorHandler.LogMessage("Migration finished successfully.");

			return true;
		}

		public async Task<bool> UpdateTenants(int appId, string url)
		{
			_tenantRepository.CurrentUser = new CurrentUser { PreviewMode = "app", TenantId = appId, UserId = 1 };
			var tenantIds = await _tenantRepository.GetIdsByAppId(appId);
			var tenantIdList = tenantIds.ToList();
			var lastTenantId = tenantIdList.Last();

			var parts = Math.Ceiling((double)tenantIdList.Count / 200);

			for (var i = 0; i < tenantIdList.Count; i++)
			{
				var tenantId = tenantIdList[i];
				var exists = PostgresHelper.Read(_configuration.GetConnectionString("PlatformDBConnection"), $"platform", $"SELECT 1 AS result FROM pg_database WHERE datname='tenant{tenantId}'", "hasRows");

				if (!exists)
					continue;

				var time = TimeSpan.FromSeconds(10);

				if (i > 200 && i <= 400)
					time = TimeSpan.FromMinutes(5);
				else if (i > 400 && i <= 600)
					time = TimeSpan.FromMinutes(10);
				else if (i > 600 && i <= 800)
					time = TimeSpan.FromMinutes(15);
				else if (i > 800 && i <= 1000)
					time = TimeSpan.FromMinutes(20);
				else if (i > 1000 && i <= 1200)
					time = TimeSpan.FromMinutes(25);
				else if (i > 1200 && i <= 1400)
					time = TimeSpan.FromMinutes(30);
				else if (i > 1400 && i <= 1600)
					time = TimeSpan.FromMinutes(35);
				else if (i > 1600 && i <= 1800)
					time = TimeSpan.FromMinutes(40);
				else if (i > 1800 && i <= 2000)
					time = TimeSpan.FromMinutes(45);
				else if (i > 2000 && i <= 2200)
					time = TimeSpan.FromMinutes(50);
				else if (i > 2200 && i <= 2400)
					time = TimeSpan.FromMinutes(55);
				else if (i > 2400 && i <= 2600)
					time = TimeSpan.FromMinutes(60);
				else if (i > 2600 && i <= 2800)
					time = TimeSpan.FromMinutes(65);
				else if (i > 2800 && i <= 3000)
					time = TimeSpan.FromMinutes(70);

				BackgroundJob.Schedule<IMigrationHelper>(x => x.UpdateTenant(tenantId, url, lastTenantId), time);
			}

			return true;
		}

		[QueueCustom]
		public async Task<bool> UpdateTenant(int id, string url, int lastTenantId)
		{
			var tenant = await _tenantRepository.GetAsync(id);

			_currentUser = new CurrentUser { PreviewMode = "tenant", TenantId = tenant.Id, UserId = 1 };

			_tenantRepository.CurrentUser = _historyStorageRepository.CurrentUser = _historyDatabaseRepository.CurrentUser = _currentUser;

			if (!string.IsNullOrEmpty(tenant.Setting.Logo) && tenant.Setting.Logo.Contains("http"))
			{
				try
				{
					var regex = new Regex(@"[\w-]+.(jpg|png|jpeg|ico)");
					var match = regex.Match(tenant.Setting.Logo);
					if (match.Success)
					{
						var webClient = new WebClient();
						var imageBytes = webClient.DownloadData(tenant.Setting.Logo);
						Stream stream = new MemoryStream(imageBytes);

						await _storage.Upload($"tenant{tenant.Id}/logos", match.Value, stream);

						tenant.Setting.Logo = $"tenant{tenant.Id}/logos/{match.Value}";
					}
				}
				catch (Exception e)
				{
					SentrySdk.CaptureMessage("Tenant logo cannot uploaded. TenantId: " + tenant.Id + " Exception Message: " + e.Message);
				}
			}

			await _tenantRepository.UpdateAsync(tenant);

			await _storage.AddHttpReferrerUrlToBucket($"tenant{tenant.Id}", url, UnifiedStorage.PolicyType.TenantPolicy);

			var seqTables = new List<string>
						{
							"action_button_permissions_id_seq", "action_buttons_id_seq", "bpm_categories_id_seq",
							"bpm_record_filters_id_seq", "bpm_workflow_logs_id_seq", "bpm_workflows_id_seq",
							"calculations_id_seq", "charts_id_seq", "components_id_seq", "conversion_mappings_id_seq",
							"conversion_sub_modules_id_seq", "dashboard_id_seq", "dashlets_id_seq", "dependencies_id_seq",
							"deployments_component_id_seq", "deployments_function_id_seq", "documents_id_seq",
							"field_filters_id_seq", "field_permissions_id_seq", "fields_id_seq", "functions_id_seq",
							"helps_id_seq", "import_maps_id_seq", "imports_id_seq", "menu_id_seq", "menu_items_id_seq",
							"module_profile_settings_id_seq", "modules_id_seq", "notes_id_seq", "notifications_id_seq",
							"picklist_items_id_seq", "picklists_id_seq", "process_approvers_id_seq",
							"process_filters_id_seq", "processes_id_seq", "profile_permissions_id_seq",
							"profiles_id_seq", "relations_id_seq", "reminders_id_seq", "report_aggregations_id_seq",
							"report_categories_id_seq", "report_fields_id_seq", "report_filters_id_seq",
							"reports_id_seq", "roles_id_seq", "section_permissions_id_seq", "sections_id_seq",
							"settings_id_seq", "tags_id_seq", "template_permissions_id_seq", "templates_id_seq",
							"view_fields_id_seq", "view_filters_id_seq", "views_id_seq", "widgets_id_seq",
							"workflow_filters_id_seq", "workflows_id_seq"
						};

			foreach (var seqTable in seqTables)
			{
				PostgresHelper.Run(_configuration.GetConnectionString("PlatformDBConnection"), $"tenant{tenant.Id}", $"SELECT setval('{seqTable}', 500000, true); ");
			}

			var lastHdRecord = await _historyDatabaseRepository.GetLast();

			if (lastHdRecord != null)
			{
				lastHdRecord.Tag = "1";
				await _historyDatabaseRepository.Update(lastHdRecord);
			}
			else
			{
				var version = new HistoryDatabase
				{
					CommandText = "",
					TableName = "",
					CreatedByEmail = "studio@primeapps.io",
					ExecutedAt = DateTime.Now,
					Tag = "1",
					Deleted = false
				};

				try
				{
					await _historyDatabaseRepository.Create(version);
				}
				catch (Exception e)
				{
					SentrySdk.CaptureMessage("Tenant HistoryDatabase error. TenantId: " + tenant.Id + " Exception Message: " + e.Message);
				}
			}

			var lastHsRecord = await _historyStorageRepository.GetLast();

			if (lastHsRecord != null)
			{
				lastHsRecord.Tag = "1";
				await _historyStorageRepository.Update(lastHsRecord);
			}
			else
			{
				var version = new HistoryStorage()
				{
					CreatedByEmail = "studio@primeapps.io",
					ExecutedAt = DateTime.Now,
					Tag = "1",
					Deleted = false
				};

				try
				{
					await _historyStorageRepository.Create(version);
				}
				catch (Exception e)
				{
					SentrySdk.CaptureMessage("Tenant HistoryStorage error. TenantId: " + tenant.Id + " Exception Message: " + e.Message);
				}
			}

			SentrySdk.CaptureMessage($"Tenant{id} has been updated successfully.", SentryLevel.Info);

			if (id == lastTenantId)
				SentrySdk.CaptureMessage("All tenants have been updated successfully.", SentryLevel.Info);

			return true;
		}

		public string GetMimeType(string name)
		{
			var type = name.Split('.')[1];
			switch (type)
			{
				case "gif":
					return "image/bmp";
				case "bmp":
					return "image/bmp";
				case "jpeg":
				case "jpg":
					return "image/jpeg";
				case "png":
					return "image/png";
				case "tif":
				case "tiff":
					return "image/tiff";
				case "doc":
					return "application/msword";
				case "docx":
					return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
				case "pdf":
					return "application/pdf";
				case "ppt":
					return "application/vnd.ms-powerpoint";
				case "pptx":
					return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
				case "xlsx":
					return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
				case "xls":
					return "application/vnd.ms-excel";
				case "csv":
					return "text/csv";
				case "xml":
					return "text/xml";
				case "html":
					return "text/html";
				case "js":
					return "text/javascript";
				case "txt":
					return "text/plain";
				case "zip":
					return "application/zip";
				case "ogg":
					return "application/ogg";
				case "mp3":
					return "audio/mpeg";
				case "wma":
					return "audio/x-ms-wma";
				case "wav":
					return "audio/x-wav";
				case "wmv":
					return "audio/x-ms-wmv";
				case "swf":
					return "application/x-shockwave-flash";
				case "avi":
					return "video/avi";
				case "mp4":
					return "video/mp4";
				case "mpeg":
					return "video/mpeg";
				case "mpg":
					return "video/mpeg";
				case "qt":
					return "video/quicktime";
				default:
					return "text/plain";
			}
		}

		public async Task ApplyMigrations(List<int> ids)
		{
			var PREConnectionString = _configuration.GetConnectionString("PlatformDBConnection");

			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				using (var platformDbContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>())
				{
					using (var applicationRepository = new ApplicationRepository(platformDbContext, _configuration))
					{
						foreach (var id in ids)
						{
							var app = await applicationRepository.Get(id);

							if (app == null)
								return;

							await UpdateAppModuleFieldsAndSettings(id, PREConnectionString);
						}

					}
				}
			}
		}

		public async Task UpdateAppModuleFieldsAndSettings(int appId, string PREConnectionString)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				using (var platformDbContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>())
				using (var tenantDbContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>())
				{
					using (var tenantRepository = new TenantRepository(platformDbContext, _configuration))
					using (var moduleRepository = new ModuleRepository(tenantDbContext, _configuration))
					using (var settingRepository = new SettingRepository(tenantDbContext, _configuration))
					{
						_currentUser = new CurrentUser { PreviewMode = "app", TenantId = appId, UserId = 1 };
						moduleRepository.CurrentUser = settingRepository.CurrentUser = _currentUser;

						var appExists = PostgresHelper.Read(_configuration.GetConnectionString("PlatformDBConnection"), $"platform", $"SELECT 1 AS result FROM pg_database WHERE datname='app{appId}'", "hasRows");

						if (appExists)
						{
							PostgresHelper.ChangeTemplateDatabaseStatus(PREConnectionString, $"app{appId}", true);
							await UpdateModules();
							await UpdateSetting();
							PostgresHelper.ChangeTemplateDatabaseStatus(PREConnectionString, $"app{appId}", false);
						}
						var tenantIds = await tenantRepository.GetIdsByAppId(appId);

						for (int i = 0; i < tenantIds.Count; i++)
						{
							var tenantId = tenantIds[i];
							var exists = PostgresHelper.Read(_configuration.GetConnectionString("PlatformDBConnection"), $"platform", $"SELECT 1 AS result FROM pg_database WHERE datname='tenant{tenantId}'", "hasRows");

							if (!exists)
								continue;

							var time = TimeSpan.FromSeconds(10);

							if (i > 200 && i <= 400)
								time = TimeSpan.FromMinutes(5);
							else if (i > 400 && i <= 600)
								time = TimeSpan.FromMinutes(10);
							else if (i > 600 && i <= 800)
								time = TimeSpan.FromMinutes(15);
							else if (i > 800 && i <= 1000)
								time = TimeSpan.FromMinutes(20);
							else if (i > 1000 && i <= 1200)
								time = TimeSpan.FromMinutes(25);
							else if (i > 1200 && i <= 1400)
								time = TimeSpan.FromMinutes(30);
							else if (i > 1400 && i <= 1600)
								time = TimeSpan.FromMinutes(35);
							else if (i > 1600 && i <= 1800)
								time = TimeSpan.FromMinutes(40);
							else if (i > 1800 && i <= 2000)
								time = TimeSpan.FromMinutes(45);
							else if (i > 2000 && i <= 2200)
								time = TimeSpan.FromMinutes(50);
							else if (i > 2200 && i <= 2400)
								time = TimeSpan.FromMinutes(55);
							else if (i > 2400 && i <= 2600)
								time = TimeSpan.FromMinutes(60);
							else if (i > 2600 && i <= 2800)
								time = TimeSpan.FromMinutes(65);
							else if (i > 2800 && i <= 3000)
								time = TimeSpan.FromMinutes(70);
							else if (i > 3000 && i <= 3200)
								time = TimeSpan.FromMinutes(75);
							else if (i > 3200 && i <= 3400)
								time = TimeSpan.FromMinutes(80);
							else if (i > 3400)
								time = TimeSpan.FromMinutes(85);

							BackgroundJob.Schedule<IMigrationHelper>(x => x.UpdateTenantModuleFields(tenantId), time);

							//Last index
							if (i == tenantIds.Count - 1)
								SentrySdk.CaptureMessage("All tenants have been updated successfully.", SentryLevel.Info);
						}
					}
				}
			}
		}

		[QueueCustom]
		public async Task UpdateTenantModuleFields(int tenantId)
		{
			_currentUser = new CurrentUser { PreviewMode = "tenant", TenantId = tenantId, UserId = 1 };
			_moduleRepository.CurrentUser = _settingRepository.CurrentUser = _currentUser;
			await UpdateSetting();
			await UpdateModules();
		}

		public async Task<ICollection<Module>> UpdateModules()
		{
			var modules = await _moduleRepository.GetAll();
			foreach (var module in modules)
			{
				var fiedlIsExist = module.Fields.SingleOrDefault(q => q.Name == "is_sample");
				if (fiedlIsExist == null)
				{

					module.Fields.Add(new Field
					{
						Name = "is_sample",
						DataType = DataType.Checkbox,
						Deleted = false,
						DisplayDetail = true,
						DisplayForm = true,
						DisplayList = true,
						Editable = true,
						InlineEdit = true,
						LabelEn = "Sample Data",
						LabelTr = "Örnek Kayıt",
						Order = (short)(module.Fields.Count + 1),
						Primary = false,
						Section = "system",
						SectionColumn = 2,
						ShowLabel = true,
						ShowOnlyEdit = false,
						SystemType = SystemType.Custom,
						Validation = new FieldValidation
						{
							Readonly = false,
							Required = false
						}
					});
					await _moduleRepository.Update(module);
				}
			}

			return modules;
		}

		public async Task UpdateSetting()
		{
			var settings = await _settingRepository.GetAllSettings();
			var settingsPasswordList = settings.Where(r => r.Key == "password");
			foreach (var settingPassword in settingsPasswordList)
			{
				if (string.IsNullOrEmpty(settingPassword.Value))
					continue;

				settingPassword.Value = CryptoHelper.Encrypt(settingPassword.Value);
				await _settingRepository.Update(settingPassword);
			}
		}
	}
}
