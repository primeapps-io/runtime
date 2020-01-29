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
using PrimeApps.Model.Enums;

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

        public async Task<JObject> GetById(Module module, int recordId, bool roleBasedEnabled = true, ICollection<Module> lookupModules = null, bool deleted = false, bool profileBasedEnabled = true, OperationType operation = OperationType.read)
        {
            string owners = null;
            string userGroups = null;

            if (roleBasedEnabled && module.Fields.Any(x => x.Name == "owner") && module.Name != "users" && module.Name != "profiles" && module.Name != "roles")
                GetRoleBasedInfo(module.Name, out owners, out userGroups);

            var sql = RecordHelper.GenerateGetSql(module, lookupModules, recordId, owners, CurrentUser.UserId, userGroups, deleted);
            var data = DbContext.Database.SqlQueryDynamic(sql).FirstOrDefault();
            var record = new JObject();

            if (data != null)
            {
                if (profileBasedEnabled && module.Name != "users" && module.Name != "profiles" && module.Name != "roles")
                    record = await RecordPermissionControl(module.Name, CurrentUser.UserId, (JObject)data, operation);
            }

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

        public async Task<JArray> Find(string moduleName, FindRequest findRequest, bool roleBasedEnabled = true, bool profileBasedEnabled = true, int timezoneOffset = 180)
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
                var newRecords = new JArray();

                foreach (var record in records)
                {
                    var newRecord = record.DeepClone();

                    if (!record.IsNullOrEmpty())
                    {
                        if (profileBasedEnabled && moduleName != "users" && moduleName != "profiles" && moduleName != "roles")
                            newRecord = await RecordPermissionControl(moduleName, CurrentUser.UserId, (JObject)record, OperationType.read);

                        if (!newRecord["shared_users"].IsNullOrEmpty() || !newRecord["shared_user_groups"].IsNullOrEmpty())
                        {
                            var userIds = newRecord["shared_users"].IsNullOrEmpty() ? "0" : string.Join(",", (JArray)newRecord["shared_users"]);
                            var userGroupIds = newRecord["shared_user_groups"].IsNullOrEmpty() ? "0" : string.Join(",", (JArray)newRecord["shared_user_groups"]);
                            var sqlSharedRead = RecordHelper.GenerateSharedSql(userIds, userGroupIds);
                            newRecord["shared_read"] = DbContext.Database.SqlQueryDynamic(sqlSharedRead);
                        }

                        if (!newRecord["shared_users_edit"].IsNullOrEmpty() || !newRecord["shared_user_groups_edit"].IsNullOrEmpty())
                        {
                            var userIdsEdit = newRecord["shared_users_edit"].IsNullOrEmpty() ? "0" : string.Join(",", (JArray)newRecord["shared_users_edit"]);
                            var userGroupIdsEdit = newRecord["shared_user_groups_edit"].IsNullOrEmpty() ? "0" : string.Join(",", (JArray)newRecord["shared_user_groups_edit"]);
                            var sqlSharedEdit = RecordHelper.GenerateSharedSql(userIdsEdit, userGroupIdsEdit);
                            newRecord["shared_edit"] = DbContext.Database.SqlQueryDynamic(sqlSharedEdit);
                        }
                    }

                    newRecords.Add(newRecord);
                }
            }

            return records;
        }

        public async Task<JArray> GetAllById(string moduleName, List<int> recordIds, bool roleBasedEnabled = true)
        {
            string owners = null;
            string userGroups = null;

            if (roleBasedEnabled && moduleName != "users")
                GetRoleBasedInfo(moduleName, out owners, out userGroups);

            var sql = RecordHelper.GenerateGetAllByIdSql(moduleName, recordIds, owners, CurrentUser.UserId, userGroups);
            var records = DbContext.Database.SqlQueryDynamic(sql);

            var newRecords = new JArray();

            foreach (var record in records)
            {
                var newRecord = await RecordPermissionControl(moduleName, CurrentUser.UserId, (JObject)record, OperationType.read);
                newRecords.Add(newRecord);
            }

            return newRecords;
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

        #region Profile based permission controls for records
        public async Task<JObject> RecordPermissionControl(string moduleName, int userId, JObject record, OperationType operation)
        {
            if (record.IsNullOrEmpty())
                return null;

            var user = await DbContext.Users
            .Include(q => q.Profile)
            .ThenInclude(q => q.Permissions)
            .Include(q => q.Groups)
            .FirstOrDefaultAsync(q => q.Id == userId);

            var profile = user.Profile;
            var module = await DbContext.Modules
            .Include(mod => mod.Sections)
            .ThenInclude(section => section.Permissions)
            .Include(mod => mod.Fields)
            .ThenInclude(field => field.Permissions)
            .Include(mod => mod.Relations)
            .FirstOrDefaultAsync(q => !q.Deleted && q.Name == moduleName);

            var isCustomSharePermission = SharedPermissionCheck(record, user, operation);
            //Module CRUD permisson control
            var modulePermission = ProfilePermissionCheck(profile.Permissions.Where(q => q.ModuleId == module.Id && q.Type == EntityType.Module).ToList(), operation);

            switch (operation)
            {
                case OperationType.insert:
                    if (modulePermission == null)
                        return null;
                    else
                    {
                        //record = SectionPermission(module, record, user, operation);
                        //record = await RelationModulePermission(module, record, user, operation);
                        //record = FieldPermission(module, record, user, operation);
                        return record;
                    }
                case OperationType.update:
                    if (isCustomSharePermission)
                        return record;

                    if (modulePermission == null)
                        return null;
                    else
                    {
                        record = SectionPermission(module, record, user, operation);
                        record = await RelationModulePermission(module, record, user, operation);
                        record = await FieldPermission(module, record, user, operation);
                        ///Todo lookup alanlar için kontrol lazım.
                        return record;
                    }
                case OperationType.read:
                    if (isCustomSharePermission)
                        return record;

                    if (modulePermission == null)
                        return null;
                    else
                    {
                        record = SectionPermission(module, record, user, operation);
                        record = await RelationModulePermission(module, record, user, operation);
                        record = await FieldPermission(module, record, user, operation);
                        ///Todo lookup alanlar için kontrol lazım.
                        return record;
                    }
                case OperationType.delete:
                    if (modulePermission == null)
                        return null;

                    return record;
                default:
                    return record;
            }
        }

        private ProfilePermission ProfilePermissionCheck(List<ProfilePermission> profilePermission, OperationType operation)
        {
            switch (operation)
            {
                case OperationType.insert:
                    return profilePermission.FirstOrDefault(q => q.Write);
                case OperationType.update:
                    return profilePermission.FirstOrDefault(q => q.Modify);
                case OperationType.read:
                    return profilePermission.FirstOrDefault(q => q.Read);
                case OperationType.delete:
                    return profilePermission.FirstOrDefault(q => q.Remove);
                default:
                    return null;
            }
        }

        private JObject SectionPermission(Module module, JObject record, TenantUser user, OperationType operation)
        {
            //Module Section permission control
            foreach (var section in module.Sections)
            {
                //Herhangi bir yetki girilmis mi diye kontrol ediyoruz. Eger girilmemisse default olarak yetkisi var demektir.
                if (section.Permissions != null && section.Permissions.Count > 0)
                {
                    //Aktif olan User'in profile bilgisine gore bir yetki eklenmis mi diye bakıyoruz.
                    var sectionPermissionList = section.Permissions.Where(q => q.ProfileId == user.Profile.Id && !q.Deleted).ToList();

                    if (sectionPermissionList == null)
                        continue;

                    var sectionPermission = SectionPermissionCheck(sectionPermissionList, operation);

                    //Bir yetkilendirme yapilmis ve aktif olan operation icin yetkisi yoksa record uzerinden o section'ini ve section'a ait olan field'leri siliyoruz.
                    if (sectionPermission == null)
                    {
                        var fields = module.Fields.Where(q => q.Section == section.Name).Select(q => q.Name).ToList();
                        record = ClearRecord(record, fields: fields);
                    }
                }
            }

            return record;
        }

        private async Task<JObject> RelationModulePermission(Module module, JObject record, TenantUser user, OperationType operation)
        {
            //Module Relations CRUD permission control

            foreach (var relation in module.Relations)
            {
                var permissionList = user.Profile.Permissions.Where(q => q.ModuleId == relation.ModuleId && q.Type == EntityType.Module).ToList();
                var relationModulePermission = ProfilePermissionCheck(permissionList, operation);

                //iliskili olan module icin profile ve operation bazli yetki kontrolu sonunda yetkisi yoksa ilgili module ile iliskili alanlari record'dan siliyoruz.
                if (relationModulePermission == null)
                {
                    var relationModule = await DbContext.Modules.FirstOrDefaultAsync(q => q.Name == relation.RelatedModule && !q.Deleted);

                    if (relationModule != null)
                        record = ClearRecord(record, $"{relationModule.LabelEnSingular.ToLower()}.");
                }
            }

            return record;
        }

        private async Task<JObject> FieldPermission(Module module, JObject record, TenantUser user, OperationType operation)
        {
            var fieldRemoveList = new List<string>();

            foreach (var field in module.Fields)
            {
                //Field icin herhangi bir yetkilendirme yapilmamissa ve lookup tipinde ise lookup module'ne gore yetki kontrolu yapiyoruz.
                if (field.Permissions == null || field.Permissions.Count <= 0)
                {
                    if (field.DataType == DataType.Lookup)
                    {
                        var lookupFields = await LookupModulePermission(field, user, operation);

                        //Lookup module icin yetkisi yoksa eger, record uzerinden silmek icin listeye ekliyoruz.
                        if (lookupFields != null && lookupFields.Count > 0)
                            record = ClearRecord(record, fields: lookupFields);
                    }

                    continue;
                }

                //Field icin yetkilendirme eklenmisse gecerli user'in profile'ne gore yetki kontrolu yapiyoruz.
                var permissionList = field.Permissions.Where(q => q.ProfileId == user.Profile.Id).ToList();

                if (permissionList == null || permissionList.Count <= 0)
                {
                    if (field.DataType == DataType.Lookup)
                    {
                        var lookupFields = await LookupModulePermission(field, user, operation);

                        if (lookupFields != null && lookupFields.Count > 0)
                            record = ClearRecord(record, fields: lookupFields);
                    }
                    continue;
                }


                var permissionCheck = FieldPermissionCheck(permissionList, operation);

                if (permissionCheck == null)
                {
                    if (field.DataType == DataType.Lookup)
                    {
                        var lookupFields = await LookupModulePermission(field, user, operation);

                        if (lookupFields != null && lookupFields.Count > 0)
                            record = ClearRecord(record, fields: lookupFields);
                    }
                    else
                        fieldRemoveList.Add(field.Name);
                }  
            }

            return fieldRemoveList.Count > 0 ? ClearRecord(record, fields: fieldRemoveList) : record;
        }

        private async Task<List<string>> LookupModulePermission(Field field, TenantUser user, OperationType operation)
        {
            var lookupModule = await DbContext.Modules.Where(q => q.Name == field.LookupType && !q.Deleted).FirstOrDefaultAsync();

            if (lookupModule == null)
                return null;

            var result = ProfilePermissionCheck(user.Profile.Permissions.Where(q => q.ModuleId == lookupModule.Id && q.Type == EntityType.Module).ToList(), operation);
            var fieldList = new List<string>();

            var primaryLookupField = lookupModule.Fields.FirstOrDefault(q => q.PrimaryLookup && !q.Deleted);
            var primaryField = lookupModule.Fields.FirstOrDefault(q => q.Primary && !q.Deleted);

            var prefix = primaryLookupField != null ? primaryLookupField.Name : primaryField.Name;

            foreach (var item in lookupModule.Fields)
            {
                if (item.Name != "id" && !item.Primary)
                {
                    //Lookup module icin yetkisi yoksa. 
                    if (result == null)
                    {
                        fieldList.Add($"{prefix}.{item.Name}");
                        fieldList.Add($"{field.Name}.{item.Name}");
                        fieldList.Add($"{prefix}.{lookupModule.Name}.{item.Name}"); //find methodu ile donen record'lar icin ekiyoruz.
                    }
                    else
                    {
                        //Lookup module icin yetkisi varsa field bazli kontrol yetki yapiyoruz.
                        var fieldCheckResult = FieldPermissionCheck(item.Permissions.ToList(), operation);

                        if (fieldCheckResult == null)
                        {
                            fieldList.Add($"{prefix}.{item.Name}");
                            fieldList.Add($"{field.Name}.{item.Name}");
                            fieldList.Add($"{prefix}.{lookupModule.Name}.{item.Name}"); //find methodu ile donen record'lar icin ekiyoruz.
                        }
                    }
                }
            }

            return fieldList;
        }

        private bool SharedPermissionCheck(JObject record, TenantUser user, OperationType operation)
        {
            var extraControl = false;

            if (operation == OperationType.read)
            {
                extraControl = true;

                if (!record["shared_users"].IsNullOrEmpty() || !record["shared_user_groups"].IsNullOrEmpty())
                {
                    var userIdList = record["shared_users"].ToObject<List<string>>();
                    var groupIdList = record["shared_user_groups"].ToObject<List<string>>();
                    var result = false;

                    foreach (var id in groupIdList)
                    {
                        if (user.Groups.Any(q => q.UserGroupId.ToString() == id))
                        {
                            result = true;
                            break;
                        }
                    }

                    var permissionResult = (userIdList != null && userIdList.Any(q => q == user.Id.ToString())) || result;

                    //Eger yetki mevcut ise kontrol islemini bitiriyoruz. Yoksa ekstra kontrol icin devam edicek.
                    if (permissionResult)
                        return permissionResult;
                }
            }

            if (operation == OperationType.update || extraControl)
            {
                if (!record["shared_users_edit"].IsNullOrEmpty() || !record["shared_user_groups_edit"].IsNullOrEmpty())
                {
                    var userIdList = record["shared_users_edit"].ToObject<List<string>>();
                    var groupIdList = record["shared_user_groups_edit"].ToObject<List<string>>();
                    var result = false;

                    foreach (var id in groupIdList)
                    {
                        if (user.Groups.Any(q => q.UserGroupId.ToString() == id))
                        {
                            result = true;
                            break;
                        }
                    }

                    return (userIdList != null && userIdList.Any(q => q == user.Id.ToString())) || result;
                }
            }

            return false;
        }

        private SectionPermission SectionPermissionCheck(List<SectionPermission> sectionPermissions, OperationType operation)
        {
            switch (operation)
            {
                case OperationType.insert:
                case OperationType.update:
                case OperationType.delete:
                    return sectionPermissions.FirstOrDefault(q => q.Type == SectionPermissionType.Full);
                case OperationType.read:
                    return sectionPermissions.FirstOrDefault(q => q.Type == SectionPermissionType.ReadOnly || q.Type == SectionPermissionType.Full);
                default:
                    return null;
            }
        }

        private FieldPermission FieldPermissionCheck(List<FieldPermission> fieldPermissions, OperationType operation)
        {
            switch (operation)
            {
                case OperationType.insert:
                case OperationType.update:
                case OperationType.delete:
                    return fieldPermissions.FirstOrDefault(q => q.Type == FieldPermissionType.Full);
                case OperationType.read:
                    return fieldPermissions.FirstOrDefault(q => q.Type == FieldPermissionType.ReadOnly || q.Type == FieldPermissionType.Full);
                default:
                    return null;
            }
        }

        private JObject ClearRecord(JObject record, string key = null, List<string> fields = null)
        {
            var newRecord = (JObject)record.DeepClone();

            if (!string.IsNullOrEmpty(key))
            {
                foreach (var prop in record)
                {
                    if (prop.Key.StartsWith(key))
                        newRecord.Remove(prop.Key);
                }
            }

            if (fields != null)
            {
                foreach (var field in fields)
                {
                    newRecord.Remove(field);
                }
            }

            return newRecord;
        }
        #endregion Profile based permission controls for records
    }
}