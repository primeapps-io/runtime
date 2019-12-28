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
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Storage;
using Sentry;
using Sentry.Protocol;

namespace PrimeApps.Admin.Helpers
{
    public interface IMigrationHelper
    {
        Task AppMigration(string schema, bool isLocal, string[] ids);
    }

    public class MigrationHelper : IMigrationHelper
    {
        private CurrentUser _currentUser;
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;
        private IHttpContextAccessor _context;
        private IUnifiedStorage _storage;
        private IHostingEnvironment _hostingEnvironment;

        public MigrationHelper(IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            IHttpContextAccessor context,
            IUnifiedStorage storage, 
            IHostingEnvironment hostingEnvironment)
        {
            _storage = storage;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task AppMigration(string schema, bool isLocal, string[] ids)
        {
            var PREConnectionString = _configuration.GetConnectionString("PlatformDBConnection");
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var platformDbContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                var tenantDbContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                using (var templateRepository = new TemplateRepository(tenantDbContext, _configuration))
                using (var historyDatabaseRepository = new HistoryDatabaseRepository(tenantDbContext, _configuration))
                using (var applicationRepository = new ApplicationRepository(platformDbContext, _configuration))
                using (var historyStorageRepository = new HistoryStorageRepository(tenantDbContext, _configuration))
                {
                    foreach (var id in ids)
                    {
                        var app = await applicationRepository.Get(int.Parse(id));

                        if (app == null)
                            return;
                        
                        _currentUser = new CurrentUser { PreviewMode = "app", TenantId = app.Id, UserId = 1 };
                        
                        templateRepository.CurrentUser = historyStorageRepository.CurrentUser = historyDatabaseRepository.CurrentUser = _currentUser;
                        
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
                                            
                                            await historyStorageRepository.Create(history);
                                            authTheme["logo"] =  $"app{app.Id}/app_logo/{match.Value}";
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
                                            
                                            await historyStorageRepository.Create(history);
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
                                            
                                            await historyStorageRepository.Create(history);
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
                                    ["banner"] = new JArray {new JObject {["image"] = "", ["descriptions"] = ""}},
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
                                            
                                            await historyStorageRepository.Create(history);
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
                                            
                                            await historyStorageRepository.Create(history);
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

                        var storageHistoryLast = await historyStorageRepository.GetLast();
                        if (storageHistoryLast != null)
                        {
                            storageHistoryLast.Tag = "1";
                            await historyStorageRepository.Update(storageHistoryLast);
                        }
                        
                        var databaseHistoryLast = await historyDatabaseRepository.GetLast();
                        if (databaseHistoryLast != null)
                        {
                            databaseHistoryLast.Tag = "1";
                            await historyDatabaseRepository.Update(databaseHistoryLast);
                        }
                        await _storage.AddHttpReferrerUrlToBucket($"app{app.Id}", $"{schema}://{app.Setting.AppDomain}", UnifiedStorage.PolicyType.TenantPolicy);
                        await _storage.AddHttpReferrerUrlToBucket($"app{app.Id}",$"{schema}://{app.Setting.AuthDomain}", UnifiedStorage.PolicyType.TenantPolicy);
                        
                        await applicationRepository.Update(app);
                        
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
                            PostgresHelper.Run(PREConnectionString, $"app{app.Id}",$"SELECT setval('{seqTable}', 11000, true); ");
                        }
                        
                        PostgresHelper.ChangeTemplateDatabaseStatus(PREConnectionString, $"app{app.Id}", false);
                        await UpdateTenants(app.Id, $"{schema}://{app.Setting.AppDomain}");
                    }
                    
                    SentrySdk.CaptureMessage("Migration finished successfully.", SentryLevel.Info);
                    //ErrorHandler.LogMessage("Migration finished successfully.");
                }
            }
        }

        public async Task<bool> UpdateTenants(int appId, string url)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var platformDbContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                var tenantDbContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                using (var tenantRepository = new TenantRepository(platformDbContext, _configuration))
                using (var historyDatabaseRepository = new HistoryDatabaseRepository(tenantDbContext, _configuration))
                using (var historyStorageRepository = new HistoryStorageRepository(tenantDbContext, _configuration))
                {
                    var tenantIds = await tenantRepository.GetByAppId(appId);
                    
                    foreach (var id in tenantIds)
                    {
                        var tenant = await tenantRepository.GetAsync(id);
                        var PREConnectionString = _configuration.GetConnectionString("PlatformDBConnection");

                        var exists = PostgresHelper.Read(PREConnectionString, $"platform",$"SELECT 1 AS result FROM pg_database WHERE datname='tenant{tenant.Id}'", "hasRows");

                        if (!exists)
                            continue;
                        
                        _currentUser = new CurrentUser {PreviewMode = "tenant", TenantId = tenant.Id, UserId = 1};

                        tenantRepository.CurrentUser = historyStorageRepository.CurrentUser = historyDatabaseRepository.CurrentUser = _currentUser;
                        
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

                        await tenantRepository.UpdateAsync(tenant);

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
                            PostgresHelper.Run(PREConnectionString, $"tenant{tenant.Id}",$"SELECT setval('{seqTable}', 500000, true); ");
                        }

                        var lastHdRecord = await historyDatabaseRepository.GetLast();

                        if (lastHdRecord != null)
                        {
                            lastHdRecord.Tag = "1";
                            await historyDatabaseRepository.Update(lastHdRecord);
                        }
                        else
                        {
                            var version = new HistoryDatabase
                            {
                                CommandText = "",
                                TableName = "",
                                CreatedByEmail = "studio@primeapps.io",
                                ExecutedAt = DateTime.Now,
                                Tag = "1"
                            };

                            await historyDatabaseRepository.Create(version);
                        }
                        
                        var lastHsRecord = await historyStorageRepository.GetLast();

                        if (lastHsRecord != null)
                        {
                            lastHsRecord.Tag = "1";
                            await historyStorageRepository.Update(lastHsRecord);
                        }
                        else
                        {
                            var version = new HistoryStorage()
                            {
                                CreatedByEmail = "studio@primeapps.io",
                                ExecutedAt = DateTime.Now,
                                Tag = "1"
                            };

                            await historyStorageRepository.Create(version);
                        }
                    }

                    return true;
                }
            }
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
    }
}