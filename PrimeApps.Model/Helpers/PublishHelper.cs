using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.Model.Common.Document;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories;
using PrimeApps.Util.Storage;

namespace PrimeApps.Model.Helpers
{
    public class PublishHelper
    {
        public static async Task<bool> UpdateTenant(string version, string dbName, IConfiguration configuration, IUnifiedStorage storage,
            int appId, int orgId, int currentReleaseId, string token)
        {
            var PREConnectionString = configuration.GetConnectionString("PlatformDBConnection");
            var rootPath = configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);
            var root = Path.Combine(rootPath, "tenant-update-logs");

            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            var available = PosgresHelper.Read(PREConnectionString, dbName, $"SELECT EXISTS (SELECT 1 FROM history_database WHERE tag = '{version}');");

            if (available.HasRows)
                return true;

            var logPath = Path.Combine(root, $"update-{dbName}-version-{version}-log.txt");

            File.AppendAllText(logPath, "\u001b[92m" + "********** Update Tenant **********" + "\u001b[39m" + Environment.NewLine);
            File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + $" : Applying version is {version}..." + Environment.NewLine);

            try
            {
                var path = Path.Combine(rootPath, "releases", $"app{appId}", version);

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
                            ["path"] = $"/releases/{version}",
                            ["key"] = $"{version}.zip"
                        };

                        var response = await httpClient.PostAsync(url,
                            new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                        if (!response.IsSuccessStatusCode)
                            File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Error - Unhandle exception. While downloading folder." + "\u001b[39m" + Environment.NewLine);

                        var fileByte = await response.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(Path.Combine(path, $"{version}.zip"), fileByte);
                        await Task.Delay(1000);
                        ZipFile.ExtractToDirectory(Path.Combine(path, $"{version}.zip"), path);
                    }
                }

                var scriptPath = Path.Combine(path, "scripts.txt");
                var storagePath = Path.Combine(path, "storage.txt");

                var scriptsText = File.ReadAllText(scriptPath, Encoding.UTF8);
                var sqls = scriptsText.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Scripts applying..." + Environment.NewLine);

                var result = PosgresHelper.RunAll(PREConnectionString, dbName, sqls);

