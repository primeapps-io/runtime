using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Storage;
using Sentry.Protocol;
using HistoryRepository = PrimeApps.Model.Context.HistoryRepository;

namespace PrimeApps.Model.Helpers
{
    public class PublishHelper
    {
        public static async Task<bool> UpdateTenant(string version, string dbName, IConfiguration configuration,
            IUnifiedStorage storage, int appId, int orgId, string token, IHostingEnvironment hostingEnvironment)
        {
            var PREConnectionString = configuration.GetConnectionString("PlatformDBConnection");
            var rootPath = DataHelper.GetDataDirectoryPath(configuration, hostingEnvironment);
            var root = Path.Combine(rootPath, "tenant-update-logs");

            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            var available = PostgresHelper.Read(PREConnectionString, dbName,
                $"SELECT 1 FROM history_database WHERE tag = '{version}';", "hasRows");

            if (available)
                return true;

            var logPath = Path.Combine(root, $"update-{dbName}-version-{version}-log.txt");

            File.AppendAllText(logPath,
                "\u001b[92m" + "********** Update Tenant **********" + "\u001b[39m" + Environment.NewLine);
            File.AppendAllText(logPath,
                "\u001b[90m" + DateTime.Now + "\u001b[39m" + $" : Applying version is {version}..." +
                Environment.NewLine);

            try
            {
                var path = Path.Combine(rootPath, "packages", $"app{appId}", version);

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);


                if (!File.Exists(Path.Combine(path, $"{version}.zip")))
                {
                    using (var httpClient = new HttpClient())
                    {
                        var studioUrl = configuration.GetValue("AppSettings:StudioUrl", string.Empty);

                        var url = studioUrl + "/storage/download_file";
                        httpClient.BaseAddress = new Uri(url);
                        httpClient.DefaultRequestHeaders.Accept.Clear();
                        httpClient.DefaultRequestHeaders.Add("X-Organization-Id", orgId.ToString());
                        httpClient.DefaultRequestHeaders.Add("X-App-Id", appId.ToString());
                        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                        var request = new JObject
                        {
                            ["bucket_name"] = $"app{appId}",
                            ["path"] = $"/packages/{version}",
                            ["key"] = $"{version}.zip"
                        };

                        var response = await httpClient.PostAsync(url,
                            new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                        if (!response.IsSuccessStatusCode)
                        {
                            File.AppendAllText(logPath,
                                "\u001b[31m" + DateTime.Now +
                                " : Error - Unhandle exception. While downloading folder." + "\u001b[39m" +
                                Environment.NewLine);
                            File.AppendAllText(logPath,
                                "\u001b[31m ********** Update Tenant Failed********** \u001b[39m" +
                                Environment.NewLine);
                            return false;
                        }

                        var fileByte = await response.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(Path.Combine(path, $"{version}.zip"), fileByte);
                        await Task.Delay(1000);
                        ZipFile.ExtractToDirectory(Path.Combine(path, $"{version}.zip"), path);
                    }
                }

                var scriptPath = Path.Combine(path, "scripts.txt");
                var storagePath = Path.Combine(path, "storage.txt");
                var scriptsText = "";

                if (File.Exists(scriptPath))
                    scriptsText = File.ReadAllText(scriptPath, Encoding.UTF8);

                var sqls = new string[] { };

                if (!string.IsNullOrEmpty(scriptsText))
                    sqls = scriptsText.Split(Environment.NewLine);

                File.AppendAllText(logPath,
                    "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Scripts applying..." + Environment.NewLine);

                var result = PostgresHelper.RunAll(PREConnectionString, dbName, sqls, version);

                if (!result)
                {
                    File.AppendAllText(logPath,
                        "\u001b[31m" + DateTime.Now + " : Unhandle exception. While applying script. \u001b[39m" +
                        Environment.NewLine);
                    File.AppendAllText(logPath,
                        "\u001b[31m ********** Update Tenant Failed********** \u001b[39m" + Environment.NewLine);
                    return false;
                }

                if (File.Exists(storagePath))
                {
                    var storageText = File.ReadAllText(storagePath, Encoding.UTF8);

                    if (!string.IsNullOrEmpty(storageText))
                    {
                        try
                        {
                            var files = JArray.Parse(storageText);


                            foreach (var file in files)
                            {
                                var bucketName = (string)file["path"];

                                if (bucketName.Contains("templates") && bucketName.Contains($"app{appId}"))
                                    bucketName = bucketName.Replace($"app{appId}", dbName);

                                if (!await storage.ObjectExists(bucketName, file["unique_name"].ToString()))
                                {
                                    using (var fileStream = new FileStream(
                                        Path.Combine(path, "files", file["file_name"].ToString()),
                                        FileMode.OpenOrCreate))
                                    {
                                        try
                                        {
                                            await storage.Upload(file["file_name"].ToString(), bucketName,
                                                file["unique_name"].ToString(), fileStream);
                                        }
                                        catch (Exception e)
                                        {
                                            File.AppendAllText(logPath,
                                                "\u001b[90m" + DateTime.Now + "\u001b[39m" +
                                                $" : File {file["file_name"]} not uploaded. Unique name is {file["unique_name"]}..." +
                                                Environment.NewLine);
                                            ErrorHandler.LogError(e,
                                                "PublishHelper UpdateTenant method error. AppId: " + appId +
                                                " - Version: " + version);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            File.AppendAllText(logPath,
                                "\u001b[90m" + DateTime.Now + "\u001b[39m" +
                                $" : \u001b[93m Unhandle exception while applying storage file... Error : {e.Message} \u001b[39m" +
                                Environment.NewLine);
                            ErrorHandler.LogError(e,
                                "PublishHelper UpdateTenant method error. AppId: " + appId + " - Version: " + version);
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                ErrorHandler.LogError(e,
                    "PublishHelper UpdateTenant method error. AppId:" + appId + ", OrgId:" + orgId + ",dbName:" +
                    dbName + ",version:" + version);
                File.AppendAllText(logPath,
                    "\u001b[90m" + DateTime.Now + "\u001b[39m" +
                    " : \u001b[93m Error - Unhandle exception - Message: " + e.Message + " \u001b[39m" +
                    Environment.NewLine);
                File.AppendAllText(logPath,
                    "\u001b[31m ********** Update Tenant Failed********** \u001b[39m" + Environment.NewLine);

                return false;
            }
        }

        public static async Task<List<Release>> ApplyVersions(IConfiguration configuration, IUnifiedStorage storage,
            JObject app, int orgId, string databaseName, List<string> versions, bool createPlatformApp, bool firstTime,
            string studioToken, string appUrl, string authUrl, bool useSsl, IHostingEnvironment hostingEnvironment)
        {
            var PREConnectionString = configuration.GetConnectionString("PlatformDBConnection");
            var postgresPath = PostgresHelper.GetPostgresBinaryPath(configuration, hostingEnvironment);
            var studioUrl = configuration.GetValue("AppSettings:StudioUrl", string.Empty);

            var rootPath = DataHelper.GetDataDirectoryPath(configuration, hostingEnvironment);
            var dbName = databaseName;
            var templateCopied = false;

            var root = Path.Combine(rootPath, "release-logs");

            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            var releaseList = new List<Release>();
            bool result;

            var localAuth = configuration.GetValue("AppSettings:AuthenticationServerURLLocal", string.Empty);
            var identityUrl = !string.IsNullOrEmpty(localAuth)
                ? localAuth
                : configuration.GetValue("AppSettings:AuthenticationServerURL", string.Empty);

            var logFileName = "";

            foreach (var obj in versions.OfType<object>().Select((version, index) => new {version, index}))
            {
                var version = obj.version;

                releaseList.Add(new Release
                {
                    AppId = int.Parse(app["id"].ToString()),
                    StartTime = DateTime.Now,
                    Version = version.ToString()
                });

                logFileName = $"release-app{app["id"]}-version{version}-{DateTime.Now:MM.dd.yyyy h-mm-ss}.txt";

                var logPath = Path.Combine(root, logFileName);

                File.AppendAllText(logPath,
                    "\u001b[92m" + "********** Apply Version **********" + "\u001b[39m" + Environment.NewLine);

                if (firstTime && obj.index == 0 && createPlatformApp)
                {
                    await storage.CreateBucketIfNotExists(databaseName);

                    File.AppendAllText(logPath,
                        "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Platform application creating..." +
                        Environment.NewLine);


                    result = PublishHelper.CreatePlatformApp(PREConnectionString, app, CryptoHelper.Encrypt(app["secret"].ToString()), appUrl, authUrl);

                    if (!result)
                    {
                        File.AppendAllText(logPath,
                            "\u001b[90m" + DateTime.Now + "\u001b[39m" +
                            " : \u001b[93m Unhandle exception while creating platform app... \u001b[39m" +
                            Environment.NewLine);

                        ErrorHandler.LogMessage($"App: app{app["id"]}, Version: {version}, Create platform application error.", SentryLevel.Error);
                    }

                    var token = studioToken;

                    if (!string.IsNullOrEmpty(localAuth))
                    {
                        var authIntegrationEmail = "app@primeapps.io";
                        var authIntegrationPassword = "123456";
                        var clientId = configuration.GetValue("AppSettings:ClientId", string.Empty);
                        var clientSecret = "secret";

                        token = await GetAuthToken(identityUrl, authIntegrationPassword, authIntegrationEmail, clientId, clientSecret);
                    }

                    if (!string.IsNullOrEmpty(token))
                    {
                        //var applicationUrl = string.Format(configuration.GetValue("AppSettings:AppUrl", string.Empty), app["name"].ToString());
                        var addClientResult = await CreateClient(appUrl, identityUrl, token, app["name"].ToString(),
                            app["label"].ToString(), app["secret"].ToString(), useSsl);

                        if (string.IsNullOrEmpty(addClientResult))
                        {
                            var clientResult = await AddClientUrl(identityUrl, token, appUrl, useSsl);
                            if (!string.IsNullOrEmpty(clientResult))
                            {
                                File.AppendAllText(logPath,
                                    "\u001b[90m" + DateTime.Now + "\u001b[39m" +
                                    " : \u001b[93m Unhandle exception while adding app url to client... \u001b[39m" +
                                    Environment.NewLine);

                                ErrorHandler.LogMessage($"App: app{app["id"]}, Version: {version}, Add client url error. Message: {clientResult}", SentryLevel.Error);
                            }
                        }
                        else
                        {
                            File.AppendAllText(logPath,
                                "\u001b[90m" + DateTime.Now + "\u001b[39m" +
                                " : \u001b[93m Unhandle exception while creating app client... Error: " +
                                addClientResult + " \u001b[39m" + Environment.NewLine);

                            ErrorHandler.LogMessage($"App: app{app["id"]}, Version: {version}, Create client error. Message: {addClientResult}", SentryLevel.Error);
                        }
                    }

                    try
                    {
                        await storage.AddHttpReferrerUrlToBucket($"app{app["id"]}",
                            (useSsl ? "https://" : "http://") + authUrl, UnifiedStorage.PolicyType.TenantPolicy);
                        await storage.AddHttpReferrerUrlToBucket($"app{app["id"]}",
                            (useSsl ? "https://" : "http://") + appUrl, UnifiedStorage.PolicyType.TenantPolicy);
                    }
                    catch (Exception e)
                    {
                        ErrorHandler.LogMessage($"App: app{app["id"]}, Version: {version}, Add storage profile permission error. Message: {e.Message}", SentryLevel.Error);
                    }
                }
                else if (!createPlatformApp && obj.index == 0)
                {
                    result = PublishHelper.UpdatePlatformApp(PREConnectionString, app, appUrl, authUrl);

                    if (!result)
                        File.AppendAllText(logPath,
                            "\u001b[90m" + DateTime.Now + "\u001b[39m" +
                            " : \u001b[93m Unhandle exception while updating platform app... \u001b[39m" +
                            Environment.NewLine);
                }

                File.AppendAllText(logPath,
                    "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Sync application templates ..." +
                    Environment.NewLine);

                PublishHelper.SyncAppTemplates(PREConnectionString, app);

                /*if (!File.Exists(logPath))
                    File.Create(logPath);*/

                File.AppendAllText(logPath,
                    "\u001b[90m" + DateTime.Now + "\u001b[39m" + $" : Applying version is {version}..." +
                    Environment.NewLine);

                try
                {
                    var path = Path.Combine(rootPath, "packages", databaseName, version.ToString());

                    if (Directory.Exists(path))
                        Directory.Delete(path, true);

                    Directory.CreateDirectory(path);

                    /*
                     * Download version folder from studio storage.
                     */

                    using (var httpClient = new HttpClient())
                    {
                        var url = studioUrl + "/storage/download_file";
                        httpClient.BaseAddress = new Uri(url);
                        httpClient.DefaultRequestHeaders.Accept.Clear();
                        httpClient.DefaultRequestHeaders.Add("X-Organization-Id", orgId.ToString());
                        httpClient.DefaultRequestHeaders.Add("X-App-Id", app["id"].ToString());
                        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {studioToken}");

                        var request = new JObject
                        {
                            ["bucket_name"] = databaseName,
                            ["path"] = $"/packages/{version}",
                            ["key"] = $"{version}.zip"
                        };

                        var response = await httpClient.PostAsync(url,
                            new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                        if (!response.IsSuccessStatusCode)
                        {
                            File.AppendAllText(logPath,
                                "\u001b[31m" + DateTime.Now +
                                " : Error - Unhandle exception. While downloading folder." + "\u001b[39m" +
                                Environment.NewLine);
                            releaseList.Last().Status = ReleaseStatus.Failed;
                            releaseList.Last().EndTime = DateTime.Now;
                            ErrorHandler.LogError(new Exception(response.RequestMessage.ToString()),
                                "Unhandle exception. While downloading package folder. Request: " +
                                JsonConvert.SerializeObject(request));
                            break;
                        }

                        var fileByte = await response.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(Path.Combine(path, $"{version}.zip"), fileByte);
                        await Task.Delay(1000);
                        ZipFile.ExtractToDirectory(Path.Combine(path, $"{version}.zip"), path);
                    }

                    var scriptPath = Path.Combine(path, "scripts.txt");
                    var storagePath = Path.Combine(path, "storage.txt");

                    //applyResult.Add("\u001b[92m" + "********** Publish **********" + "\u001b[39m");

                    if (firstTime && obj.index == 0)
                    {
                        //var sqlDump = await storage.GetObject(bucketName, "sqlDump.txt");
                        // Issue request and remember to dispose of the response

                        //var dumpText = File.ReadAllText($"{path}\\dumpSql.txt", Encoding.UTF8);

                        File.AppendAllText(logPath,
                            "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Restoring your database..." +
                            Environment.NewLine);

                        var createDbResult = PostgresHelper.CreateDatabaseIfNotExists(PREConnectionString, dbName);

                        if (!createDbResult)
                        {
                            releaseList.Last().Status = ReleaseStatus.Failed;
                            releaseList.Last().EndTime = DateTime.Now;
                            File.AppendAllText(logPath,
                                "\u001b[31m" + DateTime.Now +
                                " : Error - Unhandle exception. While creating database." + "\u001b[39m" +
                                Environment.NewLine);
                            break;
                        }

                        var restoreResult = PostgresHelper.Restore(PREConnectionString, dbName, postgresPath, $"{path}",
                            logDirectory: $"{path}");

                        if (!restoreResult)
                        {
                            releaseList.Last().Status = ReleaseStatus.Failed;
                            releaseList.Last().EndTime = DateTime.Now;
                            File.AppendAllText(logPath,
                                "\u001b[31m" + DateTime.Now +
                                " : Error - Unhandle exception. While restoring your database." + "\u001b[39m" +
                                Environment.NewLine);
                            break;
                        }

                        EfMigrate(configuration, databaseName);
                    }
                    else
                    {
                        if (!templateCopied)
                        {
                            PostgresHelper.RemoveTemplateDatabase(PREConnectionString, $"{dbName}_copy");
                            File.AppendAllText(logPath,
                                "\u001b[90m" + DateTime.Now + "\u001b[39m" +
                                " : Template database copying template database..." + Environment.NewLine);
                            result = PostgresHelper.CopyDatabase(PREConnectionString, dbName);

                            if (!result)
                            {
                                releaseList.Last().Status = ReleaseStatus.Failed;
                                releaseList.Last().EndTime = DateTime.Now;
                                File.AppendAllText(logPath,
                                    "\u001b[90m" + DateTime.Now + "\u001b[39m" +
                                    " : \u001b[93m Error - Unhandle exception while copying template database... \u001b[39m" +
                                    Environment.NewLine);
                                break;
                            }

                            dbName = $"{dbName}_copy";
                            /*File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Database removing template mark..." + Environment.NewLine);
                            result = ReleaseHelper.ChangeTemplateDatabaseStatus(PREConnectionString, $"{dbName}_copy", true, scriptPath);
    
                            if (!result)
                            {
                            File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Error - Unhandle exception while removing database as template mark... \u001b[39m" + Environment.NewLine);
                                break;
                            }*/
                            templateCopied = true;
                        }
                        else
                        {
                            PostgresHelper.ChangeTemplateDatabaseStatus(PREConnectionString, dbName, true);
                        }
                    }

                    if (File.Exists(scriptPath))
                    {
                        var scriptsText = File.ReadAllText(scriptPath, Encoding.UTF8);
                        var sqls = new string[] { };

                        if (!string.IsNullOrEmpty(scriptsText))
                            sqls = scriptsText.Split(Environment.NewLine);

                        File.AppendAllText(logPath,
                            "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Scripts applying..." +
                            Environment.NewLine);

                        foreach (var sql in sqls)
                        {
                            if (!string.IsNullOrEmpty(sql))
                            {
                                /*var tableName = ReleaseHelper.GetTableName(sql).Replace("public.", "");
                                PosgresHelper.Run(PREConnectionString, dbName, $"SELECT SETVAL('{tableName}_id_seq', (SELECT MAX(id) FROM {tableName}));");*/
                                var restoreResult = PostgresHelper.Run(PREConnectionString, dbName, sql);

                                if (!restoreResult)
                                    File.AppendAllText(logPath,
                                        "\u001b[31m" + DateTime.Now +
                                        " : Unhandle exception. While applying script. Script is : (" + sql + ")" +
                                        "\u001b[39m" + Environment.NewLine);
                            }
                        }
                    }

                    if (firstTime && obj.index == 0)
                    {
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
                            PostgresHelper.Run(PREConnectionString, dbName, $"SELECT setval('{seqTable}', 500000, true); ");
                        }
                    }

                    File.AppendAllText(logPath,
                        "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Database marking as template..." +
                        Environment.NewLine);

                    result = PostgresHelper.ChangeTemplateDatabaseStatus(PREConnectionString, dbName, false);

                    if (!result)
                        File.AppendAllText(logPath,
                            "\u001b[90m" + DateTime.Now + "\u001b[39m" +
                            " : \u001b[93m Unhandle exception while marking database as template... \u001b[39m" +
                            Environment.NewLine);

                    if (File.Exists(storagePath))
                    {
                        var storageText = File.ReadAllText(storagePath, Encoding.UTF8);

                        if (!string.IsNullOrEmpty(storageText))
                        {
                            try
                            {
                                var files = JArray.Parse(storageText);

                                foreach (var file in files)
                                {
                                    var bucketName = (string)file["path"];

                                    if (!await storage.ObjectExists(bucketName, file["unique_name"].ToString()))
                                    {
                                        using (var fileStream = new FileStream(
                                            Path.Combine(path, "files", file["file_name"].ToString()),
                                            FileMode.OpenOrCreate))
                                        {
                                            try
                                            {
                                                await storage.Upload(file["file_name"].ToString(), bucketName,
                                                    file["unique_name"].ToString(), fileStream);
                                            }
                                            catch (Exception)
                                            {
                                                File.AppendAllText(logPath,
                                                    "\u001b[90m" + DateTime.Now + "\u001b[39m" +
                                                    $" : File {file["file_name"]} not uploaded. Unique name is {file["unique_name"]}..." +
                                                    Environment.NewLine);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                File.AppendAllText(logPath,
                                    "\u001b[90m" + DateTime.Now + "\u001b[39m" +
                                    $" : \u001b[93m Unhandle exception while applying storage file... Error : {e.Message} \u001b[39m" +
                                    Environment.NewLine);
                            }
                        }
                    }

                    File.AppendAllText(logPath,
                        "\u001b[90m ------------------------------------------ \u001b[39m" + Environment.NewLine);
                    releaseList.Last().Status = ReleaseStatus.Succeed;
                    releaseList.Last().EndTime = DateTime.Now;
                }
                catch (Exception e)
                {
                    Directory.Delete(Path.Combine(rootPath, "packages", dbName), true);
                    File.AppendAllText(logPath,
                        "\u001b[90m" + DateTime.Now + "\u001b[39m" +
                        " : \u001b[93m Error - Unhandle exception \u001b[39m" + Environment.NewLine);
                    ErrorHandler.LogError(e,
                        "PublishHelper ApplyVersions method error. AppId: " + app["id"] + " - Version: " + version);
                    releaseList.Last().Status = ReleaseStatus.Failed;
                    releaseList.Last().EndTime = DateTime.Now;
                    if (File.Exists(Path.Combine(rootPath, "packages", databaseName, version.ToString())))
                        Directory.Delete(Path.Combine(rootPath, "packages", databaseName, version.ToString()), true);

                    return releaseList;
                }

                Directory.Delete(Path.Combine(rootPath, "packages", databaseName, version.ToString()), true);
            }

            if (templateCopied)
            {
                File.AppendAllText(Path.Combine(root, logFileName),
                    "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Template database swapping..." +
                    Environment.NewLine);
                result = PostgresHelper.SwapDatabase(PREConnectionString, $"{databaseName}_copy", databaseName);

                if (!result)
                    File.AppendAllText(Path.Combine(root, logFileName),
                        "\u001b[90m" + DateTime.Now + "\u001b[39m" +
                        " : \u001b[93m Error - Unhandle exception while swapping database... \u001b[39m" +
                        Environment.NewLine);
            }

            File.AppendAllText(Path.Combine(root, logFileName),
                "\u001b[92m" + "********** Apply Version End**********" + "\u001b[39m" + Environment.NewLine);
            return releaseList;
        }

        public static async Task<string> AddClientUrl(string authUrl, string token, string url, bool useSsl = false)
        {
            using (var client = new HttpClient())
            {
                var clientRequest = new JObject
                {
                    ["urls"] = (useSsl ? "https://" : "http://") + url,
                };

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync(authUrl + "/api/client/add_url",
                    new StringContent(JsonConvert.SerializeObject(clientRequest), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                        return "Unauthorized";

                    var resp = await response.Content.ReadAsStringAsync();
                    return resp;
                }

                return null;
            }
        }

        public static async Task<string> CreateClient(string appUrl, string authUrl, string token, string appName,
            string appLabel, string secret, bool useSsl = false)
        {
            using (var client = new HttpClient())
            {
                var clientRequest = new JObject
                {
                    ["client_id"] = appName,
                    ["client_name"] = appLabel,
                    ["allowed_grant_types"] = "password",
                    ["allow_remember_consent"] = false,
                    ["always_send_client_claims"] = true,
                    ["require_consent"] = false,
                    ["client_secrets"] = secret,
                    ["redirect_uris"] = (useSsl ? "https://" : "http://") + $"{appUrl}/signin-oidc",
                    ["post_logout_redirect_uris"] = (useSsl ? "https://" : "http://") + $"{appUrl}/signout-callback-oidc",
                    ["allowed_scopes"] = "openid;profile;email;api1",
                    ["access_token_life_time"] = 864000
                };

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync(authUrl + "/api/client/create",
                    new StringContent(JsonConvert.SerializeObject(clientRequest), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                        return "Unauthorized";

                    var resp = await response.Content.ReadAsStringAsync();
                    return resp;
                }

                return null;
            }
        }

        public static async Task<string> GetAuthToken(string authUrl, string studioSecret, string integrationEmail,
            string clientId, string clientSecret)
        {
            using (var httpClient = new HttpClient())
            {
                var dict = new Dictionary<string, string>
                {
                    {"grant_type", "password"},
                    {"username", integrationEmail},
                    {"password", studioSecret},
                    {"client_id", clientId},
                    {"client_secret", clientSecret}
                };

                var req = new HttpRequestMessage(HttpMethod.Post, authUrl + "/connect/token")
                    {Content = new FormUrlEncodedContent(dict)};
                var res = await httpClient.SendAsync(req);

                if (res.IsSuccessStatusCode)
                {
                    var resp = await res.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(resp))
                    {
                        var obj = JObject.Parse(resp);
                        var token = obj["access_token"].ToString();

                        return token;
                    }

                    return null;
                }

                return null;
            }
        }

        public static bool SyncAppTemplates(string connectionString, JObject app)
        {
            try
            {
                var templates = JArray.Parse(app["templates"].ToString());

                foreach (var template in templates)
                {
                    var settings = JObject.Parse(template["settings"].ToString());
                    var selectSql = $"SELECT id from app_templates WHERE settings->>'id' = '" + settings["id"] + "';";

                    var exists = PostgresHelper.Read(connectionString, "platform", selectSql, "hasRows");
                    var sql = "";

                    if (exists)
                        sql = $"UPDATE public.app_templates SET created_by = 1, updated_by = 1, created_at = '" +
                              template["created_at"] + "', updated_at = '" +
                              DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture) +
                              "', deleted = '" + (bool.Parse(template["deleted"].ToString()) ? "t" : "f") +
                              "', name = '" + template["name"] + "', subject = '" + template["subject"] +
                              "', content = '" + template["content"] + "', language = '" + template["language"] +
                              "', type = " + (int)template["type"] + ", system_code = '" + template["system_code"] +
                              "', active = '" + (bool.Parse(template["active"].ToString()) ? "t" : "f") +
                              "', settings = '" + template["settings"] + "' WHERE settings->>'id' = '" +
                              settings["id"] + "';";

                    else
                        sql =
                            $"INSERT INTO \"public\".\"app_templates\"(\"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"app_id\", \"name\", \"subject\", \"content\", \"language\", \"type\", \"system_code\", \"active\", \"settings\") VALUES (1, NULL,  '" +
                            DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture) +
                            "', NULL,'" + (bool.Parse(template["deleted"].ToString()) ? "t" : "f") + "', " + app["id"] +
                            ",'" + template["name"] + "', '" + template["subject"] + "', '" + template["content"] +
                            "', '" + template["language"] + "', " + (int)template["type"] + ", '" +
                            template["system_code"] + "', '" + (bool.Parse(template["active"].ToString()) ? "t" : "f") +
                            "', '" + template["settings"] + "');";

                    try
                    {
                        PostgresHelper.Run(connectionString, "platform", sql);
                    }
                    catch (Exception e)
                    {
                        ErrorHandler.LogError(e,
                            $"PublishHelper CreatePlatformApp method error. AppId : {app["id"]}, Template settings: {template["settings"]}");
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                ErrorHandler.LogError(e, $"PublishHelper CreatePlatformApp method error. AppId : {app["id"]}");
                return false;
            }
        }

        public static bool CreatePlatformApp(string connectionString, JObject app, string secret, string appUrl,
            string authUrl)
        {
            try
            {
                var options = JObject.Parse(app["setting"]["options"].ToString());
                var sqls = new JArray
                {
                    $"INSERT INTO \"public\".\"apps\"(\"id\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"name\", \"label\", \"description\", \"logo\", \"use_tenant_settings\", \"app_draft_id\", \"secret\") VALUES (" +
                    app["id"] + ", 1, NULL, '" +
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture) +
                    "', NULL, 'f', '" + app["name"] + "', '" + app["label"] + "', '" + app["description"] + "', '" +
                    app["logo"] + "', '" + app["use_tenant_settings"] + "', 0, '" + secret + "');",
                    $"INSERT INTO \"public\".\"app_settings\"(\"app_id\", \"app_domain\", \"auth_domain\", \"currency\", \"culture\", \"time_zone\", \"language\", \"auth_theme\", \"app_theme\", \"mail_sender_name\", \"mail_sender_email\", \"google_analytics_code\", \"tenant_operation_webhook\", \"registration_type\", \"enable_registration\", \"enable_api_registration\") VALUES (" +
                    app["id"] + ", '" + appUrl + "', '" + authUrl + "', " +
                    (!string.IsNullOrEmpty(app["setting"]["currency"].ToString())
                        ? "'" + app["setting"]["currency"] + "'"
                        : "NULL") + ", " +
                    (!string.IsNullOrEmpty(app["setting"]["culture"].ToString())
                        ? "'" + app["setting"]["culture"] + "'"
                        : "NULL") + ", " +
                    (!string.IsNullOrEmpty(app["setting"]["time_zone"].ToString())
                        ? "'" + app["setting"]["time_zone"] + "'"
                        : "NULL") + ", " +
                    (!string.IsNullOrEmpty(app["setting"]["language"].ToString())
                        ? "'" + app["setting"]["language"] + "'"
                        : "NULL") + ", " +
                    /**.ToString().Replace("'","''") yapma sebebimiz, 
                     * branding'te oluşturulan app ve auth themelerde tek tırnak içeren  stringlerin olmasıdır.
                     * Mevcutta olan tek tırnaklar, oluşturulacak olan scriptin yapısını bozmaktaydı.
                     */
                    (!app["setting"]["auth_theme"].IsNullOrEmpty()
                        ? "'" + app["setting"]["auth_theme"].ToString().Replace("'", "''") + "'"
                        : "NULL") + ", " +
                    (!app["setting"]["app_theme"].IsNullOrEmpty() ? "'" + app["setting"]["app_theme"].ToString().Replace("'", "''") + "'" : "NULL") +
                    ", " +
                    (!string.IsNullOrEmpty(app["setting"]["mail_sender_name"].ToString())
                        ? "'" + app["setting"]["mail_sender_name"] + "'"
                        : "NULL") + ", " +
                    (!string.IsNullOrEmpty(app["setting"]["mail_sender_email"].ToString())
                        ? "'" + app["setting"]["mail_sender_email"] + "'"
                        : "NULL") + ", " +
                    (!string.IsNullOrEmpty(app["setting"]["google_analytics_code"].ToString())
                        ? "'" + app["setting"]["google_analytics_code"] + "'"
                        : "NULL") + ", " +
                    (!string.IsNullOrEmpty(app["setting"]["tenant_operation_webhook"].ToString())
                        ? "'" + app["setting"]["tenant_operation_webhook"] + "'"
                        : "NULL") + ", 2, '" +
                    (options["enable_registration"] != null ? options["enable_registration"].ToString().Substring(0, 1).ToLower() : "t") + "'," +
                    (options["enable_api_registration"] != null ? options["enable_api_registration"].ToString().Substring(0, 1).ToLower() : "'t'") + ");"
                };


                foreach (var sql in sqls)
                {
                    PostgresHelper.Run(connectionString, "platform", sql.ToString());
                }

                return true;
            }
            catch (Exception e)
            {
                ErrorHandler.LogError(e, "PublishHelper CreatePlatformApp method error.");
                return false;
            }
        }

        public static bool UpdatePlatformApp(string connectionString, JObject app, string appUrl, string authUrl)
        {
            try
            {
                var options = JObject.Parse(app["setting"]["options"].ToString());
                var sqls = new JArray
                {
                    $"UPDATE public.apps SET created_by = 1, updated_by = 1, created_at = '" + app["created_at"] +
                    "', updated_at = '" +
                    DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture) +
                    "', label = '" + app["label"] + "', description = '" + app["description"] + "', logo = '" +
                    app["logo"] + "', use_tenant_settings = '" + app["use_tenant_settings"] +
                    "', app_draft_id = 0 WHERE id = " + app["id"] + ";",
                    $"UPDATE public.app_settings SET " +
                    (!string.IsNullOrEmpty(appUrl) ? $"app_domain = '{appUrl}'" : "") +
                    (!string.IsNullOrEmpty(authUrl) ? $"auth_domain = '{authUrl}'" : "") + " currency = " +
                    (!string.IsNullOrEmpty(app["setting"]["currency"].ToString())
                        ? "'" + app["setting"]["currency"] + "'"
                        : "NULL") + ", culture = " +
                    (!string.IsNullOrEmpty(app["setting"]["culture"].ToString())
                        ? "'" + app["setting"]["culture"] + "'"
                        : "NULL") + ", time_zone = " +
                    (!string.IsNullOrEmpty(app["setting"]["time_zone"].ToString())
                        ? "'" + app["setting"]["time_zone"] + "'"
                        : "NULL") + ", language = " +
                    (!string.IsNullOrEmpty(app["setting"]["language"].ToString())
                        ? "'" + app["setting"]["language"] + "'"
                        : "NULL") + ", auth_theme = " +
                    (!app["setting"]["auth_theme"].IsNullOrEmpty()
                        /**.ToString().Replace("'","''") yapma sebebimiz, 
                         * branding'te oluşturulan app ve auth themelerde tek tırnak içeren  stringlerin olmasıdır.
                         * Mevcutta olan tek tırnaklar, oluşturulacak olan scriptin yapısını bozmaktaydı.
                         */
                        ? "'" + app["setting"]["auth_theme"].ToString().Replace("'", "''") + "'"
                        : "NULL") + ", app_theme = " +
                    (!app["setting"]["app_theme"].IsNullOrEmpty() ? "'" + app["setting"]["app_theme"].ToString().Replace("'", "''") + "'" : "NULL") +
                    ", mail_sender_name = " +
                    (!string.IsNullOrEmpty(app["setting"]["mail_sender_name"].ToString())
                        ? "'" + app["setting"]["mail_sender_name"] + "'"
                        : "NULL") + ", mail_sender_email = " +
                    (!string.IsNullOrEmpty(app["setting"]["mail_sender_email"].ToString())
                        ? "'" + app["setting"]["mail_sender_email"] + "'"
                        : "NULL") + ", google_analytics_code = " +
                    (!string.IsNullOrEmpty(app["setting"]["google_analytics_code"].ToString())
                        ? "'" + app["setting"]["google_analytics_code"] + "'"
                        : "NULL") + ", tenant_operation_webhook = " +
                    (!string.IsNullOrEmpty(app["setting"]["tenant_operation_webhook"].ToString())
                        ? "'" + app["setting"]["tenant_operation_webhook"] + "'"
                        : "NULL") + "," +
                    "registration_type = 2," +
                    "enable_registration = '" + options["enable_registration"].ToString().Substring(0, 1).ToLower() + "'," +
                    "enable_api_registration = '" + options["enable_api_registration"].ToString().Substring(0, 1).ToLower() + "'" +
                    " WHERE app_id = " + app["id"] + ";"
                };

                foreach (var sql in sqls)
                {
                    PostgresHelper.Run(connectionString, "platform", sql.ToString());
                }

                return true;
            }
            catch (Exception e)
            {
                ErrorHandler.LogError(e, "PublishHelper UpdatePlatformApp method error.");
                return false;
            }
        }

        private static void EfMigrate(IConfiguration configuration, string databaseName)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TenantDBContext>();
            var connString = Postgres.GetConnectionString(configuration.GetConnectionString("TenantDBConnection"), databaseName);
            optionsBuilder.UseNpgsql(connString, x => x.MigrationsHistoryTable("_migration_history", "public")).ReplaceService<IHistoryRepository, HistoryRepository>();
            using (var tenantDatabaseContext = new TenantDBContext(optionsBuilder.Options, configuration))
            {
                var migrator = tenantDatabaseContext.Database.GetService<IMigrator>();
                var pendingMigrations = tenantDatabaseContext.Database.GetPendingMigrations().ToList();
                if (pendingMigrations.Any())
                {
                    try
                    {
                        foreach (var targetMigration in pendingMigrations)
                        {
                            migrator.Migrate(targetMigration);
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.LogError(ex, "PublishHelper EfMigrate DbName: " + databaseName);
                    }
                }
            }
        }
    }
}