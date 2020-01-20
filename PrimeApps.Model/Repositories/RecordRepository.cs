using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;
using Hangfire;
using Newtonsoft.Json.Linq;
using Npgsql;
using NpgsqlTypes;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Helpers.QueryTranslation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.Model.Repositories
{
    public class RecordRepository : RepositoryBaseTenant, IRecordRepository
    {
        private Warehouse _warehouse;

        public RecordRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }

        public RecordRepository(TenantDBContext dbContext, Warehouse warehouse, IConfiguration configuration) : base(dbContext, configuration)
        {
            _warehouse = warehouse;
        }

        public JObject GetById(Module module, int recordId, bool roleBasedEnabled = true, ICollection<Module> lookupModules = null, bool deleted = false)
        {
            string owners = null;
            string userGroups = null;

            if (roleBasedEnabled && module.Fields.Any(x => x.Name == "owner") && module.Name != "users" && module.Name != "profiles" && module.Name != "roles")
                GetRoleBasedInfo(module.Name, out owners, out userGroups);

            var sql = RecordHelper.GenerateGetSql(module, lookupModules, recordId, owners, CurrentUser.UserId, userGroups, deleted);
            var data = DbContext.Database.SqlQueryDynamic(sql).FirstOrDefault();

            var record = data == null ? new JObject() : (JObject)data;

            if (!record.IsNullOrEmpty())
            {
                if (!record["shared_users"].IsNullOrEmpty() || !record["shared_user_groups"].IsNullOrEmpty())
                {
                    var userIds = record["shared_users"].IsNullOrEmpty() ? "0" : string.Join(",", (JArray)record["shared_users"]);
                    var userGroupIds = record["shared_user_groups"].IsNullOrEmpty() ? "0" : string.Join(",", (JArray)record["shared_user_groups"]);
                    var sqlSharedRead = RecordHelper.GenerateSharedSql(userIds, userGroupIds);
                    record["shared_read"] = DbContext.Database.SqlQueryDynamic(sqlSharedRead);
                }

                if (!record["shared_users_edit"].IsNullOrEmpty() || !record["shared_user_groups_edit"].IsNullOrEmpty())
                {
                    var userIdsEdit = record["shared_users_edit"].IsNullOrEmpty() ? "0" : string.Join(",", (JArray)record["shared_users_edit"]);
                    var userGroupIdsEdit = record["shared_user_groups_edit"].IsNullOrEmpty() ? "0" : string.Join(",", (JArray)record["shared_user_groups_edit"]);
                    var sqlSharedEdit = RecordHelper.GenerateSharedSql(userIdsEdit, userGroupIdsEdit);
                    record["shared_edit"] = DbContext.Database.SqlQueryDynamic(sqlSharedEdit);
                }
            }

            return record;
        }

        public JArray Find(string moduleName, FindRequest findRequest, bool roleBasedEnabled = true, int timezoneOffset = 180)
        {
            string owners = null;
            string userGroups = null;

            if (roleBasedEnabled && moduleName != "users" && moduleName != "profiles" && moduleName != "roles")
                GetRoleBasedInfo(moduleName, out owners, out userGroups);

            var sql = RecordHelper.GenerateFindSql(moduleName, findRequest, owners, CurrentUser.UserId, userGroups, timezoneOffset: timezoneOffset);
            JArray records;
            try
            {
                records = DbContext.Database.SqlQueryDynamic(sql);
            }
            catch
            {
                //If table does not exist and relation type is many to many change table names and try again.
                if (!string.IsNullOrEmpty(findRequest.ManyToMany))
                {
                    findRequest.TwoWay = true;
                    sql = RecordHelper.GenerateFindSql(moduleName, findRequest, owners, CurrentUser.UserId, userGroups);
                    records = DbContext.Database.SqlQueryDynamic(sql);
                }
                else
                    throw;
            }

            if (records.Count > 0)
            {
                foreach (var record in records)
                {
                    if (!record.IsNullOrEmpty())
                    {
                        if (!record["shared_users"].IsNullOrEmpty() || !record["shared_user_groups"].IsNullOrEmpty())
                        {
                            var userIds = record["shared_users"].IsNullOrEmpty() ? "0" : string.Join(",", (JArray)record["shared_users"]);
                            var userGroupIds = record["shared_user_groups"].IsNullOrEmpty() ? "0" : string.Join(",", (JArray)record["shared_user_groups"]);
                            var sqlSharedRead = RecordHelper.GenerateSharedSql(userIds, userGroupIds);
                            record["shared_read"] = DbContext.Database.SqlQueryDynamic(sqlSharedRead);
                        }

                        if (!record["shared_users_edit"].IsNullOrEmpty() || !record["shared_user_groups_edit"].IsNullOrEmpty())
                        {
                            var userIdsEdit = record["shared_users_edit"].IsNullOrEmpty() ? "0" : string.Join(",", (JArray)record["shared_users_edit"]);
                            var userGroupIdsEdit = record["shared_user_groups_edit"].IsNullOrEmpty() ? "0" : string.Join(",", (JArray)record["shared_user_groups_edit"]);
                            var sqlSharedEdit = RecordHelper.GenerateSharedSql(userIdsEdit, userGroupIdsEdit);
                            record["shared_edit"] = DbContext.Database.SqlQueryDynamic(sqlSharedEdit);
                        }
                    }
                }
            }

            return records;
        }

        public JArray GetAllById(string moduleName, List<int> recordIds, bool roleBasedEnabled = true)
        {
            string owners = null;
            string userGroups = null;

            if (roleBasedEnabled && moduleName != "users")
                GetRoleBasedInfo(moduleName, out owners, out userGroups);

            var sql = RecordHelper.GenerateGetAllByIdSql(moduleName, recordIds, owners, CurrentUser.UserId, userGroups);
            var records = DbContext.Database.SqlQueryDynamic(sql);

            return records;
        }

        public async Task<int> Create(JObject record, Module module)
        {
            int result;

            using (var command = (NpgsqlCommand)DbContext.Database.GetDbConnection().CreateCommand())
            {
                var columns = new List<string>();
                var values = new List<string>();
                var currentUserId = DbContext.GetCurrentUserId();
                var now = DateTime.UtcNow;

                RecordHelper.AddCommandParameters(command, record, module);

                foreach (NpgsqlParameter parameter in command.Parameters)
                {
                    columns.Add("\"" + parameter.ParameterName + "\"");
                    values.Add("@" + parameter.ParameterName);
                }

                RecordHelper.AddCommandStandardParametersCreate(command, record, module, columns, values, currentUserId, now);

                var returnValue = new NpgsqlParameter { ParameterName = "return_value", NpgsqlDbType = NpgsqlDbType.Integer, Direction = ParameterDirection.Output };
                command.Parameters.Add(returnValue);

                var sql = $"INSERT INTO \"{module.Name}_d\" (\n\t{string.Join(",\n\t", columns)}\n) \nVALUES (\n\t{string.Join(",\n\t", values)}\n) \nRETURNING \"id\"";
                command.CommandText = sql;

                if (command.Connection.State != ConnectionState.Open)
                    command.Connection.Open();

                result = await command.ExecuteNonQueryAsync();

                if (result > 0)
                {
                    record["id"] = int.Parse(returnValue.NpgsqlValue.ToString());
                    record["created_by"] = currentUserId;
                    record["updated_by"] = currentUserId;
                    record["created_at"] = now;
                    record["updated_at"] = now;
                    record["deleted"] = false;

                    //// Create warehouse record
                    //if (string.IsNullOrWhiteSpace(_warehouse?.DatabaseName))
                    //    throw new Exception("Warehouse cannot be null during create/update/delete record.");

                    //if (_warehouse.DatabaseName != "0")
                    //    BackgroundJob.Enqueue(() => _warehouse.CreateRecord(record, _warehouse.DatabaseName, module.Name, CurrentUser));
                }
            }

            return result;
        }

        public async Task<int> Update(JObject record, Module module, bool delete = false, bool isUtc = true)
        {
            int result;

            using (var command = (NpgsqlCommand)DbContext.Database.GetDbConnection().CreateCommand())
            {
                var sets = new List<string>();
                var currentUserId = DbContext.GetCurrentUserId();
                var now = DateTime.UtcNow;
                var recordId = record["id"].IsNullOrEmpty() ? 0 : (int)record["id"];

                RecordHelper.AddCommandParameters(command, record, module, isUtc);

                foreach (NpgsqlParameter parameter in command.Parameters)
                {
                    sets.Add("\"" + parameter.ParameterName + "\" = @" + parameter.ParameterName);
                }

                RecordHelper.AddCommandStandardParametersUpdate(command, record, module, sets, currentUserId, now, delete);

                var sql = $"UPDATE \"{module.Name}_d\" \nSET \n\t{string.Join(",\n\t", sets)} \nWHERE \n\t\"id\" = {recordId}";
                command.CommandText = sql;

                if (command.Connection.State != ConnectionState.Open)
                    command.Connection.Open();

                result = await command.ExecuteNonQueryAsync();

                //if (result > 0)
                //{
                //    record["updated_by"] = currentUserId;
                //    record["updated_at"] = now;

                //    // Update warehouse record
                //    if (string.IsNullOrWhiteSpace(_warehouse?.DatabaseName))
                //        throw new Exception("Warehouse cannot be null during create/update/delete record.");

                //    if (_warehouse.DatabaseName != "0")
                //        BackgroundJob.Enqueue(() => _warehouse.UpdateRecord(record, _warehouse.DatabaseName, module.Name, CurrentUser, delete));
                //}
            }

            return result;
        }

        public async Task<int> Delete(JObject record, Module module)
        {
            var recordDelete = new JObject();
            recordDelete["id"] = record["id"];
            recordDelete["deleted"] = true;

            return await Update(recordDelete, module, true);
        }

        public async Task<int> AddRelations(JArray records, string moduleName, string relatedModuleName, int relationId = 0, bool twoway = false)
        {
            int result = 0;

            using (var command = (NpgsqlCommand)DbContext.Database.GetDbConnection().CreateCommand())
            {
                var columns = new List<string>();
                var values = new List<string>();
                var columnName1 = moduleName + "_id";
                var columnName2 = relatedModuleName + "_id";

                columns.Add($"\"{columnName1}\"");
                columns.Add($"\"{columnName2}\"");

                for (int i = 0; i < records.Count; i++)
                {
                    var record = records[i];
                    command.Parameters.Add(new NpgsqlParameter { ParameterName = columnName1 + i, NpgsqlValue = Convert.ToInt32(record[columnName1]), NpgsqlDbType = NpgsqlDbType.Integer });
                    command.Parameters.Add(new NpgsqlParameter { ParameterName = columnName2 + i, NpgsqlValue = Convert.ToInt32(record[columnName2]), NpgsqlDbType = NpgsqlDbType.Integer });

                    values.Add($"(@{columnName1 + i}, @{columnName2 + i})");
                }

                var tableName = moduleName + "_" + relatedModuleName;

                if (relationId > 0)
                    tableName += "_" + relationId;

                var sql = $"INSERT INTO \"{tableName}_d\" (\n\t{string.Join(",\n\t", columns)}\n) \nVALUES {string.Join(",\n\t", values)}";
                command.CommandText = sql;

                if (command.Connection.State != ConnectionState.Open)
                    command.Connection.Open();

                try
                {
                    result = await command.ExecuteNonQueryAsync();
                }
                catch (PostgresException ex)
                {
                    //If table does not exist change table names and try to add relation again ( This situation for only many to many relations ).
                    if (ex.SqlState == PostgreSqlStateCodes.UndefinedTable && !twoway)
                    {
                        result = await AddRelations(records, relatedModuleName, moduleName, relationId, true);
                        return result;
                    }

                    throw;
                }

                //if (result > 0)
                //{
                //    // Update warehouse record
                //    if (string.IsNullOrWhiteSpace(_warehouse?.DatabaseName))
                //        throw new Exception("Warehouse cannot be null during create/update/delete record.");

                //    if (_warehouse.DatabaseName != "0")
                //        BackgroundJob.Enqueue(() => _warehouse.AddRelations(records, moduleName, relatedModuleName, _warehouse.DatabaseName, relationId));
                //}
            }

            return result;
        }

        public async Task<int> DeleteRelation(JObject record, string moduleName, string relatedModuleName, int relationId = 0, bool twoway = false)
        {
            int result = 0;

            using (var command = (NpgsqlCommand)DbContext.Database.GetDbConnection().CreateCommand())
            {
                var columnName1 = moduleName + "_id";
                var columnName2 = relatedModuleName + "_id";

                command.Parameters.Add(new NpgsqlParameter { ParameterName = columnName1, NpgsqlValue = Convert.ToInt32(record[columnName1]), NpgsqlDbType = NpgsqlDbType.Integer });
                command.Parameters.Add(new NpgsqlParameter { ParameterName = columnName2, NpgsqlValue = Convert.ToInt32(record[columnName2]), NpgsqlDbType = NpgsqlDbType.Integer });
                var tableName = moduleName + "_" + relatedModuleName;

                if (relationId > 0)
                    tableName += "_" + relationId;

                var whereSql = $"{columnName1} = @{columnName1} AND {columnName2} = @{columnName2}";
                var sql = $"DELETE FROM \"{tableName}_d\" \nWHERE {whereSql}";
                command.CommandText = sql;

                if (command.Connection.State != ConnectionState.Open)
                    command.Connection.Open();

                try
                {
                    result = await command.ExecuteNonQueryAsync();
                }
                catch (PostgresException ex)
                {
                    //If table does not exist change table names and try to delete relation again ( This situation for only many to many relations ).
                    if (ex.SqlState == PostgreSqlStateCodes.UndefinedTable && !twoway)
                    {
                        result = await DeleteRelation(record, relatedModuleName, moduleName, relationId, true);
                        return result;
                    }
                    else
                        throw;
                }

                //if (result > 0)
                //{
                //    // Update warehouse record
                //    if (string.IsNullOrWhiteSpace(_warehouse?.DatabaseName))
                //        throw new Exception("Warehouse cannot be null during create/update/delete record.");

                //    if (_warehouse.DatabaseName != "0")
                //        BackgroundJob.Enqueue(() => _warehouse.DeleteRelation(record, moduleName, relatedModuleName, _warehouse.DatabaseName, relationId));
                //}
            }

            return result;
        }

        public void SetPicklists(Module module, JObject record, string picklistLanguage)
        {
            var sql = RecordHelper.GenerateGetPicklistItemsSql(module, record, picklistLanguage);

            if (string.IsNullOrEmpty(sql))
                return;

            var picklistItems = DbContext.Database.SqlQueryDynamic(sql);
            RecordHelper.SetPicklistItems(module, record, picklistLanguage, picklistItems);
        }

        public void MultiselectsToString(Module module, JObject record)
        {
            RecordHelper.MultiselectsToString(module, record);
        }

        public void ArrayToString(Module module, JObject record)
        {
            RecordHelper.TagToString(module, record);
        }

        public async Task<int> UpdateSystemData(int createdBy, DateTime createdAt, string tenantLanguage, int appId)
        {
            var sql = RecordHelper.GenerateSystemDataUpdateSql(createdBy, createdAt);

            var result = await DbContext.Database.ExecuteSqlCommandAsync(sql);

            return result;
        }

        public async Task<int> UpdateSampleData(PlatformUser user)
        {
            var modules = DbContext.Modules.ToList();
            var sql = RecordHelper.GenerateSampleDataUpdateSql(modules, user);

            if (string.IsNullOrEmpty(sql))
                return 0;

            return await DbContext.Database.ExecuteSqlCommandAsync(sql);
        }

        public async Task<int> DeleteSampleData(List<Module> modules)
        {
            var sql = RecordHelper.GenerateSampleDataDeleteSql(modules);

            return await DbContext.Database.ExecuteSqlCommandAsync(sql);
        }

        /// <summary>
        /// Deletes records phyically as a bulk by user id. Use with caution!
        /// </summary>
        /// <param name="modules"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task DeleteBulkRecordsPhysicallyByUserId(List<Module> modules, int userId)
        {
            List<string> retryList = new List<string>();

            foreach (var module in modules)
            {
                try
                {
                    await DbContext.Database.ExecuteSqlCommandAsync($"DELETE FROM {module.Name}_d WHERE created_by={userId};");
                }
                catch (Exception)
                {
                    retryList.Add(module.Name);
                }
            }

            int cid = 0;
            while (retryList.Count > 0)
            {
                try
                {
                    await DbContext.Database.ExecuteSqlCommandAsync($"DELETE FROM {retryList[cid]}_d WHERE created_by={userId};");
                    retryList.RemoveAt(cid);
                    cid++;
                }
                catch (Exception)
                {
                    cid++;
                }

                if (cid > retryList.Count) cid = 0;
            }
        }

        public JObject GetLookupIds(JArray lookupRequest)
        {
            var lookups = new JObject();

            foreach (var lookupItem in lookupRequest)
            {
                var sql = RecordHelper.GenerateGetLookupIdsSql((string)lookupItem["type"], (string)lookupItem["field"], (JArray)lookupItem["values"]);
                lookups[(string)lookupItem["type"]] = DbContext.Database.SqlQueryDynamic(sql);
            }

            return lookups;
        }

        public async Task<int> CreateBulk(JArray records, Module module)
        {
            var currentUserId = DbContext.GetCurrentUserId();
            var now = DateTime.UtcNow;
            var sql = RecordHelper.GenerateBulkInsertSql(records, module, currentUserId, now);
            var result = await DbContext.Database.ExecuteSqlCommandAsync(sql);

            //if (result > 0)
            //{
            //    // Create warehouse record
            //    if (string.IsNullOrWhiteSpace(_warehouse?.DatabaseName))
            //        throw new Exception("Warehouse cannot be null during create/update/delete record.");

            //    if (_warehouse.DatabaseName != "0")
            //        BackgroundJob.Enqueue(() => _warehouse.CreateRecordBulk((int)records[0]["import_id"], _warehouse.DatabaseName, module.Name, CurrentUser));
            //}

            return result;
        }

        public JArray LookupUser(LookupUserRequest request)
        {
            //if needs current user in lookup send moduleId = 0
            var sql = RecordHelper.GenerateLookupUserSql(request.ModuleId == 0 ? 1 : request.ModuleId, request.SearchTerm, request.IsReadonly, request.ModuleId == 0 ? 0 : CurrentUser.UserId);
            var records = DbContext.Database.SqlQueryDynamic(sql);

            return records;
        }

        public decimal CalculateBalance(string currentTransactionType, int id)
        {
            string type, transactionType1, transactionType2;

            switch (currentTransactionType)
            {
                case "sales_invoice":
                case "collection":
                    type = "customer";
                    transactionType1 = "collection";
                    transactionType2 = "sales_invoice";
                    break;
                case "purchase_invoice":
                case "payment":
                    type = "supplier";
                    transactionType1 = "payment";
                    transactionType2 = "purchase_invoice";
                    break;
                default:
                    throw new Exception("TransactionType must be sales_invoice, collection, purchase_invoice or payment.");
            }

            var sql = RecordHelper.GenerateBalanceSql(type, id, transactionType1, transactionType2);
            var record = (JObject)DbContext.Database.SqlQueryDynamic(sql).FirstOrDefault();
            decimal balance = 0;

            if (record != null && !record.IsNullOrEmpty() && !record["balance"].IsNullOrEmpty())
                balance = (decimal)record["balance"];

            return balance;
        }

        private void GetRoleBasedInfo(string moduleName, out string owners, out string userGroups)
        {
            var sqlRoleBased = RecordHelper.GenerateRoleBasedSql(moduleName, CurrentUser.UserId);
            var roleBasedResult = DbContext.Database.SqlQueryDynamic(sqlRoleBased);

            if (roleBasedResult.IsNullOrEmpty())
                roleBasedResult = DbContext.Database.SqlQueryDynamic(sqlRoleBased);

            if (roleBasedResult.IsNullOrEmpty())
                throw new Exception("Role based info cannot be null. TenantId: " + CurrentUser.TenantId + " UserId:" + CurrentUser.UserId + " Sql: " + sqlRoleBased);

            var roleBased = roleBasedResult.First();
            var isAdmin = (bool)roleBased["has_admin_rights"];
            var sharing = (int)roleBased["sharing"];
            owners = null;
            userGroups = null;

            if (!isAdmin && sharing == 1)
            {
                var modulePermission = false;
                owners = (string)roleBased["owners"];
                var ownersArray = owners.Split(',');

                if (!ownersArray.Contains(CurrentUser.UserId.ToString()))
                {
                    if (!string.IsNullOrEmpty(owners))
                        owners += "," + CurrentUser.UserId;
                    else
                        owners = CurrentUser.UserId.ToString();
                }

                var modules = (string)roleBased["modules"];
                if (string.IsNullOrEmpty(modules))
                    modulePermission = true;
                else
                {
                    var modulesArray = modules.Split(',');
                    foreach (var module in modulesArray)
                    {
                        if (module == moduleName)
                            modulePermission = true;
                    }
                }

                if (modulePermission)
                {
                    var customOwners = (string)roleBased["custom_owners"];
                    var sharedUserId = (string)roleBased["shared_user_id"];
                    var ownersArr = owners.Split(',');

                    if (!string.IsNullOrEmpty(customOwners))
                    {
                        var customOwnersArray = customOwners.Split(',');

                        if (customOwnersArray.Length > 0)
                        {
                            foreach (var customOwner in customOwnersArray)
                            {
                                int result = Array.IndexOf(ownersArr, customOwner);
                                if (result < 0)
                                    owners += "," + customOwner;
                            }

                            if (Array.IndexOf(ownersArr, sharedUserId) < 0)
                                owners += "," + sharedUserId;
                        }
                    }

                    var customUserGroups = (string)roleBased["user_group_owners"];

                    if (!string.IsNullOrEmpty(customUserGroups))
                    {
                        var customUserGroupsArray = customUserGroups.Split(',');

                        if (customUserGroupsArray.Length > 0)
                        {
                            foreach (var customUserGroup in customUserGroupsArray)
                            {
                                string[] currentOwnersArr = owners.Split(',');
                                int result = Array.IndexOf(currentOwnersArr, customUserGroup);
                                if (result < 0)
                                    owners += "," + customUserGroup;
                            }
                        }
                    }
                }

                var sqlUserGroup = RecordHelper.GenerateUserGroupSql(CurrentUser.UserId);
                var userGroupList = DbContext.Database.SqlQueryDynamic(sqlUserGroup).Select(x => x["group_id"]).ToList();

                if (userGroupList.Count > 0)
                    userGroups = string.Join(",", userGroupList);
            }
        } 
    }
}