                if (!result)
                {
                    File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Unhandle exception. While applying script. \u001b[39m" + Environment.NewLine);
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

                            var bucketName = UnifiedStorage.GetPath("template", "app", appId);

                            foreach (var file in files)
                            {
                                if (!await storage.ObjectExists(bucketName, file["unique_name"].ToString()))
                                {
                                    using (var fileStream = new FileStream(Path.Combine(path, "files", file["file_name"].ToString()), FileMode.OpenOrCreate))
                                    {
                                        try
                                        {
                                            await storage.Upload(file["file_name"].ToString(), bucketName, file["unique_name"].ToString(), fileStream);
                                        }
                                        catch (Exception e)
                                        {
                                            File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + $" : File {file["file_name"]} not uploaded. Unique name is {file["unique_name"]}..." + Environment.NewLine);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + $" : \u001b[93m Unhandle exception while applying storage file... Error : {e.Message} \u001b[39m" + Environment.NewLine);
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Error - Unhandle exception - Message: " + e.Message + " \u001b[39m" + Environment.NewLine);
                return false;
            }
        }

        public static async Task<List<Release>> ApplyVersions(IConfiguration configuration, IUnifiedStorage storage, JObject app, int orgId, string databaseName, List<string> versions, string studioSecret, bool firstTime, int currentReleaseId, string studioToken, string appUrl, string authUrl, bool useSsl)
        {
            var PREConnectionString = configuration.GetConnectionString("PlatformDBConnection");
            var postgresPath = configuration.GetValue("AppSettings:PostgresPath", string.Empty);
            var studioUrl = configuration.GetValue("AppSettings:StudioUrl", string.Empty);

            var rootPath = configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);
            var dbName = databaseName;
            var templateCopied = false;

            var root = Path.Combine(rootPath, "release-logs");

            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            var releaseList = new List<Release>();

            foreach (var obj in versions.OfType<object>().Select((version, index) => new {version, index}))
            {
                currentReleaseId += 1;
                var version = obj.version;

                releaseList.Add(new Release
                {
                    Id = currentReleaseId,
                    AppId = int.Parse(app["id"].ToString()),
                    StartTime = DateTime.Now,
                    Version = version.ToString()
                });

                var logPath = Path.Combine(root, $"release-{currentReleaseId}-log.txt");

                /*if (!File.Exists(logPath))
                    File.Create(logPath);*/

                File.AppendAllText(logPath, "\u001b[92m" + "********** Apply Version **********" + "\u001b[39m" + Environment.NewLine);

                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + $" : Applying version is {version}..." + Environment.NewLine);

                try
                {
                    var path = Path.Combine(rootPath, "releases", dbName, version.ToString());

                    if (Directory.Exists(path))
                        Directory.Delete(path, true);

                    Directory.CreateDirectory(path);

                    /*
                     * Download version folder from studio storage.
                     */

                    var identityUrl = configuration.GetValue("AppSettings:AuthenticationServerURL", string.Empty);
                    var integrationEmail = configuration.GetValue("AppSettings:IntegrationEmail", string.Empty);
                    var clientId = configuration.GetValue("AppSettings:ClientId", string.Empty);
                    var token = await GetAuthToken(identityUrl, studioSecret, integrationEmail, clientId);

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
                            ["path"] = $"/releases/{version}",
                            ["key"] = $"{version}.zip"
                        };

                        var response = await httpClient.PostAsync(url,
                            new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                        if (!response.IsSuccessStatusCode)
                            File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Error - Unhandle exception. While downloading folder." + "\u001b[39m" + Environment.NewLine);

                        var fileByte = await response.Content.ReadAsByteArrayAsync();
                        File.WriteAllBytes(Path.Combine(path, $"{version}.zip"), fileByte);
                        await Task.Delay(1000);
                        ZipFile.ExtractToDirectory(Path.Combine(path, $"{version}.zip"), path);
                    }

                    var scriptPath = Path.Combine(path, "scripts.txt");
                    var storagePath = Path.Combine(path, "storage.txt");

                    bool result;

                    //applyResult.Add("\u001b[92m" + "********** Publish **********" + "\u001b[39m");

                    if (firstTime && obj.index == 0)
                    {
                        //var sqlDump = await storage.GetObject(bucketName, "sqlDump.txt");
                        // Issue request and remember to dispose of the response

                        //var dumpText = File.ReadAllText($"{path}\\dumpSql.txt", Encoding.UTF8);

                        File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Restoring your database..." + Environment.NewLine);

                        var createDbResult = ReleaseHelper.CreateDatabaseIfNotExists(PREConnectionString, dbName);

                        if (!createDbResult)
                        {
                            releaseList.Last().Status = ReleaseStatus.Failed;
                            releaseList.Last().EndTime = DateTime.Now;
                            File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Error - Unhandle exception. While creating database." + "\u001b[39m" + Environment.NewLine);
                            break;
                        }

                        var restoreResult = PosgresHelper.Restore(PREConnectionString, dbName, postgresPath, $"{path}");

                        if (!restoreResult)
                        {
                            releaseList.Last().Status = ReleaseStatus.Failed;
                            releaseList.Last().EndTime = DateTime.Now;
                            File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Error - Unhandle exception. While restoring your database." + "\u001b[39m" + Environment.NewLine);
                            break;
                        }
                    }
                    else
                    {
                        if (!templateCopied)
                        {
                            ReleaseHelper.RemoveTemplateDatabase(PREConnectionString, $"{dbName}_copy");
                            File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Template database copying template database..." + Environment.NewLine);
                            result = ReleaseHelper.CopyDatabase(PREConnectionString, dbName);

                            if (!result)
                            {
                                releaseList.Last().Status = ReleaseStatus.Failed;
                                releaseList.Last().EndTime = DateTime.Now;
                                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Error - Unhandle exception while copying template database... \u001b[39m" + Environment.NewLine);
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
                    }

                    var scriptsText = File.ReadAllText(scriptPath, Encoding.UTF8);
                    var sqls = scriptsText.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Scripts applying..." + Environment.NewLine);

                    foreach (var sql in sqls)
                    {
                        if (!string.IsNullOrEmpty(sql))
                        {
                            var tableName = ReleaseHelper.GetTableName(sql).Replace("public.", "");
                            PosgresHelper.Run(PREConnectionString, dbName, $"SELECT SETVAL('{tableName}_id_seq', (SELECT MAX(id) FROM {tableName}));");
                            var restoreResult = PosgresHelper.Run(PREConnectionString, dbName, sql);

                            if (!restoreResult)
                                File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Unhandle exception. While applying script. Script is : (" + sql + ")" + "\u001b[39m" + Environment.NewLine);
                        }
                    }

                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Database marking as template..." + Environment.NewLine);

                    result = ReleaseHelper.ChangeTemplateDatabaseStatus(PREConnectionString, dbName, false, scriptPath);

                    if (!result)
                        File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while marking database as template... \u001b[39m" + Environment.NewLine);

                    if (File.Exists(storagePath))
                    {
                        var storageText = File.ReadAllText(storagePath, Encoding.UTF8);

                        if (!string.IsNullOrEmpty(storageText))
                        {
                            try
                            {
                                var files = JArray.Parse(storageText);

                                var bucketName = UnifiedStorage.GetPath("template", "app", int.Parse(app["id"].ToString()));

                                foreach (var file in files)
                                {
                                    if (!await storage.ObjectExists(bucketName, file["unique_name"].ToString()))
                                    {
                                        using (var fileStream = new FileStream(Path.Combine(path, "files", file["file_name"].ToString()), FileMode.OpenOrCreate))
                                        {
                                            try
                                            {
                                                await storage.Upload(file["file_name"].ToString(), bucketName, file["unique_name"].ToString(), fileStream);
                                            }
                                            catch (Exception e)
                                            {
                                                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + $" : File {file["file_name"]} not uploaded. Unique name is {file["unique_name"]}..." + Environment.NewLine);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + $" : \u001b[93m Unhandle exception while applying storage file... Error : {e.Message} \u001b[39m" + Environment.NewLine);
                            }
                        }
                    }

                    if (firstTime && obj.index == 0)
                    {
                        File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Final arrangements being made..." + Environment.NewLine);

                        var secret = Guid.NewGuid().ToString().Replace("-", string.Empty);
                        var secretEncrypt = CryptoHelper.Encrypt(secret);
                        result = ReleaseHelper.CreatePlatformApp(PREConnectionString, app, secretEncrypt, appUrl, authUrl, scriptPath);

                        if (!result)
                            File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while creating platform app... \u001b[39m" + Environment.NewLine);

                        if (!string.IsNullOrEmpty(token))
                        {
                            //var applicationUrl = string.Format(configuration.GetValue("AppSettings:AppUrl", string.Empty), app["name"].ToString());
                            var addClientResult = await CreateClient(appUrl, identityUrl, token, app["name"].ToString(), app["label"].ToString(), secret, useSsl);

                            if (string.IsNullOrEmpty(addClientResult))
                            {
                                var clientResult = await AddClientUrl(configuration.GetValue("AppSettings:AuthenticationServerURL", string.Empty), token, appUrl, useSsl);
                                if (!string.IsNullOrEmpty(clientResult))
                                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while adding app url to client... \u001b[39m" + Environment.NewLine);
                            }
                            else
                                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while creating app client... Error: " + addClientResult + " \u001b[39m" + Environment.NewLine);
                        }
                    }
                    else if (!firstTime && obj.index == 0)
                    {
                        if (!string.IsNullOrEmpty(appUrl) && !string.IsNullOrEmpty(authUrl))
                        {
                            result = ReleaseHelper.UpdatePlatformApp(PREConnectionString, app, appUrl, authUrl, scriptPath);

                            if (!result)
                                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while updating platform app... \u001b[39m" + Environment.NewLine);
                        }
                    }

                    File.AppendAllText(logPath, "\u001b[90m ------------------------------------------ \u001b[39m" + Environment.NewLine);
                    releaseList.Last().Status = ReleaseStatus.Succeed;
                    releaseList.Last().EndTime = DateTime.Now;
                }
                catch (Exception e)
                {
                    Directory.Delete($"{rootPath}releases\\{dbName}", true);
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Error - Unhandle exception \u001b[39m" + Environment.NewLine);

                    releaseList.Last().Status = ReleaseStatus.Failed;
                    releaseList.Last().EndTime = DateTime.Now;
                    break;
                }

                Directory.Delete(Path.Combine(rootPath, "releases", databaseName, version.ToString()), true);
            }

            if (templateCopied)
            {
                File.AppendAllText(Path.Combine(root, $"releases-{currentReleaseId}-log.txt"), "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Template database swapping..." + Environment.NewLine);
                var result = ReleaseHelper.SwapCopyDatabase(PREConnectionString, databaseName);

                if (!result)
                    File.AppendAllText($"{root}\\release-{currentReleaseId}-log.txt", "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Error - Unhandle exception while swapping database... \u001b[39m" + Environment.NewLine);
            }

            File.AppendAllText(Path.Combine(root, $"releases-{currentReleaseId}-log.txt"), "\u001b[92m" + "********** Apply Version End**********" + "\u001b[39m" + Environment.NewLine);
            return releaseList;
        }

        public static async Task<string> AddClientUrl(string authUrl, string token, string url, bool useSsl = false)
        {
            using (var client = new HttpClient())
            {
                var clientRequest = new JObject
                {
                    ["urls"] = useSsl ? "https://" : "http://" + url,
                };

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync(authUrl + "/api/client/add_url", new StringContent(JsonConvert.SerializeObject(clientRequest), Encoding.UTF8, "application/json"));


                if (!response.IsSuccessStatusCode)
                {
                    var resp = await response.Content.ReadAsStringAsync();
                    return resp;
                }

                return null;
            }
        }

        public static async Task<string> CreateClient(string appUrl, string authUrl, string token, string appName, string appLabel, string secret, bool useSsl = false)
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
                    ["redirect_uris"] = useSsl ? "https://" : "http://" + $"{appUrl}/signin-oidc",
                    ["post_logout_redirect_uris"] = $"{appUrl}/signout-callback-oidc",
                    ["allowed_scopes"] = "openid;profile;email;api1",
                    ["access_token_life_time"] = 864000
                };

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync(authUrl + "/api/client/create", new StringContent(JsonConvert.SerializeObject(clientRequest), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var resp = await response.Content.ReadAsStringAsync();

                    return resp;
                }

                return null;
            }
        }

        public static async Task<string> GetAuthToken(string authUrl, string studioSecret, string integrationEmail, string clientId)
        {
            using (var httpClient = new HttpClient())
            {
                var dict = new Dictionary<string, string>
                {
                    {"grant_type", "password"},
                    {"username", integrationEmail},
                    {"password", studioSecret},
                    {"client_id", clientId},
                    {"client_secret", studioSecret}
                };

                var req = new HttpRequestMessage(HttpMethod.Post, authUrl + "/connect/token") {Content = new FormUrlEncodedContent(dict)};
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
    }
}