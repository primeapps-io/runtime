using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Devart.Data.PostgreSql;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.Util.Storage;

namespace PrimeApps.Model.Helpers
{
    public class PublishHelper
    {
        public static async Task<bool> Create(IConfiguration configuration, IUnifiedStorage storage, JObject app, string dbName, int version, string studioSecret, bool firstPublish)
        {
            try
            {
                var PREConnectionString = configuration.GetConnectionString("PlatformDBConnection");

                var bucketName = UnifiedStorage.GetPath("releases", "app", int.Parse(app["id"].ToString()), "/" + version + "/");

                var path = configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);

                path = $"{path}releases\\{dbName}\\{version}";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    await storage.CopyBucket(bucketName, path);
                }

                var logPath = $"{path}\\log.txt";
                var scriptPath = $"{path}\\scripts.txt";
                bool result;

                File.AppendAllText(logPath, "\u001b[92m" + "********** Publish **********" + "\u001b[39m" + Environment.NewLine);
                if (firstPublish)
                {
                    //var sqlDump = await storage.GetObject(bucketName, "sqlDump.txt");
                    // Issue request and remember to dispose of the response

                    var dumpText = File.ReadAllText($"{path}\\dumpSql.txt", Encoding.UTF8);

                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Restoring your database..." + Environment.NewLine);
                    var restoreResult = ReleaseHelper.RestoreDatabase(PREConnectionString, dumpText, dbName);

                    if (!restoreResult)
                        File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Unhandle exception. While restoring your database." + "\u001b[39m" + Environment.NewLine);
                }
                else
                {
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Database removing template mark..." + Environment.NewLine);
                    result = ReleaseHelper.ChangeTemplateDatabaseStatus(PREConnectionString, dbName, true, scriptPath);

                    if (!result)
                        File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while removing database as template mark... \u001b[39m" + Environment.NewLine);
                }

                var scriptsText = File.ReadAllText($"{path}\\scripts.txt", Encoding.UTF8);
                var sqls = scriptsText.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Scripts applying..." + Environment.NewLine);

                foreach (var sql in sqls)
                {
                    if (!string.IsNullOrEmpty(sql))
                    {
                        var restoreResult = ReleaseHelper.ScriptApply(PREConnectionString, dbName, sql);

                        if (!restoreResult)
                            File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Unhandle exception. While applying script. Script is : (" + sql + ")" + "\u001b[39m" + Environment.NewLine);
                    }
                }

                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Database marking as template..." + Environment.NewLine);
                result = ReleaseHelper.ChangeTemplateDatabaseStatus(PREConnectionString, dbName, false, scriptPath);

                if (!result)
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while marking database as template... \u001b[39m" + Environment.NewLine);

                if (firstPublish)
                {
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Final arrangements being made..." + Environment.NewLine);
                    var secret = Guid.NewGuid().ToString().Replace("-", string.Empty);
                    var secretEncrypt = CryptoHelper.Encrypt(secret);
                    result = ReleaseHelper.CreatePlatformApp(PREConnectionString, app, secretEncrypt, scriptPath);

                    if (!result)
                        File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while creating platform app... \u001b[39m" + Environment.NewLine);

                    var authUrl = configuration.GetValue("AppSettings:AuthenticationServerURL", string.Empty);
                    var integrationEmail = configuration.GetValue("AppSettings:IntegrationEmail", string.Empty);
                    var clientId = configuration.GetValue("AppSettings:ClientId", string.Empty);
                    var token = await GetAuthToken(authUrl, studioSecret, integrationEmail, clientId);

                    if (!string.IsNullOrEmpty(token))
                    {
                        var appUrl = string.Format(configuration.GetValue("AppSettings:AppUrl", string.Empty), app["name"].ToString());
                        var addClientResult = await CreateClient(appUrl, authUrl, token, app["name"].ToString(), app["label"].ToString(), secret, logPath);

                        if (addClientResult)
                        {
                            await AddClientUrl(configuration.GetValue("AppSettings:AuthenticationServerURL", string.Empty), token, appUrl, logPath);
                        }
                    }
                }

                File.AppendAllText(logPath, "\u001b[92m" + "********** Publish End**********" + "\u001b[39m" + Environment.NewLine);

                /*await storage.UploadDirAsync(bucketName, path);
                Directory.Delete(path);*/
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            return true;
        }

        public static async Task<bool> Update(IConfiguration configuration, IUnifiedStorage storage, JObject app, string dbName, int version, bool firstPublish)
        {
            try
            {
                var PREConnectionString = configuration.GetConnectionString("PlatformDBConnection");

                var bucketName = UnifiedStorage.GetPath("releases", "app", int.Parse(app["id"].ToString()), "/" + version + "/");

                var path = configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);

                path = $"{path}releases\\{dbName}\\{version}";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    await storage.CopyBucket(bucketName, path);
                }

                var logPath = $"{path}\\log.txt";
                var scriptPath = $"{path}\\scripts.txt";
                var storagePath = $"{path}\\storage.txt";

                File.AppendAllText(logPath, "\u001b[92m" + "********** Publish **********" + "\u001b[39m" + Environment.NewLine);

                var scriptsText = File.ReadAllText(scriptPath, Encoding.UTF8);
                var sqls = scriptsText.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Applying database scripts..." + Environment.NewLine);

                foreach (var sql in sqls)
                {
                    if (!string.IsNullOrEmpty(sql))
                    {
                        var restoreResult = ReleaseHelper.RunHistoryDatabaseScript(PREConnectionString, dbName, sql);

                        if (!restoreResult)
                            File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Unhandle exception. While applying script. Script is : (" + sql + ")" + "\u001b[39m" + Environment.NewLine);
                    }
                }

                var storagesText = File.ReadAllText(storagePath, Encoding.UTF8);
                sqls = storagesText.Split(new[] {Environment.NewLine}, StringSplitOptions.None);

                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Applying storage scripts..." + Environment.NewLine);

                foreach (var sql in sqls)
                {
                    if (!string.IsNullOrEmpty(sql))
                    {
                        var restoreResult = ReleaseHelper.RunHistoryDatabaseScript(PREConnectionString, dbName, sql);

                        if (!restoreResult)
                            File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Unhandle exception. While applying script. Script is : (" + sql + ")" + "\u001b[39m" + Environment.NewLine);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


            return true;
        }

        public static async Task<bool> AddClientUrl(string authUrl, string token, string url, string logPath = null)
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

                    if (!string.IsNullOrEmpty(logPath))
                        File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while adding app url to client... \u001b[39m" + Environment.NewLine);
                    else
                        throw new Exception(resp);

                    return false;
                }

                return true;
            }
        }

        public static async Task<bool> CreateClient(string appUrl, string authUrl, string token, string appName, string appLabel, string secret, string logPath = null)
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

                    if (!string.IsNullOrEmpty(logPath))
                        File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while creating app client... \u001b[39m" + Environment.NewLine);
                    else
                        throw new Exception(resp);

                    return false;
                }

                return true;
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