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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Util.Storage;

namespace PrimeApps.Model.Helpers
{
    public enum Location
    {
        [EnumMember(Value = "PDE")]PDE = 1,

        [EnumMember(Value = "PRE")]PRE = 2
    }

    public class ReleaseHelper
    {
        public static async Task<bool> All(JObject app, string studioSecret, bool clearAllRecords, string dbName, string version, IConfiguration configuration, IUnifiedStorage storage, List<HistoryStorage> historyStorages)
        {
            var PDEConnectionString = configuration.GetConnectionString("StudioDBConnection");
            var postgresPath = configuration.GetValue("AppSettings:PostgresPath", string.Empty);
            var root = configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);

            var path = Path.Combine(root, "packages", dbName, version);

            if (Directory.Exists(path))
                Directory.Delete(path, true);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var logPath = Path.Combine(path, "log.txt"); //Path.Combine(path,"log.txt") 
            var scriptPath = Path.Combine(path, "scripts.txt");
            var storagePath = Path.Combine(path, "storage.txt");
            var storageFilesPath = Path.Combine(path, "files");

            if (!Directory.Exists(storageFilesPath))
                Directory.CreateDirectory(storageFilesPath);

            try
            {
                File.AppendAllText(logPath, "\u001b[92m" + "********** Create Package **********" + "\u001b[39m" + Environment.NewLine);
                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Database sql generating..." + Environment.NewLine);

                //var sqlDump = ReleaseHelper.GetSqlDump(PDEConnectionString, dbName, $"{path}\\dumpSql.txt");

                var sqlDumpResult = PosgresHelper.Dump(PDEConnectionString, dbName, postgresPath, $"{path}/");

                if (!sqlDumpResult)
                    File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Unhandle exception. While creating sql dump script." + "\u001b[39m" + Environment.NewLine);

                /*if (autoDistribute)
                {
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Restoring your database..." + Environment.NewLine);
                    var restoreResult = ReleaseHelper.RestoreDatabase(PREConnectionString, sqlDump, Location.PRE, dbName);

                    if (!restoreResult)
                        File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Unhandle exception. While restoring your database." + "\u001b[39m" + Environment.NewLine);
                }*/

                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Settings information adding..." + Environment.NewLine);

                var result = ReleaseHelper.CreateSettingsAppNameSql(app["name"].ToString());

                AddScript(scriptPath, result);

                /*if (!result)
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while adding app setting values... \u001b[39m" + Environment.NewLine);
                */

                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : System tables clearing..." + Environment.NewLine);
                result = ReleaseHelper.CleanUpSystemTablesSql();
                AddScript(scriptPath, result);

                /*if (!result)
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while clean-up system tables... \u001b[39m" + Environment.NewLine);
                */

                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Dynamic tables clearing..." + Environment.NewLine);
                //var arrayResult = ReleaseHelper.GetAllDynamicTables(PDEConnectionString, dbName);

                var arrayResult = PosgresHelper.Read(PDEConnectionString, dbName, GetAllDynamicTablesSql(), "array");

                if (arrayResult != null)
                {
                    foreach (var table in arrayResult)
                    {
                        var sql = $"DELETE FROM {table["table_name"]};";
                        AddScript(scriptPath, sql);
                    }
                }

                /*if (!result)
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while clearing dynamic tables... \u001b[39m" + Environment.NewLine);
                */

                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Records are marking as sample..." + Environment.NewLine);
                result = ReleaseHelper.SetRecordsIsSampleSql();

                AddScript(scriptPath, result);

                /*if (!result)
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while marking records as sample... \u001b[39m" + Environment.NewLine);
                */

                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Editing your records..." + Environment.NewLine);
                arrayResult = ReleaseHelper.GetAllUsersFKSql(PDEConnectionString, dbName);

                if (arrayResult != null)
                {
                    foreach (var sql in arrayResult)
                    {
                        AddScript(scriptPath, sql.ToString());
                    }
                }

                if (historyStorages != null)
                {
                    foreach (var (value, index) in historyStorages.Select((v, i) => (v, i)))
                    {
                        AddScript(storagePath, (index == 0 ? "[" : "") + new JObject {["mime_type"] = value.MimeType, ["operation"] = value.Operation, ["file_name"] = value.FileName, ["unique_name"] = value.UniqueName, ["path"] = value.Path}.ToJsonString() + (index != historyStorages.Count() - 1 ? "," : "]"));

                        var file = await storage.GetObject($"app{app["id"]}/templates/", value.UniqueName);
                        using (var fileStream = File.Create(Path.Combine(storageFilesPath, value.FileName)))
                        {
                            file.ResponseStream.CopyTo(fileStream);
                        }
                    }
                }

                try
                {
                    ZipFile.CreateFromDirectory(path, path = Path.Combine(root, "packages", dbName, $"{dbName}.zip"));
                }
                catch (Exception e)
                {
                }


                /*if (!result)
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while editing records... \u001b[39m" + Environment.NewLine);
                */

                /*File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Database marking as template..." + Environment.NewLine);
                result = PublishHelper.SetDatabaseIsTemplate(PREConnectionString, Location.PRE, dbName, scriptPath, autoDistribute);

                if (!result)
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while marking database as template... \u001b[39m" + Environment.NewLine);
                */

                /*File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Final arrangements being made..." + Environment.NewLine);
                var secret = Guid.NewGuid().ToString().Replace("-", string.Empty);
                var secretEncrypt = CryptoHelper.Encrypt(secret);
                result = PublishHelper.CreatePlatformApp(PREConnectionString, app, secretEncrypt, scriptPath, autoDistribute);

                if (!result)
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while creating platform app... \u001b[39m" + Environment.NewLine);
                
                if (autoDistribute)
                {
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
                */

                File.AppendAllText(logPath, "\u001b[92m" + "********** Package Created **********" + "\u001b[39m" + Environment.NewLine);

                /*if (goLive)
                    PublishHelper.Create(configuration, storage, app, dbName, version, studioSecret, app["status"].ToString() != "published");*/
                /*else
                {
                    var bucketName = UnifiedStorage.GetPath("packages", null, int.Parse(app["id"].ToString()), "/" + version + "/");
                    await storage.UploadDirAsync(bucketName, path);
                    Directory.Delete(path);
                }*/

                return true;
            }
            catch (Exception e)
            {
                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : " + "\u001b[31m" + "Error : " + e.Message + "\u001b[39m" + Environment.NewLine);

                //tw.WriteLine(e.Message);
                return false;
            }
        }

        public static async Task<bool> Diffs(List<HistoryDatabase> historyDatabases, List<HistoryStorage> historyStorages, JObject app, string studioSecret, string dbName, string version, int deploymentId, IConfiguration configuration, IUnifiedStorage storage)
        {
            var root = configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);

            var path = Path.Combine(root, "packages", dbName, version);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var logPath = Path.Combine(path, "log.txt");
            var scriptPath = Path.Combine(path, "scripts.txt");
            var storagePath = Path.Combine(path, "storage.txt");
            var storageFilesPath = Path.Combine(path, "files");

            if (!Directory.Exists(storageFilesPath))
                Directory.CreateDirectory(storageFilesPath);

            try
            {
                File.AppendAllText(logPath, "\u001b[92m" + "********** Create Package **********" + "\u001b[39m" + Environment.NewLine);
                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Database scripts creating..." + Environment.NewLine);

                if (historyDatabases != null)
                {
                    foreach (var table in historyDatabases)
                    {
                        try
                        {
                            AddScript(scriptPath, table.CommandText);
                            AddScript(scriptPath, $"INSERT INTO public.history_database (id, command_text, table_name, command_id, executed_at, created_by, deleted, tag) VALUES ({table.Id}, '{table.CommandText.Replace("'", "''")}', '{table.TableName ?? "NULL"}', '{table.CommandId}', '{Convert.ToDateTime(table.ExecutedAt).ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture)}', '{table.CreatedByEmail}', '{table.Deleted}', {(!string.IsNullOrEmpty(table.Tag) ? "'" + table.Tag + "'" : "NULL")});");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                }

                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Storage scripts creating..." + Environment.NewLine);

                var bucketName = UnifiedStorage.GetPath("packages", "app", int.Parse(app["id"].ToString()), version + "/");

                if (historyStorages != null)
                {
                    foreach (var (value, index) in historyStorages.Select((v, i) => (v, i)))
                    {
                        AddScript(storagePath, (index == 0 ? "[" : "") + new JObject {["mime_type"] = value.MimeType, ["operation"] = value.Operation, ["file_name"] = value.FileName, ["unique_name"] = value.UniqueName, ["path"] = value.Path}.ToJsonString() + (index != historyStorages.Count() - 1 ? "," : "]"));
                        AddScript(scriptPath, $"INSERT INTO public.history_storage (id, mime_type, operation, file_name, unique_name, path, executed_at, created_by, deleted, tag) VALUES ({value.Id}, '{value.MimeType}', '{value.Operation}', '{value.FileName}', '{value.UniqueName}', '{value.Path}', '{Convert.ToDateTime(value.ExecutedAt).ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture)}', '{value.CreatedByEmail}', '{value.Deleted}', {(!string.IsNullOrEmpty(value.Tag) ? "'" + value.Tag + "'" : "NULL")});");
                        //await storage.Download(bucketName, value.UniqueName, value.FileName);
                        var file = await storage.GetObject(value.Path, value.UniqueName);

                        using (var fileStream = File.Create(Path.Combine(storageFilesPath, $"{value.FileName}")))
                        {
                            //file.ResponseStream.Seek(0, SeekOrigin.Begin);
                            file.ResponseStream.CopyTo(fileStream);
                        }
                    }
                }

                //var bucketName = UnifiedStorage.GetPath("packages", "app", int.Parse(app["id"].ToString()), version + "/");

                try
                {
                    ZipFile.CreateFromDirectory(path, path = Path.Combine(root, "packages", dbName, $"{dbName}.zip"));
                }
                catch (Exception e)
                {
                }

                File.AppendAllText(logPath, "\u001b[92m" + "********** Package Created **********" + "\u001b[39m" + Environment.NewLine);

                /*if (historyStorages != null || historyDatabases != null)
                    await storage.UploadDirAsync(bucketName, path);

                Directory.Delete(path, true);*/

                /*if (goLive)
                    PublishHelper.Update(configuration, storage, app, dbName, version, app["status"].ToString() != "published");*/

                return true;
            }
            catch (Exception e)
            {
                File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : " + "\u001b[31m" + "Error : " + e.Message + "\u001b[39m" + Environment.NewLine);

                return false;
            }
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

        public static string GetConnectionString(string connectionString, string database)
        {
            var connection = new NpgsqlConnectionStringBuilder(connectionString);

            //license key=trial:{Environment.CurrentDirectory}\\devart.key
            /*if (location == Location.PDE)
            return $"host={connection.Host};port={connection.Port};user id={connection.Username};password={connection.Password};database={database};";*/

            return $"host={connection.Host};port={connection.Port};user id={connection.Username};password={connection.Password};database={database};";
        }

        public static string SetRecordsIsSampleSql()
        {
            return "SELECT 'update \"' || cl.\"table_name\" || '\" set is_sample=true;' " +
                   "FROM information_schema.\"columns\" cl " +
                   "WHERE cl.\"column_name\" = 'is_sample' " +
                   "ORDER BY cl.\"table_name\";";
        }

        /*public static bool SetRecordsIsSample(string connectionString, string dbName, string scriptPath = null)
        {
            try
            {
                var sql = SetRecordsIsSampleSql();

                if (!string.IsNullOrEmpty(scriptPath))
                    File.AppendAllText(scriptPath, sql + Environment.NewLine);

                var connection = new PgSqlConnection(GetConnectionString(connectionString, dbName));
                connection.Open();

                var dropCommand = new PgSqlCommand(sql) {Connection = connection};
                var result = dropCommand.ExecuteReader();

                connection.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }*/

        public static string CleanUpSystemTablesSql()
        {
            return $"DELETE FROM analytic_shares;" + Environment.NewLine +
                   "DELETE FROM audit_logs;" + Environment.NewLine +
                   "DELETE FROM changelogs;" + Environment.NewLine +
                   "DELETE FROM imports;" + Environment.NewLine +
                   "DELETE FROM note_likes;" + Environment.NewLine +
                   "DELETE FROM process_logs;" + Environment.NewLine +
                   "DELETE FROM process_requests;" + Environment.NewLine +
                   "DELETE FROM reminders;" + Environment.NewLine +
                   "DELETE FROM report_shares;" + Environment.NewLine +
                   "DELETE FROM template_shares;" + Environment.NewLine +
                   "DELETE FROM user_custom_shares;" + Environment.NewLine +
                   "DELETE FROM users_user_groups;" + Environment.NewLine +
                   "DELETE FROM user_groups;" + Environment.NewLine +
                   "DELETE FROM view_shares;" + Environment.NewLine +
                   "DELETE FROM view_states;" + Environment.NewLine +
                   "DELETE FROM workflow_logs;" + Environment.NewLine;
        }

        /*public static bool CleanUpTables(string connectionString, string dbName, string scriptPath, JArray tableNames = null)
        {
            try
            {
                var connection = new PgSqlConnection(GetConnectionString(connectionString, dbName));
                connection.Open();

                if (tableNames == null)
                    tableNames = GetAllDynamicTables(connectionString, dbName);

                foreach (var table in tableNames)
                {
                    var sql = $"DELETE FROM {table["table_name"]};";
                    var dropCommand = new PgSqlCommand(sql) {Connection = connection};
                    dropCommand.ExecuteReader();

                    if (!string.IsNullOrEmpty(scriptPath))
                        File.AppendAllText(scriptPath, sql + Environment.NewLine);
                }

                connection.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }*/

        /*public static bool CleanUpSystemTables(string connectionString, Location location, string dbName, string scriptPath)
        {
            try
            {
                var sqls = CleanUpSystemTablesSql().Split(";");

                var connection = new PgSqlConnection(GetConnectionString(connectionString, dbName));
                connection.Open();

                foreach (var sql in sqls)
                {
                    if (string.IsNullOrEmpty(sql))
                        continue;

                    var dropCommand = new PgSqlCommand(sql) {Connection = connection};
                    var result = dropCommand.ExecuteReader();

                    if (!string.IsNullOrEmpty(scriptPath))
                        File.AppendAllText(scriptPath, sql + ";" + Environment.NewLine);
                }

                connection.Close();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }*/

        public static string GetAllSystemTablesSql()
        {
            return "SELECT table_name FROM information_schema.tables WHERE table_type != 'VIEW' AND table_schema = 'public' AND table_name NOT LIKE '%_d'";
        }

        public static string GetAllDynamicTablesSql()
        {
            return "SELECT table_name FROM information_schema.tables WHERE table_type != 'VIEW' AND table_schema = 'public' AND table_name LIKE '%_d'";
        }

        public static JArray GetAllUsersFKSql(string connectionString, string dbName)
        {
            var sqls = new JArray();

            var dropCommand = PosgresHelper.Read(connectionString, dbName, GetAllSystemTablesSql(), "array");
            var tableData = dropCommand;

            if (!tableData.HasValues) return null;

            foreach (var t in tableData)
            {
                var table = t["table_name"];
                var columns = PosgresHelper.Read(connectionString, dbName, GetUserFKColumnsSql(table.ToString()), "array");

                if (!columns.HasValues) continue;

                dropCommand = PosgresHelper.Read(connectionString, dbName, ColumnIsExistsSql(table.ToString(), "id"), "hasRows");
                var idColumnIsExists = dropCommand;
                var idExists = true;

                var fkColumns = (JArray)columns.DeepClone();

                if (!idColumnIsExists)
                {
                    columns = PosgresHelper.Read(connectionString, dbName, GetAllColumnNamesSql(table.ToString()), "array");

                    idExists = false;
                }
                else
                {
                    columns.Add(new JObject {["column_name"] = "id"});
                }

                var records = PosgresHelper.Read(connectionString, dbName, GetAllRecordsWithColumnsSql(table.ToString(), columns), "array");

                foreach (var record in records)
                {
                    var sql = UpdateRecordSql(record, table.ToString(), fkColumns, 1, idExists);
                    sqls.Add(sql);
                }
            }

            return sqls;
        }

        /*public static bool SetAllUserFK(string connectionString, Location location, string dbName, string scriptPath = null)
        {
            try
            {
                var sqls = ReleaseHelper.GetAllUsersFKSql(connectionString, dbName);
                var connection = new PgSqlConnection(GetConnectionString(connectionString, dbName));
                connection.Open();

                foreach (var sql in sqls)
                {
                    var dropCommand = new PgSqlCommand(sql.ToString()) {Connection = connection};
                    var result = dropCommand.ExecuteReader().ResultToJArray();

                    if (!string.IsNullOrEmpty(scriptPath))
                        File.AppendAllText(scriptPath, sql + Environment.NewLine);
                }

                connection.Close();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }*/

        /*public static JArray GetAllDynamicTables(string connectionString, string dbName)
        {
            try
            {
                var connection = new PgSqlConnection(GetConnectionString(connectionString, dbName));
                connection.Open();

                var dropCommand = new PgSqlCommand(GetAllDynamicTablesSql()) {Connection = connection};
                var result = dropCommand.ExecuteReader().ResultToJArray();

                connection.Close();
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }*/

        public static string GetUserFKColumnsSql(string tableName)
        {
            return "SELECT " +
                   "kcu.column_name " +
                   "FROM " +
                   "information_schema.table_constraints AS tc " +
                   "JOIN information_schema.key_column_usage AS kcu " +
                   "ON tc.constraint_name = kcu.constraint_name " +
                   "AND tc.table_schema = kcu.table_schema " +
                   "JOIN information_schema.constraint_column_usage AS ccu " +
                   "ON ccu.constraint_name = tc.constraint_name " +
                   "AND ccu.table_schema = tc.table_schema " +
                   "WHERE tc.constraint_type = 'FOREIGN KEY' AND tc.table_name='" + tableName + "' AND ccu.table_name = 'users';";
        }

        public static string GetAllRecordsWithColumnsSql(string tableName, JArray columns)
        {
            var sql = new StringBuilder("SELECT ");

            foreach (var column in columns)
            {
                var columnName = column["column_name"];
                sql.Append(columnName + ",");
            }

            sql = sql.Remove(sql.Length - 1, 1);
            sql.Append(" FROM " + tableName);

            return sql.ToString();
        }

        public static string UpdateRecordSql(JToken record, string tableName, JArray columns, int value, bool idExists)
        {
            var sql = new StringBuilder("UPDATE " + tableName + " SET ");

            foreach (var column in columns)
            {
                var columnName = column["column_name"];

                if (string.IsNullOrEmpty(record[columnName.ToString()].ToString()))
                    continue;

                sql.Append(columnName + " = " + value + ",");
            }

            sql = sql.Remove(sql.Length - 1, 1);

            if (idExists)
            {
                sql.Append(" WHERE id = " + (int)record["id"] + ";");
            }
            else
            {
                if (columns.Count > 0)
                {
                    sql.Append("WHERE ");
                    foreach (var column in columns)
                    {
                        var columnName = column["column_name"];

                        if (!string.IsNullOrEmpty(record[columnName.ToString()].ToString()))
                        {
                            sql.Append(columnName + " = " + record[columnName] + " ,");
                        }
                    }

                    sql = sql.Remove(sql.Length - 1, 1);
                }
            }

            return sql.ToString();
        }

        public static string GetAllColumnNamesSql(string tableName)
        {
            return "SELECT column_name " +
                   "FROM information_schema.columns " +
                   "WHERE table_schema = 'public' " +
                   "AND table_name   = '" + tableName + "';";
        }

        public static string ColumnIsExistsSql(string tableName, string columnName)
        {
            return "SELECT column_name " +
                   "FROM information_schema.columns " +
                   "WHERE table_name='" + tableName + "' and column_name='" + columnName + "';";
        }

        /*public static object GetScriptResult(string connectionString, string dbName, string script)
        {
            try
            {
                var connection = new PgSqlConnection(GetConnectionString(connectionString, dbName));
                connection.Open();

                var dropCommand = new PgSqlCommand(script) {Connection = connection};
                return dropCommand.ExecuteReader();
            }
            catch (Exception ex)
            {
                return null;
            }
        }*/


        /*public static bool RunRuntimeScript(string connectionString, string dbName, string script)
        {
            try
            {
                var connection = new PgSqlConnection(GetConnectionString(connectionString, dbName));
                connection.Open();

                var dropCommand = new PgSqlCommand(script) {Connection = connection};
                dropCommand.ExecuteReader();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }*/

        public static bool CreateDatabaseIfNotExists(string connectionString, string dbName)
        {
            try
            {
                PosgresHelper.Run(connectionString, "postgres", $"DROP DATABASE IF EXISTS {dbName};");
                PosgresHelper.Run(connectionString, "postgres", $"CREATE DATABASE {dbName};");

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool CopyDatabase(string connectionString, string dbName, string scriptPath = null)
        {
            try
            {
                var sql = $"CREATE DATABASE {dbName}_copy WITH TEMPLATE {dbName} OWNER primeapps;";

                if (!string.IsNullOrEmpty(scriptPath))
                    File.AppendAllText(scriptPath, sql + Environment.NewLine);

                return PosgresHelper.Run(connectionString, "postgres", sql);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool ChangeTemplateDatabaseStatus(string connectionString, string dbName, bool open = false)
        {
            try
            {
                JArray sqls;

                if (!open)
                {
                    sqls = new JArray()
                    {
                        $"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{dbName}' AND pid <> pg_backend_pid();",
                        $"UPDATE pg_database SET datistemplate = TRUE WHERE datname = '{dbName}';",
                        $"UPDATE pg_database SET datallowconn = FALSE WHERE datname = '{dbName}';"
                    };
                }
                else
                {
                    sqls = new JArray()
                    {
                        $"UPDATE pg_database SET datistemplate = FALSE WHERE datname = '{dbName}';",
                        $"UPDATE pg_database SET datallowconn = TRUE WHERE datname = '{dbName}';"
                    };
                }

                foreach (var sql in sqls)
                {
                    PosgresHelper.Run(connectionString, "postgres", sql.ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /*public static string GetSqlDump(string connectionString, string database, string scriptPath = null)
        {
            try
            {
                var connection = new PgSqlConnection(GetConnectionString(connectionString, database));

                connection.Open();
                var dump = new PgSqlDump {Connection = connection, Schema = "public", IncludeDrop = false};
                dump.Backup();
                connection.Close();

                if (!string.IsNullOrEmpty(scriptPath))
                    File.AppendAllText(scriptPath, dump.DumpText + Environment.NewLine);

                return dump.DumpText;
            }
            catch (Exception ex)
            {
                return null;
            }
        }*/

        /*public static bool RestoreDatabase(string connectionString, string dumpSql, string database)
        {
            try
            {
                var result = CreateDatabaseIfNotExists(connectionString, database);
                if (!result)
                    return false;

               PosgresHelper.Restore(connectionString, database, );

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }*/

        public static string CreateSettingsAppNameSql(string name)
        {
            return $"INSERT INTO \"public\".\"settings\"(\"type\", \"user_id\", \"key\", \"value\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\") VALUES (1, 1, 'app', '" + name + "', 1, NULL, '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture) + "', NULL, 'f');";
        }

        public static bool CreateSettingsAppName(string connectionString, string name, string dbName)
        {
            try
            {
                var sql = CreateSettingsAppNameSql(name);

                /*if (run)
                {
                    var connection = new PgSqlConnection(GetConnectionString(connectionString, dbName));
                    connection.Open();

                    var dropCommand = new PgSqlCommand(sql) {Connection = connection};
                    dropCommand.ExecuteReader();
                    connection.Close();
                }*/

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static JArray CreatePlatformAppSql(JObject app, string secret, string appUrl, string authUrl)
        {
            var options = JObject.Parse(app["setting"]["options"].ToString());
            var sqls = new JArray
            {
                $"INSERT INTO \"public\".\"apps\"(\"id\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"name\", \"label\", \"description\", \"logo\", \"use_tenant_settings\", \"app_draft_id\", \"secret\") VALUES (" + app["id"] + ", 1, NULL, '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture) + "', NULL, 'f', '" + app["name"] + "', '" + app["label"] + "', '" + app["description"] + "', '" + app["logo"] + "', '" + app["use_tenant_settings"] + "', 0, '" + secret + "');",
                $"INSERT INTO \"public\".\"app_settings\"(\"app_id\", \"app_domain\", \"auth_domain\", \"currency\", \"culture\", \"time_zone\", \"language\", \"auth_theme\", \"app_theme\", \"mail_sender_name\", \"mail_sender_email\", \"google_analytics_code\", \"tenant_operation_webhook\", \"registration_type\", \"enable_registration\") VALUES (" + app["id"] + ", '" + appUrl + "', '" + authUrl + "', " + (!string.IsNullOrEmpty(app["setting"]["currency"].ToString()) ? "'" + app["setting"]["currency"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["culture"].ToString()) ? "'" + app["setting"]["culture"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["time_zone"].ToString()) ? "'" + app["setting"]["time_zone"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["language"].ToString()) ? "'" + app["setting"]["language"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["auth_theme"].ToString()) ? "'" + app["setting"]["auth_theme"].ToJsonString() + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["app_theme"].ToString()) ? "'" + app["setting"]["app_theme"].ToJsonString() + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["mail_sender_name"].ToString()) ? "'" + app["setting"]["mail_sender_name"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["mail_sender_email"].ToString()) ? "'" + app["setting"]["mail_sender_email"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["google_analytics_code"].ToString()) ? "'" + app["setting"]["google_analytics_code"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["tenant_operation_webhook"].ToString()) ? "'" + app["setting"]["tenant_operation_webhook"] + "'" : "NULL") + ", 2, '" + options["enable_registration"].ToString().Substring(0, 1).ToLower() + "');"
            };

            //app["templates"].Aggregate(sql, (current, template) => current + ($"INSERT INTO \"public\".\"app_templates\"(\"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"app_id\", \"name\", \"subject\", \"content\", \"language\", \"type\", \"system_code\", \"active\", \"settings\") VALUES (1, NULL, '2018-10-01 11:41:02.829818', NULL, '" + template["deleted"] + "', " + app["id"] + ", '" + template["name"] + "', '" + template["subject"] + "', '" + template["content"] + "', '" + template["language"] + "', " + template["type"] + ", '" + template["system_code"] + "', '" + template["active"] + "', '" + template["settings"] + "');" + Environment.NewLine));
            return sqls;
        }

        public static JArray UpdatePlatformAppSql(JObject app, string appUrl, string authUrl)
        {
            var options = JObject.Parse(app["setting"]["options"].ToString());
            var sqls = new JArray
            {
                $"UPDATE public.apps SET created_by = 1, updated_by = 1, created_at = '2019-05-24 07:15:15.699988', updated_at = '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture) + "', label = '" + app["label"] + "', description = '" + app["description"] + "', logo = '" + app["logo"] + "', use_tenant_settings = '" + app["use_tenant_settings"] + "', app_draft_id = 0 WHERE id = " + app["id"] + ";",
                $"UPDATE public.app_settings SET " + (!string.IsNullOrEmpty(appUrl) ? $"app_domain = '{appUrl}'" : "") + (!string.IsNullOrEmpty(authUrl) ? $"auth_domain = '{authUrl}'" : "") + " currency = " + (!string.IsNullOrEmpty(app["setting"]["currency"].ToString()) ? "'" + app["setting"]["currency"] + "'" : "NULL") + ", culture = " + (!string.IsNullOrEmpty(app["setting"]["culture"].ToString()) ? "'" + app["setting"]["culture"] + "'" : "NULL") + ", time_zone = " + (!string.IsNullOrEmpty(app["setting"]["time_zone"].ToString()) ? "'" + app["setting"]["time_zone"] + "'" : "NULL") + ", language = " + (!string.IsNullOrEmpty(app["setting"]["language"].ToString()) ? "'" + app["setting"]["language"] + "'" : "NULL") + ", auth_theme = " + (!string.IsNullOrEmpty(app["setting"]["auth_theme"].ToString()) ? "'" + app["setting"]["auth_theme"].ToJsonString() + "'" : "NULL") + ", app_theme = " + (!string.IsNullOrEmpty(app["setting"]["app_theme"].ToString()) ? "'" + app["setting"]["app_theme"].ToJsonString() + "'" : "NULL") + ", mail_sender_name = " + (!string.IsNullOrEmpty(app["setting"]["mail_sender_name"].ToString()) ? "'" + app["setting"]["mail_sender_name"] + "'" : "NULL") + ", mail_sender_email = " + (!string.IsNullOrEmpty(app["setting"]["mail_sender_email"].ToString()) ? "'" + app["setting"]["mail_sender_email"] + "'" : "NULL") + ", google_analytics_code = " + (!string.IsNullOrEmpty(app["setting"]["google_analytics_code"].ToString()) ? "'" + app["setting"]["google_analytics_code"] + "'" : "NULL") + ", tenant_operation_webhook = " + (!string.IsNullOrEmpty(app["setting"]["tenant_operation_webhook"].ToString()) ? "'" + app["setting"]["tenant_operation_webhook"] + "'" : "NULL") + ", registration_type = 2, enable_registration = '" + options["enable_registration"].ToString().Substring(0, 1).ToLower() + "' WHERE app_id = " + app["id"] + ";"
            };

            //app["templates"].Aggregate(sql, (current, template) => current + ($"INSERT INTO \"public\".\"app_templates\"(\"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"app_id\", \"name\", \"subject\", \"content\", \"language\", \"type\", \"system_code\", \"active\", \"settings\") VALUES (1, NULL, '2018-10-01 11:41:02.829818', NULL, '" + template["deleted"] + "', " + app["id"] + ", '" + template["name"] + "', '" + template["subject"] + "', '" + template["content"] + "', '" + template["language"] + "', " + template["type"] + ", '" + template["system_code"] + "', '" + template["active"] + "', '" + template["settings"] + "');" + Environment.NewLine));
            return sqls;
        }

        public static bool CreatePlatformApp(string connectionString, JObject app, string secret, string appUrl, string authUrl)
        {
            try
            {
                var sqls = CreatePlatformAppSql(app, secret, appUrl, authUrl);

                foreach (var sql in sqls)
                {
                    PosgresHelper.Run(connectionString, "platform", sql.ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool UpdatePlatformApp(string connectionString, JObject app, string appUrl, string authUrl)
        {
            try
            {
                var sqls = UpdatePlatformAppSql(app, appUrl, authUrl);

                foreach (var sql in sqls)
                {
                    PosgresHelper.Run(connectionString, "platform", sql.ToString());
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool SwapCopyDatabase(string connectionString, string dbName)
        {
            try
            {
                var result = RemoveTemplateDatabase(connectionString, dbName);

                if (result)
                {
                    try
                    {
                        PosgresHelper.Run(connectionString, "postgres", $"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{dbName}_copy' AND pid <> pg_backend_pid();");
                        PosgresHelper.Run(connectionString, "postgres", $"ALTER DATABASE {dbName}_copy RENAME TO {dbName};");
                        PosgresHelper.Run(connectionString, "postgres", $"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{dbName}' AND pid <> pg_backend_pid();");
                        ChangeTemplateDatabaseStatus(connectionString, dbName, true);
                    }
                    catch (Exception e)
                    {
                        return false;
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool RemoveTemplateDatabase(string connectionString, string dbName)
        {
            try
            {
                ChangeTemplateDatabaseStatus(connectionString, dbName, true);

                PosgresHelper.Run(connectionString, "postgres", $"DROP DATABASE {dbName};");


                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void AddScript(string path, string sql)
        {
            if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(sql))
                File.AppendAllText(path, sql + Environment.NewLine);
        }

        public static string GetTableName(string query)
        {
            Regex nameExtractor = new Regex("((?<=INSERT\\sINTO\\s)|(?<=UPDATE\\s)|(?<=DELETE\\sFROM\\s))([^\\s]+)");

            Match match = nameExtractor.Match(query);

            return match.Value;
        }
    }
}