using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json.Serialization;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Storage;
using Sentry.Protocol;

namespace PrimeApps.Studio.Helpers
{
    public interface IMigrationHelper
    {
        Task Apply(string schema, bool isLocal);
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

        public async Task Apply(string schema, bool isLocal)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var studioDbContext = _scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                var tenantDbContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                using (var templateRepository = new TemplateRepository(tenantDbContext, _configuration))
                using (var appDraftRepository = new AppDraftRepository(studioDbContext, _configuration))
                using (var packageRepository = new PackageRepository(studioDbContext, _configuration))
                using (var historyDatabaseRepository = new HistoryDatabaseRepository(tenantDbContext, _configuration))
                using (var historyStorageRepository = new HistoryStorageRepository(tenantDbContext, _configuration))
                {
                    var apps = await appDraftRepository.GetAll();

                    foreach (var app in apps)
                    {
                        _currentUser = new CurrentUser { PreviewMode = "app", TenantId = app.Id, UserId = 1 };
                        
                        templateRepository.CurrentUser = historyStorageRepository.CurrentUser = historyDatabaseRepository.CurrentUser = packageRepository.CurrentUser = appDraftRepository.CurrentUser = _currentUser;
                        
                        try
                        {
                            await _storage.CreateBucketIfNotExists($"app{app.Id}");
                            
                            if (!string.IsNullOrEmpty(app.Logo) && app.Logo.Contains("http"))
                            {
                                var regex = new Regex(@"[\w-]+.(jpg|png|jpeg)");
                                var match = regex.Match(app.Logo);
                                if (match.Success)
                                {
                                    await _storage.CopyObject($"organization{app.OrganizationId}/app{app.Id}/app_logo", match.Value, $"app{app.Id}/app_logo", match.Value);
                                    app.Logo = $"app{app.Id}/app_logo/{match.Value}";
                                }
                            }

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

                            if (string.IsNullOrEmpty(app.Setting.Options))
                            {
                                app.Setting.Options = new JObject
                                {
                                    //["clear_all_records"] = true,
                                    ["enable_registration"] = true,
									["enable_ldap"] = false,
									["enable_api_registration"] = true,
                                    ["picklist_language"] = "en"
								}.ToString();
                            }

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
                                            if(!await _storage.ObjectExists($"app{app.Id}/app_logo", match.Value))
                                                await _storage.CopyObject($"organization{app.OrganizationId}/app{app.Id}/app_logo", match.Value, $"app{app.Id}/app_logo", match.Value);
                                            
                                            var history = new HistoryStorage
                                            {
                                                MimeType = DocumentHelper.GetMimeType(match.Value),
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
                                            if(!await _storage.ObjectExists($"app{app.Id}/app_logo", match.Value))
                                                await _storage.CopyObject($"organization{app.OrganizationId}/app{app.Id}/app_logo", match.Value, $"app{app.Id}/app_logo", match.Value);
                                            
                                            var history = new HistoryStorage
                                            {
                                                MimeType = DocumentHelper.GetMimeType(match.Value),
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
                                            if(!await _storage.ObjectExists($"app{app.Id}/app_logo", match.Value))
                                                await _storage.CopyObject($"organization{app.OrganizationId}/app{app.Id}/app_logo", match.Value, $"app{app.Id}/app_logo", match.Value);
                                            
                                            var history = new HistoryStorage
                                            {
                                                MimeType = DocumentHelper.GetMimeType(match.Value),
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
                                    ErrorHandler.LogError(e, $"Migration auth theme error. App Id : {app.Id}");
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
                                            if(!await _storage.ObjectExists($"app{app.Id}/app_logo", match.Value))
                                                await _storage.CopyObject($"organization{app.OrganizationId}/app{app.Id}/app_logo", match.Value, $"app{app.Id}/app_logo", match.Value);
                                            
                                            var history = new HistoryStorage
                                            {
                                                MimeType = DocumentHelper.GetMimeType(match.Value),
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
                                            if(!await _storage.ObjectExists($"app{app.Id}/app_logo", match.Value))
                                                await _storage.CopyObject($"organization{app.OrganizationId}/app{app.Id}/app_logo", match.Value, $"app{app.Id}/app_logo", match.Value);
                                            
                                            var history = new HistoryStorage
                                            {
                                                MimeType = DocumentHelper.GetMimeType(match.Value),
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
                                    ErrorHandler.LogError(e, $"Migration app theme error. App Id : {app.Id}");
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

                            var excelTemplates = await templateRepository.GetByType(TemplateType.Excel);

                            foreach (var excelTemplate in excelTemplates)
                            {
                                if (!await _storage.ObjectExists($"app{app.Id}/templates", excelTemplate.Content))
                                {
                                    await _storage.CopyObject($"organization{app.OrganizationId}/app{app.Id}/templates", excelTemplate.Content, $"app{app.Id}/templates", excelTemplate.Content);

                                    var history = new HistoryStorage
                                    {
                                        MimeType = DocumentHelper.GetMimeType(excelTemplate.Content),
                                        Path = $"app{app.Id}/templates/",
                                        Operation = "PUT",
                                        FileName = excelTemplate.Content,
                                        UniqueName = excelTemplate.Content,
                                        ExecutedAt = DateTime.Now,
                                        CreatedByEmail = "studio@primeapps.io" ?? ""
                                    };
                                            
                                    await historyStorageRepository.Create(history);
                                }
                               
                            }
                            
                            var moduleTemplates = await templateRepository.GetByType(TemplateType.Module);
                            
                            foreach (var moduleTemplate in moduleTemplates)
                            {
                                if (!await _storage.ObjectExists($"app{app.Id}/templates", moduleTemplate.Content))
                                {
                                    await _storage.CopyObject($"organization{app.OrganizationId}/app{app.Id}/templates", moduleTemplate.Content, $"app{app.Id}/templates", moduleTemplate.Content);

                                    var history = new HistoryStorage
                                    {
                                        MimeType = DocumentHelper.GetMimeType(moduleTemplate.Content),
                                        Path = $"app{app.Id}/templates/",
                                        Operation = "PUT",
                                        FileName = moduleTemplate.Content,
                                        UniqueName = moduleTemplate.Content,
                                        ExecutedAt = DateTime.Now,
                                        CreatedByEmail = "studio@primeapps.io" ?? ""
                                    };

                                    await historyStorageRepository.Create(history);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ErrorHandler.LogError(e, $"Migration eror for app {app.Id}.");
                        }
                       
                        if(isLocal)
                            await _storage.AddHttpReferrerUrlToBucket($"app{app.Id}", $"{schema}://localhost:*", UnifiedStorage.PolicyType.StudioPolicy);
                        else
                        {
                            await _storage.AddHttpReferrerUrlToBucket($"app{app.Id}", $"{schema}://*.primeapps.io", UnifiedStorage.PolicyType.StudioPolicy);
                            await _storage.AddHttpReferrerUrlToBucket($"app{app.Id}",$"{schema}://*.primeapps.app", UnifiedStorage.PolicyType.StudioPolicy);
                        }
                        
                        await appDraftRepository.Update(app);
                        
                        var PDEConnectionString = _configuration.GetConnectionString("StudioDBConnection");
                        
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
                            PostgresHelper.Run(PDEConnectionString, $"app{app.Id}",$"SELECT setval('{seqTable}', 10000, true); ");
                        }
                    }
                    
                    ErrorHandler.LogMessage("Migration finished successfully.");
                }
            }
        }
    }
}