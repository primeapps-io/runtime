using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Storage;

namespace PrimeApps.Model.Helpers
{
	public class PackageHelper
	{
		public static async Task<bool> All(JObject app, JObject packageModel, string dbName, string version, IConfiguration configuration, IUnifiedStorage storage, List<HistoryStorage> historyStorages, IHostingEnvironment hostingEnvironment, RecordRepository recordRepository, ModuleRepository moduleRepository)
		{
			var PDEConnectionString = configuration.GetConnectionString("StudioDBConnection");
			var postgresPath = PostgresHelper.GetPostgresBinaryPath(configuration, hostingEnvironment);
			var root = DataHelper.GetDataDirectoryPath(configuration, hostingEnvironment);

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
				File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Creating your database template..." + Environment.NewLine);

				//var sqlDump = ReleaseHelper.GetSqlDump(PDEConnectionString, dbName, $"{path}\\dumpSql.txt");

				var sqlDumpResult = PostgresHelper.Dump(PDEConnectionString, dbName, postgresPath, $"{path}/");

				if (!sqlDumpResult)
					File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Unhandle exception. While creating sql dump script." + "\u001b[39m" + Environment.NewLine);

				/*if (autoDistribute)
                {
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Restoring your database..." + Environment.NewLine);
                    var restoreResult = ReleaseHelper.RestoreDatabase(PREConnectionString, sqlDump, Location.PRE, dbName);

                    if (!restoreResult)
                        File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Unhandle exception. While restoring your database." + "\u001b[39m" + Environment.NewLine);
                }*/

				File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Adjusting template settings..." + Environment.NewLine);

				var result = PackageHelper.CreateSettingsAppNameSql(app["name"].ToString());

				AddScript(scriptPath, result);

				/*if (!result)
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while adding app setting values... \u001b[39m" + Environment.NewLine);
                */

				File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Tables are preparing..." + Environment.NewLine);
				result = PackageHelper.CleanUpSystemTablesSql();
				AddScript(scriptPath, result);

				/*if (!result)
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while clean-up system tables... \u001b[39m" + Environment.NewLine);
                */

				//var arrayResult = ReleaseHelper.GetAllDynamicTables(PDEConnectionString, dbName);

				var arrayResult = PostgresHelper.Read(PDEConnectionString, dbName, GetAllDynamicTablesSql(), "array");

				var protectModulesType = (PackageModulesType)Enum.Parse(typeof(PackageModulesType), packageModel["protectModules"].ToString(), true);

				if (protectModulesType == PackageModulesType.DontTransfer)
				{
					if (arrayResult != null)
					{
						foreach (var table in arrayResult)
						{
							var sql = $"TRUNCATE {table["table_name"]} CASCADE;";
							AddScript(scriptPath, sql);
						}
					}
				}
				else if (protectModulesType == PackageModulesType.SelectedModules)
				{
					/**selectedModules JArray, it has modules and modules' related lookups
					 * for exp: ["mName":[{name:"lookupName",lookup_type:"LookupModuleName"},{}]]
					 * **/
					var selectedModules = (JArray)packageModel["selectedModules"];
					/** it likes  selectedModules, but it has difrent modules and modules' related lookups
					 * That modules weren't chose
					 * All modules and related lookups = {"m1":[{}..],"m2":[{}]}
					 * selectedModules = ["m2":{}...], modulesRelations={"m1":[]...}
					 * modulesRelations = allModules - selectedModules;
					 * **/
					var modulesRelations = (JObject)packageModel["modulesRelations"];
					var moduleName = "";
					var childArray = new JArray();
					var updatedList = new List<string>();

					foreach (JObject selectedModule in selectedModules)
					{
						foreach (var property in selectedModule)
						{
							if (updatedList.IndexOf(property.Key) < 0)
								updatedList.Add(property.Key);

							moduleName = property.Key;
							childArray = (JArray)property.Value;
							await ControlChildArray(selectedModules, childArray, recordRepository, moduleRepository, moduleName, scriptPath, updatedList);
						}
					}

					foreach (var property in modulesRelations)
					{
						moduleName = property.Key;
						childArray = (JArray)property.Value;
						await ControlChildArray(selectedModules, childArray, recordRepository, moduleRepository, moduleName, scriptPath, updatedList, true);
					}

				}

				/*if (!result)
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while clearing dynamic tables... \u001b[39m" + Environment.NewLine);
                */

				//Set is sample removed.
				/*File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Records are marking as sample..." + Environment.NewLine);
                arrayResult = PostgresHelper.Read(PDEConnectionString, dbName, PackageHelper.SetRecordsIsSampleSql(), "array");

                if (arrayResult != null)
                {
                    foreach (var r in arrayResult)
                    {
                        AddScript(scriptPath, ((JObject)r)["?column?"].ToString());
                    }
                }*/

				//AddScript(scriptPath, $"INSERT INTO \"public\".\"dashboard\"(\"id\", \"name\", \"description\", \"user_id\", \"profile_id\", \"is_active\", \"sharing_type\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\") VALUES (1, 'Default Dashboard', NULL, NULL, NULL, 'f', 1, 1, NULL, '2018-02-18 23:14:26.377005', NULL, 'f');");

				/*if (!result)
                    File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : \u001b[93m Unhandle exception while marking records as sample... \u001b[39m" + Environment.NewLine);
                */

				File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Making final adjusments to template..." + Environment.NewLine);
				arrayResult = PackageHelper.GetAllUsersFkSql(PDEConnectionString, dbName);

				if (arrayResult != null)
				{
					foreach (var sql in arrayResult)
					{
						AddScript(scriptPath, sql.ToString());
					}
				}

				File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : Adding files to package..." + Environment.NewLine);
				if (historyStorages != null)
				{
					foreach (var (value, index) in historyStorages.Select((v, i) => (v, i)))
					{
						AddScript(storagePath, (index == 0 ? "[" : "") + new JObject { ["mime_type"] = value.MimeType, ["operation"] = value.Operation, ["file_name"] = value.FileName, ["unique_name"] = value.UniqueName, ["path"] = value.Path }.ToJsonString() + (index != historyStorages.Count() - 1 ? "," : "]"));

						var file = await storage.GetObject(value.Path, value.UniqueName);
						using (var fileStream = File.Create(Path.Combine(storageFilesPath, value.FileName)))
						{
							file.ResponseStream.CopyTo(fileStream);
						}
					}
				}

				// Delete all users without 1
				AddScript(scriptPath, $"DELETE FROM users WHERE id != 1;");

				try
				{
					ZipFile.CreateFromDirectory(path, path = Path.Combine(root, "packages", dbName, $"{dbName}.zip"));
				}
				catch (Exception e)
				{
					File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Unhandle exception. While zipping package folder. Error : " + e.Message + "\u001b[39m" + Environment.NewLine);
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
				File.AppendAllText(logPath, "\u001b[90m" + DateTime.Now + "\u001b[39m" + " : " + "\u001b[31m" + " Error : " + e.Message + "\u001b[39m" + Environment.NewLine);
				ErrorHandler.LogError(e, "PackageHelper All method error.");

				//tw.WriteLine(e.Message);
				return false;
			}
		}

		public static async Task<bool> Diffs(List<HistoryDatabase> historyDatabases, List<HistoryStorage> historyStorages, JObject app, string dbName, string version, int deploymentId, IConfiguration configuration, IUnifiedStorage storage, IHostingEnvironment hostingEnvironment)
		{
			var root = DataHelper.GetDataDirectoryPath(configuration, hostingEnvironment);

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
						AddScript(storagePath, (index == 0 ? "[" : "") + new JObject { ["mime_type"] = value.MimeType, ["operation"] = value.Operation, ["file_name"] = value.FileName, ["unique_name"] = value.UniqueName, ["path"] = value.Path }.ToJsonString() + (index != historyStorages.Count() - 1 ? "," : "]"));
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
					File.AppendAllText(logPath, "\u001b[31m" + DateTime.Now + " : Unhandle exception. While zipping package folder. Error : " + e.Message + "\u001b[39m" + Environment.NewLine);
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
				ErrorHandler.LogError(e, "PackageHelper Diffs method error.");

				return false;
			}
		}

		public static string SetRecordsIsSampleSql()
		{
			return "SELECT 'update \"' || cl.\"table_name\" || '\" set is_sample=true;' " +
				   "FROM information_schema.\"columns\" cl " +
				   "WHERE cl.\"column_name\" = 'is_sample' " +
				   "ORDER BY cl.\"table_name\";";
		}

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

		public static string GetAllTablesSql()
		{
			return $"SELECT table_name FROM information_schema.tables WHERE table_type != 'VIEW' AND table_schema = 'public'";
		}

		public static string GetAllDynamicTablesSql()
		{
			return $"SELECT table_name FROM information_schema.tables WHERE table_type != 'VIEW' AND table_schema = 'public' AND table_name LIKE '%\\_d'";
		}

		public static JArray GetAllUsersFkSql(string connectionString, string dbName)
		{
			var sqls = new JArray();

			var dropCommand = PostgresHelper.Read(connectionString, dbName, GetAllTablesSql(), "array");
			var tableData = dropCommand;

			if (!tableData.HasValues)
				return null;

			foreach (var t in tableData)
			{
				var table = t["table_name"];
				var columns = PostgresHelper.Read(connectionString, dbName, GetUserFKColumnsSql(table.ToString()), "array");

				if (!columns.HasValues)
					continue;

				if (((JArray)columns).Count == 1 && ((JArray)columns).First["column_name"].ToString() == "user_id")
					continue;

				sqls.Add(UpdateUserFkRecordSql(table.ToString(), columns));
				/*dropCommand = PostgresHelper.Read(connectionString, dbName, ColumnIsExistsSql(table.ToString(), "id"), "hasRows");
                var idColumnIsExists = dropCommand;
                var idExists = true;

                var fkColumns = (JArray)columns.DeepClone();

                if (!idColumnIsExists)
                {
                    columns = PostgresHelper.Read(connectionString, dbName, GetAllColumnNamesSql(table.ToString()), "array");

                    idExists = false;
                }
                else
                {
                    columns.Add(new JObject {["column_name"] = "id"});
                }

                var records = PostgresHelper.Read(connectionString, dbName, GetAllRecordsWithColumnsSql(table.ToString(), columns), "array");

                foreach (var record in records)
                {
                    var sql = UpdateRecordSql(record, table.ToString(), fkColumns, 1, idExists);
                    sqls.Add(sql);
                }*/
			}

			return sqls;
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

		public static string UpdateUserFkRecordSql(string tableName, JArray columns)
		{
			var sql = new StringBuilder($"UPDATE {tableName} SET ");

			foreach (var column in columns)
			{
				var columnName = column["column_name"];

				if (columnName.ToString() == "user_id")
					continue;

				sql.Append(columnName + " = 1,");
			}

			sql = sql.Remove(sql.Length - 1, 1);
			sql.Append(";");
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

		public static string CreateSettingsAppNameSql(string name)
		{
			return $"INSERT INTO \"public\".\"settings\"(\"type\", \"user_id\", \"key\", \"value\", \"created_by\", \"updated_by\", \"created_at\", \"updated_at\", \"deleted\") VALUES (1, 1, 'app', '" + name + "', 1, NULL, '" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture) + "', NULL, 'f');";
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

		public static JArray GetRecords(JArray modulesArray, RecordRepository recordRepository, string moduleName)
		{
			var findRequest = new FindRequest();
			findRequest.Fields = new List<string>();
			findRequest.Limit = 9999;
			findRequest.Fields.Add("is_sample");

			var lookupModules = (JArray)modulesArray.Where(q => q[moduleName] != null)?.FirstOrDefault()?[moduleName];
			if (lookupModules != null)
				foreach (var lookupModule in lookupModules)
				{
					var relatedFieldName = lookupModule["name"].ToString();
					var relatedModuleName = lookupModule["lookup_type"].ToString();
					findRequest.Fields.Add(relatedFieldName + "." + relatedModuleName + ".name.primary");
				}

			return recordRepository.Find(moduleName, findRequest);
		}

		public static async Task ControlChildArray(JArray selectedModules, JArray childArray, RecordRepository recordRepository, ModuleRepository moduleRepository, string moduleName, string scriptPath, List<string> updatedList, bool checkIsSample = false)
		{
			/**if childArray in selectedModules, checkIsSample = false 
			 * else checkIsSample = true**/
			if (childArray.Count < 1 && checkIsSample)
				await UpdateRecords(recordRepository, moduleRepository, selectedModules, moduleName, "", "", scriptPath, true);

			else
				for (int j = 0; j < childArray.Count; j++)
				{
					var relatedModuleField = childArray[j];
					var relatedFieldName = relatedModuleField["name"].ToString(); //It's lookup field name at the main module
					var relatedModuleName = relatedModuleField["lookup_type"].ToString(); // It's lookup module name

					await ControlLookupModules(selectedModules, recordRepository, moduleRepository, moduleName, relatedModuleName, relatedFieldName, scriptPath, updatedList, checkIsSample);
				}
		}

		public static async Task ControlLookupModules(JArray selectedModules, RecordRepository recordRepository, ModuleRepository moduleRepository, string moduleName, string relatedModuleName, string relatedFieldName, string scriptPath, List<string> updatedList, bool checkSampleData = false)
		{
			/** I have to control relatedModuleName in selected Modules
			 * if exist we have to control checkSampleData, if checkSampleData = true it comes from modulesRelations, we have to use update method.
			 * When I updated any module, I'm adding that to updatedList because if i'm not add it will try to follow the same steps and this way may cause it to loop forever(infinite loop)
			 * **/
			var isExistModule = (JArray)selectedModules.Where(child => child[relatedModuleName] != null)?.FirstOrDefault()?[relatedModuleName];

			if (isExistModule != null)
			{

				if (checkSampleData)
					await UpdateRecords(recordRepository, moduleRepository, selectedModules, moduleName, relatedModuleName, relatedFieldName, scriptPath, checkSampleData);

				else if (updatedList.IndexOf(relatedModuleName) < 0)
				{
					updatedList.Add(relatedModuleName);
					moduleName = relatedModuleName;
					var childArray = (JArray)selectedModules.Where(q => q[relatedModuleName] != null)?.FirstOrDefault()?[relatedModuleName];
					await ControlChildArray(selectedModules, childArray, recordRepository, moduleRepository, moduleName, scriptPath, updatedList, checkSampleData);
				}
			}
			else
				await UpdateRecords(recordRepository, moduleRepository, selectedModules, moduleName, relatedModuleName, relatedFieldName, scriptPath, checkSampleData);

		}

		public static async Task UpdateRecords(RecordRepository recordRepository, ModuleRepository moduleRepository, JArray selectedModules, string moduleName, string relatedModuleName, string fieldName, string scriptPath, bool checkSampleData = false)
		{
			var records = PackageHelper.GetRecords(selectedModules, recordRepository, moduleName);
			var isSampleDatas = checkSampleData ? records.Children().Where(q => (bool)q["is_sample"] == true) : records;
			var sql = "";
			//if  module has is_sample datas or  checkSampleData = false
			//else we have to generate TRUNCATE sql query
			if ((isSampleDatas != null && isSampleDatas?.Count() > 0) || !checkSampleData)
			{
				var mainModule = await moduleRepository.GetBasicByName(moduleName);
				string ids = "";
				var relatedModule = new Module();

				if (!string.IsNullOrEmpty(relatedModuleName))
					relatedModule = await moduleRepository.GetBasicByName(relatedModuleName);

				foreach (var record in isSampleDatas)
				{
					var updateRequest = new JObject();
					updateRequest["id"] = (int)record["id"];

					if (!string.IsNullOrEmpty(fieldName))
						updateRequest[fieldName] = null;


					if (relatedModule.Id > 0)
					{
						var childRecord = await recordRepository.GetById(relatedModule, (int)record[fieldName + "." + relatedModuleName + ".id"]);

						if (!(bool)childRecord["is_sample"])
							await recordRepository.Update(updateRequest, mainModule);
					}

					if (checkSampleData)
					{
						ids += (string)record["id"] + (record == records.Last() ? ", " : "");

						if (!string.IsNullOrEmpty(ids))
						{
							sql = $"DELETE FROM {moduleName}_d WHERE id NOT IN ({ids});";
							AddScript(scriptPath, sql);
						}
					}
				}
			}
			else
			{
				sql = $"TRUNCATE {moduleName}_d CASCADE;";
				AddScript(scriptPath, sql);
			}
		}
	}
}