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
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace PrimeApps.Model.Helpers
{
    public enum Location
    {
        [EnumMember(Value = "PDE")] PDE = 1,

        [EnumMember(Value = "PRE")] PRE = 2
    }

    public class PublishHelper
    {
        public static async Task<bool> Create(JObject app, string studioSecret, bool clearAllRecords, string dbName, int version, int deploymentId, IConfiguration configuration)
        {
            var PDEConnectionString = configuration.GetConnectionString("StudioDBConnection");
            var PREConnectionString = configuration.GetConnectionString("PlatformDBConnection");
            var path = configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);

            path = $"{path}\\published\\logs\\{dbName}";

            if (!File.Exists(path))
                Directory.CreateDirectory(path);

            path = $"{path}\\{version}.txt";

            try
            {
                File.AppendAllText(path, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : " + "\u001b[92m" + "Publish starting..." + "\u001b[39m" + Environment.NewLine);
                File.AppendAllText(path, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Database sql generating..." + Environment.NewLine);

                var sqlDump = PublishHelper.GetSqlDump(PDEConnectionString, Location.PDE, dbName);

                if (string.IsNullOrEmpty(sqlDump))
                    File.AppendAllText(path, "\u001b[31m" + DateTime.Now + " : Unhandle exception. While creating sql dump script." + "\u001b[39m" + Environment.NewLine);

                File.AppendAllText(path, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Restoring your database..." + Environment.NewLine);
                var restoreResult = PublishHelper.RestoreDatabase(PREConnectionString, sqlDump, Location.PRE, dbName);

                if (!restoreResult)
                    File.AppendAllText(path, "\u001b[31m" + DateTime.Now + " : Unhandle exception. While restoring your database." + "\u001b[39m" + Environment.NewLine);

                File.AppendAllText(path, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Settings information adding..." + Environment.NewLine);
                PublishHelper.CreateSettingsAppName(PREConnectionString, app["name"].ToString(), dbName);

                File.AppendAllText(path, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : System tables clearing..." + Environment.NewLine);
                PublishHelper.CleanUpSystemTables(PREConnectionString, Location.PRE, dbName);

                File.AppendAllText(path, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Dynamic tables clearing..." + Environment.NewLine);
                PublishHelper.CleanUpTables(PREConnectionString, Location.PRE, dbName, clearAllRecords ? null : new JArray());

                File.AppendAllText(path, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Records are marking as sample..." + Environment.NewLine);
                PublishHelper.SetRecordsIsSample(PREConnectionString, Location.PRE, dbName);

                File.AppendAllText(path, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Editing your records..." + Environment.NewLine);
                PublishHelper.SetAllUserFK(PREConnectionString, Location.PRE, dbName);

                File.AppendAllText(path, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Database marking as template..." + Environment.NewLine);
                PublishHelper.SetDatabaseIsTemplate(PREConnectionString, Location.PRE, dbName);

                File.AppendAllText(path, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Final arrangements being made..." + Environment.NewLine);
                var secret = Guid.NewGuid().ToString().Replace("-", string.Empty);
                var secretEncrypt = CryptoHelper.Encrypt(secret);
                PublishHelper.CreatePlatformApp(PREConnectionString, app, secretEncrypt);

                using (var httpClient = new HttpClient())
                {
                    var dict = new Dictionary<string, string>
                    {
                        {"grant_type", "password"},
                        {"username", configuration.GetValue("AppSettings:IntegrationEmail", string.Empty)},
                        {"password", studioSecret},
                        {"client_id", configuration.GetValue("AppSettings:ClientId", string.Empty)},
                        {"client_secret", studioSecret}
                    };

                    var authUrl = configuration.GetValue("AppSettings:AuthenticationServerURL", string.Empty);

                    var req = new HttpRequestMessage(HttpMethod.Post, authUrl + "/connect/token") { Content = new FormUrlEncodedContent(dict) };
                    var res = await httpClient.SendAsync(req);

                    if (res.IsSuccessStatusCode)
                    {
                        var resp = await res.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(resp))
                        {
                            var obj = JObject.Parse(resp);
                            var token = obj["access_token"].ToString();

                            using (var client = new HttpClient())
                            {
                                var clientRequest = new JObject
                                {
                                    ["client_id"] = app["name"].ToString(),
                                    ["client_name"] = app["label"].ToString(),
                                    ["allowed_grant_types"] = "password",
                                    ["allow_remember_consent"] = false,
                                    ["always_send_client_claims"] = true,
                                    ["require_consent"] = false,
                                    ["client_secrets"] = secret,
                                    ["redirect_uris"] = "http://localhost:5010/signin-oidc",
                                    ["post_logout_redirect_uris"] = "http://localhost:5010/signout-callback-oidc",
                                    ["allowed_scopes"] = "openid;profile;email;api1",
                                    ["access_token_life_time"] = 864000
                                };

                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                                var response = await client.PostAsync(authUrl + "/api/client/create", new StringContent(JsonConvert.SerializeObject(clientRequest), Encoding.UTF8, "application/json"));

                                if (!response.IsSuccessStatusCode)
                                {
                                    resp = await response.Content.ReadAsStringAsync();
                                }
                            }

                            using (var client = new HttpClient())
                            {
                                var clientRequest = new JObject
                                {
                                    ["urls"] = "http://localhost:5010",
                                };

                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                                var response = await client.PostAsync(authUrl + "/api/add_url", new StringContent(JsonConvert.SerializeObject(clientRequest), Encoding.UTF8, "application/json"));

                                if (!response.IsSuccessStatusCode)
                                {
                                    resp = await response.Content.ReadAsStringAsync();
                                }
                            }

                        }
                    }
                }

                File.AppendAllText(path, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : " + "\u001b[92m" + "Done..." + "\u001b[39m" + Environment.NewLine);

                return true;
            }
            catch (Exception e)
            {
                File.AppendAllText(path, DateTime.Now + " : " + e.Message + Environment.NewLine);

                //tw.WriteLine(e.Message);
                return false;
            }
        }

        public static string GetConnectionString(string connectionString, Location location, string database)
        {
            var connection = new NpgsqlConnectionStringBuilder(connectionString);

            if (location == Location.PDE)
                return $"host={connection.Host};port={connection.Port};user id={connection.Username};password={connection.Password};database={database};";

            return $"host={connection.Host};port={connection.Port};user id={connection.Username};password={connection.Password};database={database};";
        }

        public static string SetRecordsIsSampleSql()
        {
            return "SELECT 'update \"' || cl.\"table_name\" || '\" set is_sample=true;' " +
                   "FROM information_schema.\"columns\" cl " +
                   "WHERE cl.\"column_name\" = 'is_sample' " +
                   "ORDER BY cl.\"table_name\" ";
        }

        public static bool SetRecordsIsSample(string connectionString, Location location, string dbName)
        {
            try
            {
                var connection = new PgSqlConnection(GetConnectionString(connectionString, location, dbName));
                connection.Open();

                var dropCommand = new PgSqlCommand(SetRecordsIsSampleSql()) { Connection = connection };
                var result = dropCommand.ExecuteReader();

                connection.Close();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static string CleanUpSystemTablesSql()
        {
            return "DELETE FROM analytic_shares; " +
                   "DELETE FROM audit_logs; " +
                   "DELETE FROM changelogs; " +
                   "DELETE FROM imports; " +
                   "DELETE FROM note_likes; " +
                   "DELETE FROM process_logs; " +
                   "DELETE FROM process_requests; " +
                   "DELETE FROM reminders; " +
                   "DELETE FROM report_shares; " +
                   "DELETE FROM template_shares; " +
                   "DELETE FROM user_custom_shares; " +
                   "DELETE FROM users_user_groups; " +
                   "DELETE FROM user_groups; " +
                   "DELETE FROM view_shares; " +
                   "DELETE FROM view_states; " +
                   "DELETE FROM workflow_logs; ";
        }

        public static bool CleanUpTables(string connectionString, Location location, string dbName, JArray tableNames = null)
        {
            try
            {
                var connection = new PgSqlConnection(GetConnectionString(connectionString, location, dbName));
                connection.Open();

                if (tableNames == null)
                    tableNames = GetAllDynamicTables(connectionString, location, dbName);

                foreach (var table in tableNames)
                {
                    var sql = $"DELETE FROM {table["table_name"]};";
                    var dropCommand = new PgSqlCommand(sql) { Connection = connection };
                    dropCommand.ExecuteReader();
                }

                connection.Close();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool CleanUpSystemTables(string connectionString, Location location, string dbName)
        {
            try
            {
                var connection = new PgSqlConnection(GetConnectionString(connectionString, location, dbName));
                connection.Open();

                var sqls = CleanUpSystemTablesSql().Split("; ");
                foreach (var sql in sqls)
                {
                    if (string.IsNullOrEmpty(sql))
                        continue;

                    var dropCommand = new PgSqlCommand(sql) { Connection = connection };
                    var result = dropCommand.ExecuteReader();
                }

                connection.Close();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static string GetAllSystemTablesSql()
        {
            return "SELECT table_name FROM information_schema.tables WHERE table_type != 'VIEW' AND table_schema = 'public' AND table_name NOT LIKE '%_d'";
        }

        public static string GetAllDynamicTablesSql()
        {
            return "SELECT table_name FROM information_schema.tables WHERE table_type != 'VIEW' AND table_schema = 'public' AND table_name LIKE '%_d'";
        }

        public static bool SetAllUserFK(string connectionString, Location location, string dbName)
        {
            try
            {
                var connection = new PgSqlConnection(GetConnectionString(connectionString, location, dbName));
                connection.Open();

                var dropCommand = new PgSqlCommand(GetAllSystemTablesSql()) { Connection = connection };
                var tableData = dropCommand.ExecuteReader().ResultToJArray();

                if (!tableData.HasValues) return false;

                foreach (var t in tableData)
                {
                    var table = t["table_name"];
                    dropCommand = new PgSqlCommand(GetUserFKColumnsSql(table.ToString())) { Connection = connection };
                    var columns = dropCommand.ExecuteReader().ResultToJArray();

                    if (!columns.HasValues) continue;

                    dropCommand = new PgSqlCommand(ColumnIsExistsSql(table.ToString(), "id")) { Connection = connection };
                    var idColumnIsExists = dropCommand.ExecuteReader();
                    var idExists = true;

                    var fkColumns = (JArray)columns.DeepClone();

                    if (!idColumnIsExists.HasRows)
                    {
                        dropCommand = new PgSqlCommand(GetAllColumnNamesSql(table.ToString())) { Connection = connection };
                        columns = dropCommand.ExecuteReader().ResultToJArray();

                        idExists = false;
                    }
                    else
                    {
                        columns.Add(new JObject { ["column_name"] = "id" });
                    }

                    dropCommand = new PgSqlCommand(GetAllRecordsWithColumnsSql(table.ToString(), columns)) { Connection = connection };
                    var records = dropCommand.ExecuteReader().ResultToJArray();

                    foreach (var record in records)
                    {
                        dropCommand = new PgSqlCommand(UpdateRecordSql(record, table.ToString(), fkColumns, 1, idExists)) { Connection = connection };
                        var result = dropCommand.ExecuteReader().ResultToJArray();
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public static JArray GetAllDynamicTables(string connectionString, Location location, string dbName)
        {
            try
            {
                var connection = new PgSqlConnection(GetConnectionString(connectionString, location, dbName));
                connection.Open();

                var dropCommand = new PgSqlCommand(GetAllDynamicTablesSql()) { Connection = connection };
                var result = dropCommand.ExecuteReader().ResultToJArray();

                connection.Close();
                return result;
            }
            catch (Exception e)
            {
                return null;
            }
        }

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

        public static bool CreateDatabaseIfNotExists(string connectionString, Location location, string dbName)
        {
            try
            {
                var connection = new PgSqlConnection(GetConnectionString(connectionString, location, "postgres"));
                connection.Open();

                var dropCommand = new PgSqlCommand($"DROP DATABASE IF EXISTS {dbName};");
                dropCommand.Connection = connection;
                dropCommand.ExecuteReader();

                var createCommand = new PgSqlCommand($"CREATE DATABASE {dbName};");
                createCommand.Connection = connection;
                createCommand.ExecuteReader();
                connection.Close();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static bool SetDatabaseIsTemplate(string connectionString, Location location, string dbName)
        {
            try
            {
                var connection = new PgSqlConnection(GetConnectionString(connectionString, location, "postgres"));
                connection.Open();

                var terminateCommand = new PgSqlCommand($"SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '{dbName}' AND pid <> pg_backend_pid();") { Connection = connection };
                terminateCommand.ExecuteReader();

                var setTemplateCommand = new PgSqlCommand($"UPDATE pg_database SET datistemplate = TRUE WHERE datname = '{dbName}';") { Connection = connection };
                setTemplateCommand.ExecuteReader();

                var notAllowConnectionCommand = new PgSqlCommand($"UPDATE pg_database SET datallowconn = FALSE WHERE datname = '{dbName}';") { Connection = connection };
                notAllowConnectionCommand.ExecuteReader();
                connection.Close();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static string GetSqlDump(string connectionString, Location location, string database)
        {
            try
            {
                var connection = new PgSqlConnection(GetConnectionString(connectionString, location, database));

                connection.Open();
                var dump = new PgSqlDump { Connection = connection, Schema = "public", IncludeDrop = false };
                dump.Backup();
                connection.Close();

                return dump.DumpText;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static bool RestoreDatabase(string connectionString, string dumpSql, Location location, string database)
        {
            try
            {
                var result = CreateDatabaseIfNotExists(connectionString, location, database);
                if (!result)
                    return false;

                var connection = new PgSqlConnection(GetConnectionString(connectionString, location, database));

                connection.Open();
                var dump = new PgSqlDump { Connection = connection, DumpText = dumpSql };
                dump.Restore();
                connection.Close();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        public static string CreateSettingsAppNameSql(string name)
        {
            return $"INSERT INTO \"public\".\"settings\"(\"type\", \"user_id\", \"key\", \"value\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\") VALUES (1, 1, 'app', '" + name + "', 1, NULL, '" + DateTime.UtcNow + "', NULL, 'f')";
        }

        public static bool CreateSettingsAppName(string connectionString, string name, string dbName)
        {
            try
            {
                var connection = new PgSqlConnection(GetConnectionString(connectionString, Location.PRE, dbName));
                connection.Open();

                var sql = CreateSettingsAppNameSql(name);
                var dropCommand = new PgSqlCommand(sql) { Connection = connection };

                dropCommand.ExecuteReader();
                connection.Close();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static JArray CreatePlatformAppSql(JObject app, string secret)
        {
            var sqls = new JArray
            {
                $"INSERT INTO \"public\".\"apps\"(\"id\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"name\", \"label\", \"description\", \"logo\", \"use_tenant_settings\", \"app_draft_id\", \"secret\") VALUES (" + app["id"] + ", 1, NULL, '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture) + "', NULL, 'f', '" + app["name"] + "', '" + app["label"] + "', '" + app["description"] + "', '" + app["logo"] + "', '" + app["use_tenant_settings"] + "', 0, '" + secret + "');",
                $"INSERT INTO \"public\".\"app_settings\"(\"app_id\", \"app_domain\", \"auth_domain\", \"currency\", \"culture\", \"time_zone\", \"language\", \"auth_theme\", \"app_theme\", \"mail_sender_name\", \"mail_sender_email\", \"google_analytics_code\", \"tenant_operation_webhook\", \"registration_type\", \"enable_registration\") VALUES (" + app["id"] + ", " + (!string.IsNullOrEmpty(app["setting"]["app_domain"].ToString()) ? "'" + app["setting"]["app_domain"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["auth_domain"].ToString()) ? "'" + app["setting"]["auth_domain"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["currency"].ToString()) ? "'" + app["setting"]["currency"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["culture"].ToString()) ? "'" + app["setting"]["culture"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["time_zone"].ToString()) ? "'" + app["setting"]["time_zone"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["language"].ToString()) ? "'" + app["setting"]["language"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["auth_theme"].ToString()) ? "'" + app["setting"]["auth_theme"].ToJsonString() + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["app_theme"].ToString()) ? "'" + app["setting"]["app_theme"].ToJsonString() + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["mail_sender_name"].ToString()) ? "'" + app["setting"]["mail_sender_name"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["mail_sender_email"].ToString()) ? "'" + app["setting"]["mail_sender_email"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["google_analytics_code"].ToString()) ? "'" + app["setting"]["google_analytics_code"] + "'" : "NULL") + ", " + (!string.IsNullOrEmpty(app["setting"]["tenant_operation_webhook"].ToString()) ? "'" + app["setting"]["tenant_operation_webhook"] + "'" : "NULL") + ", 2, '" + app["setting"]["enable_registration"].ToString().Substring(0, 1).ToLower() + "');"
            };


            //app["templates"].Aggregate(sql, (current, template) => current + ($"INSERT INTO \"public\".\"app_templates\"(\"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\", \"app_id\", \"name\", \"subject\", \"content\", \"language\", \"type\", \"system_code\", \"active\", \"settings\") VALUES (1, NULL, '2018-10-01 11:41:02.829818', NULL, '" + template["deleted"] + "', " + app["id"] + ", '" + template["name"] + "', '" + template["subject"] + "', '" + template["content"] + "', '" + template["language"] + "', " + template["type"] + ", '" + template["system_code"] + "', '" + template["active"] + "', '" + template["settings"] + "');" + Environment.NewLine));
            return sqls;
        }

        public static bool CreatePlatformApp(string connectionString, JObject app, string secret)
        {
            try
            {
                var connection = new PgSqlConnection(GetConnectionString(connectionString, Location.PRE, "platform"));
                connection.Open();

                var sqls = CreatePlatformAppSql(app, secret);

                foreach (var sql in sqls)
                {
                    var dropCommand = new PgSqlCommand(sql.ToString()) { Connection = connection };
                    dropCommand.ExecuteReader();
                }

                connection.Close();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}