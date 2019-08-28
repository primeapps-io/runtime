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
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.Model.Common.Document;
using PrimeApps.Util.Storage;

namespace PrimeApps.Model.Helpers
{
    public class PublishHelper
    {
        public static async Task ApplyVersions(IConfiguration configuration, IUnifiedStorage storage, JObject app, int orgId, string dbName, List<string> versions, string studioSecret, bool firstTime, int releaseId, string studioToken)
        {
            var PREConnectionString = configuration.GetConnectionString("PlatformDBConnection");
            var postgresPath = configuration.GetValue("AppSettings:PostgresPath", string.Empty);
            var studioUrl = configuration.GetValue("AppSettings:StudioUrl", string.Empty);

            var rootPath = configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);

            var logPath = $"{rootPath}\\release-logs";

            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            logPath += $"\\release-{releaseId}-log.txt";

            File.AppendAllText(logPath, "\u001b[92m" + "********** Publish **********" + "\u001b[39m" + Environment.NewLine);

            foreach (var obj in versions.OfType<object>().Select((version, index) => new {version, index}))
            {
                var version = obj.version;

                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + $" : Applying version is {version}..." + Environment.NewLine);

                try
                {
                    var path = $"{rootPath}releases\\{dbName}\\{version}";

                    if (Directory.Exists(path))
                        Directory.Delete(path);

                    Directory.CreateDirectory(path);

                    /*
                     * Download version folder from studio storage.
                     */

                    var authUrl = configuration.GetValue("AppSettings:AuthenticationServerURL", string.Empty);
                    var integrationEmail = configuration.GetValue("AppSettings:IntegrationEmail", string.Empty);
                    var clientId = configuration.GetValue("AppSettings:ClientId", string.Empty);
                    var token = await GetAuthToken(authUrl, studioSecret, integrationEmail, clientId);

                    using (var httpClient = new HttpClient())
                    {
                        var url = studioUrl + "/storage/download_folder";
                        httpClient.BaseAddress = new Uri(url);
                        httpClient.DefaultRequestHeaders.Accept.Clear();
                        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                        httpClient.DefaultRequestHeaders.Add("X-Organization-Id", orgId.ToString());
                        httpClient.DefaultRequestHeaders.Add("X-App-Id", app["id"].ToString());
                        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {studioToken}");

                        var request = new JObject
                        {
                            ["bucket_name"] = dbName,
                            ["version"] = version.ToString()
                        };

                        var response = await httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

                        if (!response.IsSuccessStatusCode)
                            File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Error - Unhandle exception. While downloading folder." + "\u001b[39m" + Environment.NewLine);

                        var fileByte = await response.Content.ReadAsByteArrayAsync();
                        Directory.CreateDirectory(path);
                        File.WriteAllBytes(path + $"\\{version}.zip", fileByte);
                        ZipFile.ExtractToDirectory(path + $"\\{version}.zip", path);
                    }

                    var scriptPath = $"{path}\\scripts.txt";
                    var storagePath = $"{path}\\storage.txt";

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
                            File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Error - Unhandle exception. While creating database." + "\u001b[39m" + Environment.NewLine);
                            break;
                        }

                        var restoreResult =
                            PosgresHelper.Restore(PREConnectionString, dbName, postgresPath, $"{path}");

                        if (!restoreResult)
                        {
                            File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Error - Unhandle exception. While restoring your database." + "\u001b[39m" + Environment.NewLine);
                            break;
                        }
                    }
                    else
                    {
                        File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Database removing template mark..." + Environment.NewLine);
                        result = ReleaseHelper.ChangeTemplateDatabaseStatus(PREConnectionString, dbName, true, scriptPath);

                        if (!result)
                        {
                            File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Error - Unhandle exception while removing database as template mark... \u001b[39m" + Environment.NewLine);
                            break;
                        }
                    }

                    var scriptsText = File.ReadAllText(scriptPath, Encoding.UTF8);
                    var sqls = scriptsText.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Scripts applying..." + Environment.NewLine);

                    foreach (var sql in sqls)
                    {
                        if (!string.IsNullOrEmpty(sql))
                        {
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

                                foreach (var file in files)
                                {
                                }
                            }
                            catch (Exception e)
                            {
                                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while applying storage files... \u001b[39m" + Environment.NewLine);
                            }
                        }
                    }


                    /*var ext = Path.GetExtension(parser.Filename);
                    var uniqueName = Guid.NewGuid().ToString().Replace("-", "") + ext;

                    using (Stream stream = new MemoryStream(parser.FileContents))
                    {
                        await _storage.Upload(bucketName, uniqueName, stream);
                    }

                    var result = new DocumentUploadResult
                    {
                        ContentType = parser.ContentType,
                        UniqueName = uniqueName,
                        Chunks = 0
                    };*/


                    if (firstTime && obj.index == 0)
                    {
                        File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Final arrangements being made..." + Environment.NewLine);

                        var secret = Guid.NewGuid().ToString().Replace("-", string.Empty);
                        var secretEncrypt = CryptoHelper.Encrypt(secret);
                        result = ReleaseHelper.CreatePlatformApp(PREConnectionString, app, secretEncrypt, scriptPath);

                        if (!result)
                            File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while creating platform app... \u001b[39m" + Environment.NewLine);

                        if (!string.IsNullOrEmpty(token))
                        {
                            var appUrl = string.Format(configuration.GetValue("AppSettings:AppUrl", string.Empty), app["name"].ToString());
                            var addClientResult = await CreateClient(appUrl, authUrl, token, app["name"].ToString(), app["label"].ToString(), secret);

                            if (string.IsNullOrEmpty(addClientResult))
                            {
                                var clientResult = await AddClientUrl(configuration.GetValue("AppSettings:AuthenticationServerURL", string.Empty), token, appUrl);
                                if (!string.IsNullOrEmpty(clientResult))
                                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while adding app url to client... \u001b[39m" + Environment.NewLine);
                            }
                            else
                                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while creating app client... Error: " + addClientResult + " \u001b[39m" + Environment.NewLine);
                        }
                    }

                    File.AppendAllText(logPath, "\u001b[90m ------------------------------------------ \u001b[39m" + Environment.NewLine);
                }
                catch (Exception e)
                {
                    Directory.Delete($"{rootPath}releases\\{dbName}");
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Error - Unhandle exception \u001b[39m" + Environment.NewLine);
                    break;
                }
            }

            File.AppendAllText(logPath, "\u001b[92m" + "********** Publish End**********" + "\u001b[39m" + Environment.NewLine);
        }

        public static async Task<string> AddClientUrl(string authUrl, string token, string url)
        {
            using (var client = new HttpClient())
            {
                var clientRequest = new JObject
                {
                    ["urls"] = url,
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

        public static async Task<string> CreateClient(string appUrl, string authUrl, string token, string appName, string appLabel, string secret)
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
                    ["redirect_uris"] = $"{appUrl}/signin-oidc",
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