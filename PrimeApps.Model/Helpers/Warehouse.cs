using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using Database = Microsoft.SqlServer.Management.Smo.Database;
using DataType = Microsoft.SqlServer.Management.Smo.DataType;
using View = Microsoft.SqlServer.Management.Smo.View;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace PrimeApps.Model.Helpers
{
    public class Warehouse
    {
        private IAnalyticRepository _analyticRepository;
        private IConfiguration _configuration;

        public string DatabaseName { get; set; }

        public Warehouse(IAnalyticRepository analyticRepository, IConfiguration configuration)
        {
            _analyticRepository = analyticRepository;
            _configuration = configuration;
        }

        public void CreateUser(Model.Entities.Platform.PlatformWarehouse warehouseEntity)
        {
            var connection = new SqlConnection(GetConnectionString(warehouseEntity.DatabaseName));

            using (connection)
            {
                connection.Open();

                var sqlUserRole = "CREATE ROLE reader_view_creator AUTHORIZATION [dbo];\n" +
                                  "GRANT CREATE VIEW TO reader_view_creator;" +
                                  "GRANT SELECT, ALTER, VIEW DEFINITION ON SCHEMA::dbo TO reader_view_creator;" +
                                  $"CREATE USER {warehouseEntity.DatabaseUser} FROM LOGIN {warehouseEntity.DatabaseUser};\n" +
                                  $"EXEC sp_addrolemember 'reader_view_creator', '{warehouseEntity.DatabaseUser}';";

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlUserRole;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }

                var sqlTrigger = "CREATE TRIGGER trig_db_BlockAlterDropTable\n" +
                                 "ON DATABASE\n" +
                                 "FOR DROP_TABLE, ALTER_TABLE\n" +
                                 "AS\n" +
                                 "BEGIN\n" +
                                 "\tIF IS_MEMBER('reader_view_creator') = 1\n" +
                                 "\tBEGIN\n" +
                                 "\t\tPRINT 'You are not authorized to alter or drop a table.';\n" +
                                 "\t\tROLLBACK TRANSACTION;\n" +
                                 "\tEND;\n" +
                                 "END;";

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlTrigger;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }

        public void CreateSchema(Entities.Platform.PlatformWarehouse warehouseEntity, ICollection<Module> modules, string tenantLanguage, string connectionString)
        {
            //Connect sql database using SMO
            var connection = new SqlConnection(connectionString);
            var serverConnection = new ServerConnection(connection);
            var server = new Server(serverConnection);
            var database = server.Databases[warehouseEntity.DatabaseName];
            var existingTables = database.Tables.Cast<Table>().ToList();

            //First delete all existing tables
            foreach (var tbl in existingTables)
            {
                var table = database.Tables[tbl.Name];
                table.Refresh();
                table.Drop();
            }

            //Create dynamic tables
            foreach (var module in modules)
            {
                CreateTables(database, module, tenantLanguage);
            }

            //Create roles table
            CreateRolesTable(database);

            //Create profiles table
            CreateProfilesTables(database);

            //Create users table
            CreateUsersTable(database);
        }

        public void CreateTables(Database database, Module module, string tenantLanguage)
        {
            var table = new Table(database, module.Name + "_d");

            //Create identity column
            var idColumn = new Column(table, "id", DataType.Int);
            idColumn.Nullable = false;
            table.Columns.Add(idColumn);

            //Create all columns of module
            var fields = module.Fields.OrderBy(x => x.Order);

            foreach (var field in fields)
            {
                if (ModuleHelper.SystemFieldsExtended.Contains(field.Name))
                    continue;

                CreateColumn(database, table, module, field);
            }

            //Opportunities module specific fields
            if (module.Name == "opportunities")
            {
                //Create forecast columns
                var forecastTypeColumn = new Column(table, "forecast_type", DataType.NVarChar(101));
                forecastTypeColumn.Nullable = true;
                table.Columns.Add(forecastTypeColumn);

                var forecastCategoryColumn = new Column(table, "forecast_category", DataType.NVarChar(101));
                forecastCategoryColumn.Nullable = true;
                table.Columns.Add(forecastCategoryColumn);

                var forecastYearColumn = new Column(table, "forecast_year", DataType.Int);
                forecastYearColumn.Nullable = true;
                table.Columns.Add(forecastYearColumn);

                var forecastMonthColumn = new Column(table, "forecast_month", DataType.Int);
                forecastMonthColumn.Nullable = true;
                table.Columns.Add(forecastMonthColumn);

                var forecastQuarterColumn = new Column(table, "forecast_quarter", DataType.Int);
                forecastQuarterColumn.Nullable = true;
                table.Columns.Add(forecastQuarterColumn);

                //Create nonclustered index for forecast fields
                var forecastTypeIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_forecast_type");
                table.Indexes.Add(forecastTypeIx);
                forecastTypeIx.IndexedColumns.Add(new IndexedColumn(forecastTypeIx, "forecast_type", true));
                forecastTypeIx.IsClustered = false;
                forecastTypeIx.IsUnique = false;
                forecastTypeIx.IndexKeyType = IndexKeyType.None;

                var forecastCategoryIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_forecast_category");
                table.Indexes.Add(forecastCategoryIx);
                forecastCategoryIx.IndexedColumns.Add(new IndexedColumn(forecastCategoryIx, "forecast_category", true));
                forecastCategoryIx.IsClustered = false;
                forecastCategoryIx.IsUnique = false;
                forecastCategoryIx.IndexKeyType = IndexKeyType.None;

                var forecastYearIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_forecast_year");
                table.Indexes.Add(forecastYearIx);
                forecastYearIx.IndexedColumns.Add(new IndexedColumn(forecastYearIx, "forecast_year", true));
                forecastYearIx.IsClustered = false;
                forecastYearIx.IsUnique = false;
                forecastYearIx.IndexKeyType = IndexKeyType.None;

                var forecastMonthIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_forecast_month");
                table.Indexes.Add(forecastMonthIx);
                forecastMonthIx.IndexedColumns.Add(new IndexedColumn(forecastMonthIx, "forecast_month", true));
                forecastMonthIx.IsClustered = false;
                forecastMonthIx.IsUnique = false;
                forecastMonthIx.IndexKeyType = IndexKeyType.None;

                var forecastQuarterIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_forecast_quarter");
                table.Indexes.Add(forecastQuarterIx);
                forecastQuarterIx.IndexedColumns.Add(new IndexedColumn(forecastQuarterIx, "forecast_quarter", true));
                forecastQuarterIx.IsClustered = false;
                forecastQuarterIx.IsUnique = false;
                forecastQuarterIx.IndexKeyType = IndexKeyType.None;
            }

            //Activities module specific fields
            if (module.Name == "activities")
            {
                //Create activity_type_system column
                var activityTypeColumn = new Column(table, "activity_type_system", DataType.VarChar(10));
                activityTypeColumn.Nullable = true;
                table.Columns.Add(activityTypeColumn);

                //Create nonclustered index for activity_type_system field
                var activityTypeIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_activity_type_system");
                table.Indexes.Add(activityTypeIx);
                activityTypeIx.IndexedColumns.Add(new IndexedColumn(activityTypeIx, "activity_type_system", true));
                activityTypeIx.IsClustered = false;
                activityTypeIx.IsUnique = false;
                activityTypeIx.IndexKeyType = IndexKeyType.None;
            }

            //CurrentAccounts module specific fields
            if (module.Name == "current_accounts")
            {
                //Create transaction_type_system column
                var transactionTypeColumn = new Column(table, "transaction_type_system", DataType.VarChar(30));
                transactionTypeColumn.Nullable = true;
                table.Columns.Add(transactionTypeColumn);

                //Create nonclustered index for transaction_type_system field
                var transactionTypeIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_transaction_type_system");
                table.Indexes.Add(transactionTypeIx);
                transactionTypeIx.IndexedColumns.Add(new IndexedColumn(transactionTypeIx, "transaction_type_system", true));
                transactionTypeIx.IsClustered = false;
                transactionTypeIx.IsUnique = false;
                transactionTypeIx.IndexKeyType = IndexKeyType.None;
            }

            //Create system fields
            var sharedUsersColumn = new Column(table, "shared_users", DataType.VarChar(2000));
            sharedUsersColumn.Nullable = true;
            table.Columns.Add(sharedUsersColumn);

            var sharedUserGroupsColumn = new Column(table, "shared_user_groups", DataType.VarChar(2000));
            sharedUserGroupsColumn.Nullable = true;
            table.Columns.Add(sharedUserGroupsColumn);

            var isConvertedColumn = new Column(table, "is_converted", DataType.Bit);
            isConvertedColumn.Nullable = false;
            table.Columns.Add(isConvertedColumn);

            var masterIdColumn = new Column(table, "master_id", DataType.Int);
            masterIdColumn.Nullable = true;
            table.Columns.Add(masterIdColumn);

            var importIdColumn = new Column(table, "import_id", DataType.Int);
            importIdColumn.Nullable = true;
            table.Columns.Add(importIdColumn);

            var createdByColumn = new Column(table, "created_by", DataType.Int);
            createdByColumn.Nullable = false;
            table.Columns.Add(createdByColumn);

            var updatedByColumn = new Column(table, "updated_by", DataType.Int);
            updatedByColumn.Nullable = true;
            table.Columns.Add(updatedByColumn);

            var createdAtColumn = new Column(table, "created_at", DataType.DateTime);
            createdAtColumn.Nullable = false;
            table.Columns.Add(createdAtColumn);

            var updatedAtColumn = new Column(table, "updated_at", DataType.DateTime);
            updatedAtColumn.Nullable = true;
            table.Columns.Add(updatedAtColumn);

            var deletedColumn = new Column(table, "deleted", DataType.Bit);
            deletedColumn.Nullable = false;
            table.Columns.Add(deletedColumn);

            //Create nonclustered index for system fields
            var sharedUsersIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_shared_users");
            table.Indexes.Add(sharedUsersIx);
            sharedUsersIx.IndexedColumns.Add(new IndexedColumn(sharedUsersIx, "shared_users", true));
            sharedUsersIx.IsClustered = false;
            sharedUsersIx.IsUnique = false;
            sharedUsersIx.IndexKeyType = IndexKeyType.None;

            var sharedUserGroupsIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_shared_user_groups");
            table.Indexes.Add(sharedUserGroupsIx);
            sharedUserGroupsIx.IndexedColumns.Add(new IndexedColumn(sharedUserGroupsIx, "shared_user_groups", true));
            sharedUserGroupsIx.IsClustered = false;
            sharedUserGroupsIx.IsUnique = false;
            sharedUserGroupsIx.IndexKeyType = IndexKeyType.None;

            var isConvertedIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_is_converted");
            table.Indexes.Add(isConvertedIx);
            isConvertedIx.IndexedColumns.Add(new IndexedColumn(isConvertedIx, "is_converted", true));
            isConvertedIx.IsClustered = false;
            isConvertedIx.IsUnique = false;
            isConvertedIx.IndexKeyType = IndexKeyType.None;

            var masterIdIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_master_id");
            table.Indexes.Add(masterIdIx);
            masterIdIx.IndexedColumns.Add(new IndexedColumn(masterIdIx, "master_id", true));
            masterIdIx.IsClustered = false;
            masterIdIx.IsUnique = false;
            masterIdIx.IndexKeyType = IndexKeyType.None;

            var importIdIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_import_id");
            table.Indexes.Add(importIdIx);
            importIdIx.IndexedColumns.Add(new IndexedColumn(importIdIx, "import_id", true));
            importIdIx.IsClustered = false;
            importIdIx.IsUnique = false;
            importIdIx.IndexKeyType = IndexKeyType.None;

            var createdByIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_created_by");
            table.Indexes.Add(createdByIx);
            createdByIx.IndexedColumns.Add(new IndexedColumn(createdByIx, "created_by", true));
            createdByIx.IsClustered = false;
            createdByIx.IsUnique = false;
            createdByIx.IndexKeyType = IndexKeyType.None;

            var updatedByIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_updated_by");
            table.Indexes.Add(updatedByIx);
            updatedByIx.IndexedColumns.Add(new IndexedColumn(updatedByIx, "updated_by", true));
            updatedByIx.IsClustered = false;
            updatedByIx.IsUnique = false;
            updatedByIx.IndexKeyType = IndexKeyType.None;

            var createdAtIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_created_at");
            table.Indexes.Add(createdAtIx);
            createdAtIx.IndexedColumns.Add(new IndexedColumn(createdAtIx, "created_at", true));
            createdAtIx.IsClustered = false;
            createdAtIx.IsUnique = false;
            createdAtIx.IndexKeyType = IndexKeyType.None;

            var updatedAtIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_updated_at");
            table.Indexes.Add(updatedAtIx);
            updatedAtIx.IndexedColumns.Add(new IndexedColumn(updatedAtIx, "updated_at", true));
            updatedAtIx.IsClustered = false;
            updatedAtIx.IsUnique = false;
            updatedAtIx.IndexKeyType = IndexKeyType.None;

            var deletedIx = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_deleted");
            table.Indexes.Add(deletedIx);
            deletedIx.IndexedColumns.Add(new IndexedColumn(deletedIx, "deleted", true));
            deletedIx.IsClustered = false;
            deletedIx.IsUnique = false;
            deletedIx.IndexKeyType = IndexKeyType.None;

            //Crete primary key index for the table
            var pk = new Microsoft.SqlServer.Management.Smo.Index(table, "PK_" + table.Name);
            table.Indexes.Add(pk);
            pk.IndexedColumns.Add(new IndexedColumn(pk, idColumn.Name));
            pk.IsClustered = true;
            pk.IsUnique = true;
            pk.IndexKeyType = IndexKeyType.DriPrimaryKey;

            table.Create();

            if (module.Relations != null && module.Relations.Count > 0)
            {
                var junctionTableNames = new List<string>();
                var relations = module.Relations.OrderBy(x => x.Id).ToList();

                foreach (var relation in relations)
                {
                    if (relation.Deleted == true)
                        continue;

                    if (relation.RelationType == RelationType.ManyToMany)
                    {
                        CreateJunctionTable(database, module, relation, junctionTableNames);
                    }
                }
            }

            CreateView(database, module, tenantLanguage);
        }

        public void CreateTable(string warehouseDatabaseName, string moduleName, CurrentUser currentUser, string tenantLanguage)
        {
            var warehouseConnection = _configuration.GetValue("AppSettings:WarehouseConnection", string.Empty);
            var connection = new SqlConnection();
            if (!string.IsNullOrEmpty(warehouseConnection))
            {
                connection = new SqlConnection(warehouseConnection);
            }

            var serverConnection = new ServerConnection(connection);
            var server = new Server(serverConnection);
            var database = server.Databases[warehouseDatabaseName];
            var existingTable = database.Tables[moduleName + "_d"];

            if (existingTable != null)
            {
                existingTable.Refresh();
                existingTable.Drop();
            }

            var module = GetModule(moduleName, currentUser);
            CreateTables(database, module, tenantLanguage);
        }

        public void CreateColumns(string warehouseDatabaseName, string moduleName, List<int> fieldIds, CurrentUser currentUser, string tenantLanguage)
        {
            var warehouseConnection = _configuration.GetValue("AppSettings:WarehouseConnection", string.Empty);
            var connection = new SqlConnection();
            if (!string.IsNullOrEmpty(warehouseConnection))
            {
                connection = new SqlConnection(warehouseConnection);
            }

            var serverConnection = new ServerConnection(connection);
            var server = new Server(serverConnection);
            var database = server.Databases[warehouseDatabaseName];
            var table = database.Tables[moduleName + "_d"];
            var module = GetModule(moduleName, currentUser);

            foreach (var fieldId in fieldIds)
            {
                var field = module.Fields.Single(x => x.Id == fieldId);
                CreateColumn(database, table, module, field);
            }

            table.Alter();
            AlterView(database, module, tenantLanguage, true);
        }

        public void CreateJunctionTable(string warehouseDatabaseName, string moduleName, int relationId, CurrentUser currentUser)
        {
            var warehouseConnection = _configuration.GetValue("AppSettings:WarehouseConnection", string.Empty);
            var connection = new SqlConnection();
            if (!string.IsNullOrEmpty(warehouseConnection))
            {
                connection = new SqlConnection(warehouseConnection);
            }

            var serverConnection = new ServerConnection(connection);
            var server = new Server(serverConnection);
            var database = server.Databases[warehouseDatabaseName];
            var module = GetModule(moduleName, currentUser);
            var relation = module.Relations.Single(x => x.Id == relationId);
            var junctionTableNames = new List<string>();
            var relations = module.Relations.OrderBy(x => x.Id).ToList();

            foreach (var currentRelation in relations)
            {
                if (currentRelation.Id == relationId)
                    continue;

                if (currentRelation.RelationType == RelationType.ManyToMany)
                {
                    var junctionTableName = module.Name + "_" + currentRelation.RelatedModule;

                    if (junctionTableNames.Contains(junctionTableName))
                        junctionTableName = junctionTableName + "_" + currentRelation.Id;

                    junctionTableNames.Add(junctionTableName);
                }
            }

            CreateJunctionTable(database, module, relation, junctionTableNames);
        }

        public void SyncData(ICollection<Module> modules, string databaseName, CurrentUser currentUser, string tenantLanguage)
        {
            _analyticRepository.CurrentUser = currentUser;
            _analyticRepository.TenantId = currentUser.TenantId;

            // Insert roles
            var roles = _analyticRepository.DbContext.Roles.ToList();

            foreach (var role in roles)
            {
                CreateRole(role, databaseName, tenantLanguage);
            }

            //Insert profiles
            var profiles = _analyticRepository.DbContext.Profiles.Where(x => !x.Deleted).ToList();

            foreach (var profile in profiles)
            {
                if (string.IsNullOrEmpty(profile.Name) && string.IsNullOrEmpty(profile.Description) && profile.IsPersistent)
                {
                    if (profile.HasAdminRights)
                    {
                        profile.Name = "Sistem Yöneticisi";
                        profile.Description = "Bu profil tüm yetkilere sahiptir";
                    }
                    else
                    {
                        profile.Name = "Standart Kullanıcı";
                        profile.Description = "Bu profil yönetimsel yetkilere sahip değildir";
                    }
                }

                CreateProfile(profile, databaseName, tenantLanguage);
            }

            // Insert users
            var users = _analyticRepository.DbContext.Users.Include(x => x.Profile).Include(x => x.Role).ToList();

            foreach (var user in users)
            {
                CreateTenantUser(user, databaseName, tenantLanguage);
            }

            // Insert records
            foreach (var module in modules)
            {
                var totalRequest = new FindRequest { Fields = new List<string> { "total_count()" }, Limit = 1, Offset = 0 };
                var sqlTotal = RecordHelper.GenerateFindSql(module.Name, totalRequest);
                var totalResponse = _analyticRepository.DbContext.Database.SqlQueryDynamic(sqlTotal);

                if (totalResponse.IsNullOrEmpty())
                    continue;

                var total = (int)totalResponse[0]["total_count"];

                if (total < 1)
                    continue;

                var pageCount = 1;

                if (total > 200)
                    pageCount = (int)Math.Ceiling((double)total / 200);

                for (var i = 0; i < pageCount; i++)
                {
                    var recordRequest = new FindRequest { Limit = 200, Offset = i * 200 };
                    var sqlRecords = RecordHelper.GenerateFindSql(module.Name, recordRequest);
                    var records = _analyticRepository.DbContext.Database.SqlQueryDynamic(sqlRecords);

                    foreach (JObject record in records)
                    {
                        RecordHelper.MultiselectsToString(module, record);
                        CreateRecord(record, databaseName, module);
                    }
                }

                if (module.Relations != null && module.Relations.Count > 0)
                {
                    var relations = module.Relations.OrderBy(x => x.Id).ToList();

                    foreach (var relation in relations)
                    {
                        if (relation.RelationType == RelationType.ManyToMany)
                        {
                            var manyToMany = module.Name;
                            var relationId = 0;
                            var relationsManyToMany = relations.Where(x => x.RelationType == RelationType.ManyToMany).ToList();

                            if (relations.Count(x => x.RelatedModule == relation.RelatedModule) > 1 && relationsManyToMany[0].Id != relation.Id)
                            {
                                manyToMany = manyToMany + "|" + relation.Id;
                                relationId = relation.Id;
                            }

                            var relationTotalRequest = new FindRequest { Fields = new List<string> { "total_count()" }, Limit = 1, Offset = 0, ManyToMany = manyToMany };
                            var sqlManyToManyTotal = RecordHelper.GenerateFindSql(relation.RelatedModule, relationTotalRequest);
                            var totalManyToManyResponse = _analyticRepository.DbContext.Database.SqlQueryDynamic(sqlManyToManyTotal);

                            if (totalManyToManyResponse.IsNullOrEmpty())
                                continue;

                            var manyToManyTotal = (int)totalManyToManyResponse[0]["total_count"];

                            if (manyToManyTotal < 1)
                                continue;

                            var manyToManyPageCount = 1;

                            if (manyToManyTotal > 200)
                                manyToManyPageCount = (int)Math.Ceiling((double)manyToManyTotal / 200);

                            for (var i = 0; i < manyToManyPageCount; i++)
                            {
                                var manyToManyRecordRequest = new FindRequest { Limit = 200, Offset = i * 200, ManyToMany = manyToMany };
                                var sqlManyToManyRecords = RecordHelper.GenerateFindSql(relation.RelatedModule, manyToManyRecordRequest);
                                var manyToManyRecords = _analyticRepository.DbContext.Database.SqlQueryDynamic(sqlManyToManyRecords);
                                AddRelations(manyToManyRecords, module.Name, relation.RelatedModule, databaseName, relationId);
                            }
                        }
                    }
                }
            }
        }

        public void CreateRole(Role role, string databaseName, string tenantLanguage)
        {
            var connection = new SqlConnection(GetConnectionString(databaseName));

            using (connection)
            {
                using (var command = connection.CreateCommand())
                {
                    var columns = new List<string>();
                    var values = new List<string>();

                    command.Parameters.Add(new SqlParameter { ParameterName = "id", SqlValue = role.Id, SqlDbType = SqlDbType.Int });
                    command.Parameters.Add(new SqlParameter { ParameterName = "label", SqlValue = tenantLanguage == "tr" ? role.LabelTr : role.LabelEn, SqlDbType = SqlDbType.NVarChar });
                    command.Parameters.Add(new SqlParameter { ParameterName = "owners", SqlValue = role.Owners, SqlDbType = SqlDbType.VarChar });
                    command.Parameters.Add(new SqlParameter { ParameterName = "deleted", SqlValue = role.Deleted, SqlDbType = SqlDbType.Bit });

                    foreach (SqlParameter parameter in command.Parameters)
                    {
                        columns.Add("[" + parameter.ParameterName + "]");
                        values.Add("@" + parameter.ParameterName);
                    }

                    var sql = $"INSERT INTO [roles] (\n\t{string.Join(",\n\t", columns)}\n) \nVALUES (\n\t{string.Join(",\n\t", values)}\n)";

                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    if (command.Connection.State != ConnectionState.Open)
                        connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }

        public void CreateRole(int roleId, string databaseName, CurrentUser currentUser, string tenantLanguage)
        {
            if (string.IsNullOrEmpty(tenantLanguage))
                tenantLanguage = "tr";

            var role = GetRole(roleId, currentUser);
            CreateRole(role, databaseName, tenantLanguage);
        }

        public void UpdateRole(int roleId, string databaseName, CurrentUser currentUser, string tenantLanguage)
        {
            var role = GetRole(roleId, currentUser);

            var connection = new SqlConnection(GetConnectionString(databaseName));

            using (connection)
            {
                using (var command = connection.CreateCommand())
                {
                    var columns = new List<string>();
                    var values = new List<string>();
                    var sets = new List<string>();

                    command.Parameters.Add(new SqlParameter { ParameterName = "label", SqlValue = tenantLanguage == "tr" ? role.LabelTr : role.LabelEn, SqlDbType = SqlDbType.NVarChar });
                    command.Parameters.Add(new SqlParameter { ParameterName = "owners", SqlValue = role.Owners, SqlDbType = SqlDbType.VarChar });
                    command.Parameters.Add(new SqlParameter { ParameterName = "deleted", SqlValue = role.Deleted, SqlDbType = SqlDbType.Bit });

                    foreach (SqlParameter parameter in command.Parameters)
                    {
                        columns.Add("[" + parameter.ParameterName + "]");
                        values.Add("@" + parameter.ParameterName);
                        sets.Add("[" + parameter.ParameterName + "] = @" + parameter.ParameterName);
                    }

                    var sql = $"UPDATE [roles] \nSET \n\t{string.Join(",\n\t", sets)} \nWHERE \n\t[id] = {role.Id}";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    if (command.Connection.State != ConnectionState.Open)
                        connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }

        public void CreateProfile(Profile profile, string databaseName, string tenantLanguage)
        {
            var connection = new SqlConnection(GetConnectionString(databaseName));

            using (connection)
            {
                using (var command = connection.CreateCommand())
                {
                    var columns = new List<string>();
                    var values = new List<string>();

                    command.Parameters.Add(new SqlParameter { ParameterName = "id", SqlValue = profile.Id, SqlDbType = SqlDbType.Int });
                    command.Parameters.Add(new SqlParameter { ParameterName = "has_admin_rights", SqlValue = profile.HasAdminRights, SqlDbType = SqlDbType.Bit });
                    command.Parameters.Add(new SqlParameter { ParameterName = "parent_id", SqlValue = profile.ParentId, SqlDbType = SqlDbType.Int });
                    command.Parameters.Add(new SqlParameter { ParameterName = "order", SqlValue = profile.Order, SqlDbType = SqlDbType.Int });

                    if (!string.IsNullOrEmpty(profile.Name))
                        command.Parameters.Add(new SqlParameter { ParameterName = "name", SqlValue = profile.Name, SqlDbType = SqlDbType.NVarChar });

                    if (!string.IsNullOrEmpty(profile.Description))
                        command.Parameters.Add(new SqlParameter { ParameterName = "description", SqlValue = profile.Description, SqlDbType = SqlDbType.NVarChar });

                    if (!string.IsNullOrEmpty(profile.SystemCode))
                        command.Parameters.Add(new SqlParameter { ParameterName = "system_code", SqlValue = profile.SystemCode, SqlDbType = SqlDbType.NVarChar });

                    foreach (SqlParameter parameter in command.Parameters)
                    {
                        columns.Add("[" + parameter.ParameterName + "]");
                        values.Add("@" + parameter.ParameterName);
                    }

                    var sql = $"INSERT INTO [profiles] (\n\t{string.Join(",\n\t", columns)}\n) \nVALUES (\n\t{string.Join(",\n\t", values)}\n)";

                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    if (command.Connection.State != ConnectionState.Open)
                        connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }

        public void CreateProfile(int profileId, string databaseName, int tenantId, string tenantLanguage)
        {
            if (string.IsNullOrEmpty(tenantLanguage))
                tenantLanguage = "tr";

            var profile = GetProfile(profileId, tenantId);
            CreateProfile(profile, databaseName, tenantLanguage);
        }

        public void UpdateProfile(int profileId, string databaseName, int tenantId, string tenantLanguage)
        {
            var profile = GetProfile(profileId, tenantId);

            var connection = new SqlConnection(GetConnectionString(databaseName));

            using (connection)
            {
                using (var command = connection.CreateCommand())
                {
                    var columns = new List<string>();
                    var values = new List<string>();
                    var sets = new List<string>();

                    command.Parameters.Add(new SqlParameter { ParameterName = "has_admin_rights", SqlValue = profile.HasAdminRights, SqlDbType = SqlDbType.Bit });
                    command.Parameters.Add(new SqlParameter { ParameterName = "parent_id", SqlValue = profile.ParentId, SqlDbType = SqlDbType.Int });
                    command.Parameters.Add(new SqlParameter { ParameterName = "order", SqlValue = profile.Order, SqlDbType = SqlDbType.Int });

                    if (!string.IsNullOrEmpty(profile.Name))
                        command.Parameters.Add(new SqlParameter { ParameterName = "name", SqlValue = profile.Name, SqlDbType = SqlDbType.NVarChar });

                    if (!string.IsNullOrEmpty(profile.Description))
                        command.Parameters.Add(new SqlParameter { ParameterName = "description", SqlValue = profile.Description, SqlDbType = SqlDbType.NVarChar });

                    if (!string.IsNullOrEmpty(profile.SystemCode))
                        command.Parameters.Add(new SqlParameter { ParameterName = "system_code", SqlValue = profile.SystemCode, SqlDbType = SqlDbType.NVarChar });


                    foreach (SqlParameter parameter in command.Parameters)
                    {
                        columns.Add("[" + parameter.ParameterName + "]");
                        values.Add("@" + parameter.ParameterName);
                        sets.Add("[" + parameter.ParameterName + "] = @" + parameter.ParameterName);
                    }

                    var sql = $"UPDATE [profiles] \nSET \n\t{string.Join(",\n\t", sets)} \nWHERE \n\t[id] = {profile.Id}";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    if (command.Connection.State != ConnectionState.Open)
                        connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }

        public void CreateTenantUser(Entities.Tenant.TenantUser user, string databaseName, string tenantLanguage)
        {
            var connection = new SqlConnection(GetConnectionString(databaseName));

            using (connection)
            {
                using (var command = connection.CreateCommand())
                {
                    var columns = new List<string>();
                    var values = new List<string>();

                    command.Parameters.Add(new SqlParameter { ParameterName = "id", SqlValue = user.Id, SqlDbType = SqlDbType.Int });
                    command.Parameters.Add(new SqlParameter { ParameterName = "email", SqlValue = user.Email, SqlDbType = SqlDbType.NVarChar });
                    command.Parameters.Add(new SqlParameter { ParameterName = "first_name", SqlValue = user.FirstName, SqlDbType = SqlDbType.NVarChar });
                    command.Parameters.Add(new SqlParameter { ParameterName = "last_name", SqlValue = user.LastName, SqlDbType = SqlDbType.NVarChar });
                    command.Parameters.Add(new SqlParameter { ParameterName = "full_name", SqlValue = user.FullName, SqlDbType = SqlDbType.NVarChar });

                    if (user.Profile != null)
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "profile", SqlValue = user.Profile.Name, SqlDbType = SqlDbType.NVarChar });
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "profile", SqlValue = DBNull.Value, SqlDbType = SqlDbType.NVarChar });
                        //user.IsActive = false;
                    }

                    if (user.RoleId.HasValue)
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "role", SqlValue = tenantLanguage == "tr" ? user.Role.LabelTr : user.Role.LabelEn, SqlDbType = SqlDbType.NVarChar });
                        command.Parameters.Add(new SqlParameter { ParameterName = "role_id", SqlValue = user.RoleId.Value, SqlDbType = SqlDbType.Int });
                    }
                    else
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "role", SqlValue = DBNull.Value, SqlDbType = SqlDbType.NVarChar });
                        command.Parameters.Add(new SqlParameter { ParameterName = "role_id", SqlValue = DBNull.Value, SqlDbType = SqlDbType.Int });
                        //user.IsActive = false;
                    }

                    if (!string.IsNullOrWhiteSpace(user.Picture))
                        command.Parameters.Add(new SqlParameter { ParameterName = "picture", SqlValue = user.Picture, SqlDbType = SqlDbType.VarChar });
                    else
                        command.Parameters.Add(new SqlParameter { ParameterName = "picture", SqlValue = DBNull.Value, SqlDbType = SqlDbType.VarChar });

                    command.Parameters.Add(new SqlParameter { ParameterName = "created_by", SqlValue = user.CreatedByEmail, SqlDbType = SqlDbType.VarChar });
                    command.Parameters.Add(new SqlParameter { ParameterName = "created_at", SqlValue = user.CreatedAt, SqlDbType = SqlDbType.DateTime });
                    command.Parameters.Add(new SqlParameter { ParameterName = "is_active", SqlValue = user.IsActive, SqlDbType = SqlDbType.Bit });
                    command.Parameters.Add(new SqlParameter { ParameterName = "is_subscriber", SqlValue = user.IsSubscriber, SqlDbType = SqlDbType.Bit });
                    command.Parameters.Add(new SqlParameter { ParameterName = "deleted", SqlValue = user.Deleted, SqlDbType = SqlDbType.Bit });

                    foreach (SqlParameter parameter in command.Parameters)
                    {
                        columns.Add("[" + parameter.ParameterName + "]");
                        values.Add("@" + parameter.ParameterName);
                    }

                    var sql = $"INSERT INTO [users] (\n\t{string.Join(",\n\t", columns)}\n) \nVALUES (\n\t{string.Join(",\n\t", values)}\n)";

                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    if (command.Connection.State != ConnectionState.Open)
                        connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }

        public void CreateTenantUser(int userId, string databaseName, CurrentUser currentUser, string tenantLanguage)
        {
            if (string.IsNullOrEmpty(tenantLanguage))
                tenantLanguage = "tr";

            var user = GetTenantUser(userId, currentUser);
            CreateTenantUser(user, databaseName, tenantLanguage);
        }

        public void UpdateTenantUser(Entities.Tenant.TenantUser user, string databaseName, string tenantLanguage, bool delete = false)
        {
            var connection = new SqlConnection(GetConnectionString(databaseName));

            using (connection)
            {
                using (var command = connection.CreateCommand())
                {
                    var columns = new List<string>();
                    var values = new List<string>();
                    var sets = new List<string>();
                    var userID = user.Id;

                    command.Parameters.Add(new SqlParameter { ParameterName = "first_name", SqlValue = user.FirstName, SqlDbType = SqlDbType.NVarChar });
                    command.Parameters.Add(new SqlParameter { ParameterName = "last_name", SqlValue = user.LastName, SqlDbType = SqlDbType.NVarChar });
                    command.Parameters.Add(new SqlParameter { ParameterName = "full_name", SqlValue = user.FullName, SqlDbType = SqlDbType.NVarChar });

                    if (user.Profile != null)
                    {
                        if (user.Profile.Name != null)
                        {
                            command.Parameters.Add(new SqlParameter { ParameterName = "profile", SqlValue = user.Profile.Name, SqlDbType = SqlDbType.NVarChar });
                        }
                        else
                        {
                            command.Parameters.Add(new SqlParameter { ParameterName = "profile", SqlValue = DBNull.Value, SqlDbType = SqlDbType.NVarChar });
                            user.IsActive = false;
                        }
                    }

                    if (user.Role != null)
                    {
                        if (user.RoleId.HasValue)
                        {
                            command.Parameters.Add(new SqlParameter { ParameterName = "role", SqlValue = tenantLanguage == "tr" ? user.Role.LabelTr : user.Role.LabelEn, SqlDbType = SqlDbType.NVarChar });
                            command.Parameters.Add(new SqlParameter { ParameterName = "role_id", SqlValue = user.RoleId.Value, SqlDbType = SqlDbType.Int });
                        }
                        else
                        {
                            command.Parameters.Add(new SqlParameter { ParameterName = "role", SqlValue = DBNull.Value, SqlDbType = SqlDbType.NVarChar });
                            command.Parameters.Add(new SqlParameter { ParameterName = "role_id", SqlValue = DBNull.Value, SqlDbType = SqlDbType.Int });
                            user.IsActive = false;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(user.Picture))
                        command.Parameters.Add(new SqlParameter { ParameterName = "picture", SqlValue = user.Picture, SqlDbType = SqlDbType.VarChar });
                    else
                        command.Parameters.Add(new SqlParameter { ParameterName = "picture", SqlValue = DBNull.Value, SqlDbType = SqlDbType.VarChar });

                    command.Parameters.Add(new SqlParameter { ParameterName = "created_by", SqlValue = user.CreatedByEmail, SqlDbType = SqlDbType.VarChar });
                    command.Parameters.Add(new SqlParameter { ParameterName = "created_at", SqlValue = user.CreatedAt, SqlDbType = SqlDbType.DateTime });
                    command.Parameters.Add(new SqlParameter { ParameterName = "is_active", SqlValue = user.IsActive, SqlDbType = SqlDbType.Bit });
                    command.Parameters.Add(new SqlParameter { ParameterName = "is_subscriber", SqlValue = user.IsSubscriber, SqlDbType = SqlDbType.Bit });
                    command.Parameters.Add(new SqlParameter { ParameterName = "deleted", SqlValue = user.Deleted, SqlDbType = SqlDbType.Bit });

                    foreach (SqlParameter parameter in command.Parameters)
                    {
                        columns.Add("[" + parameter.ParameterName + "]");
                        values.Add("@" + parameter.ParameterName);
                        sets.Add("[" + parameter.ParameterName + "] = @" + parameter.ParameterName);
                    }

                    //var sql = $"UPDATE [users] (\n\t{string.Join(",\n\t", columns)}\n) \nVALUES (\n\t{string.Join(",\n\t", values)}\n)";
                    var sql = $"UPDATE [users] \nSET \n\t{string.Join(",\n\t", sets)} \nWHERE \n\t[id] = {userID}";
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    if (command.Connection.State != ConnectionState.Open)
                        connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateTenantUser(int userId, string databaseName, CurrentUser currentUser)
        {
            var user = GetTenantUser(userId, currentUser);
            UpdateTenantUser(user, databaseName, user.Culture.Contains("tr") ? "tr" : "en", false);
        }

        public void CreateRecord(JObject record, string databaseName, Module module)
        {
            if (record["id"].IsNullOrEmpty() || record["created_by"].IsNullOrEmpty() || record["created_at"].IsNullOrEmpty())
                return;

            var connection = new SqlConnection(GetConnectionString(databaseName));

            using (connection)
            {
                using (var command = connection.CreateCommand())
                {
                    var columns = new List<string>();
                    var values = new List<string>();

                    AddCommandParameters(command, record, module);

                    foreach (SqlParameter parameter in command.Parameters)
                    {
                        columns.Add("[" + parameter.ParameterName + "]");
                        values.Add("@" + parameter.ParameterName);
                    }

                    AddCommandStandardParametersCreate(command, record, module, columns, values);

                    var sql = $"INSERT INTO [{module.Name}_d] (\n\t{string.Join(",\n\t", columns)}\n) \nVALUES (\n\t{string.Join(",\n\t", values)}\n)";

                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    if (command.Connection.State != ConnectionState.Open)
                        connection.Open();

                    command.ExecuteNonQuery();
                }

                foreach (var field in module.Fields)
                {
                    if (field.DataType == Enums.DataType.Multiselect && !record[field.Name].IsNullOrEmpty())
                    {
                        using (var command = connection.CreateCommand())
                        {
                            var values = new List<string>();
                            var picklistItems = record[field.Name].ToString().Split('|');
                            command.Parameters.Add(new SqlParameter { ParameterName = module.Name + "_id", SqlValue = (int)record["id"], SqlDbType = SqlDbType.Int });

                            for (int i = 0; i < picklistItems.Length; i++)
                            {
                                command.Parameters.Add(new SqlParameter { ParameterName = "value" + i, SqlValue = picklistItems[i], SqlDbType = SqlDbType.NVarChar });
                                values.Add($"(@{module.Name + "_id"}, @value{i})");
                            }

                            var sql = $"INSERT INTO [{module.Name + "_" + field.Name}_d] ([{module.Name}_id], [value]) \nVALUES {string.Join(",\n\t", values)}";

                            command.CommandText = sql;
                            command.CommandType = CommandType.Text;

                            if (command.Connection.State != ConnectionState.Open)
                                connection.Open();

                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public void CreateRecord(JObject record, string databaseName, string moduleName, CurrentUser currentUser)
        {
            var module = GetModule(moduleName, currentUser);
            CreateRecord(record, databaseName, module);
        }

        public void UpdateRecord(JObject record, string databaseName, Module module, bool delete = false)
        {
            var connection = new SqlConnection(GetConnectionString(databaseName));

            using (connection)
            {
                using (var command = connection.CreateCommand())
                {
                    var sets = new List<string>();
                    var recordId = (int)record["id"];

                    AddCommandParameters(command, record, module);

                    foreach (SqlParameter parameter in command.Parameters)
                    {
                        sets.Add("[" + parameter.ParameterName + "] = @" + parameter.ParameterName);
                    }

                    AddCommandStandardParametersUpdate(command, record, module, sets, delete);

                    var sql = $"UPDATE [{module.Name}_d] \nSET \n\t{string.Join(",\n\t", sets)} \nWHERE \n\t[id] = {recordId}";

                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    if (command.Connection.State != ConnectionState.Open)
                        connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateRecord(JObject record, string databaseName, string moduleName, CurrentUser currentUser, bool delete = false)
        {
            var module = GetModule(moduleName, currentUser);
            UpdateRecord(record, databaseName, module, delete);
        }

        public void AddRelations(JArray records, string moduleName, string relatedModuleName, string databaseName, int relationId = 0)
        {
            var connection = new SqlConnection(GetConnectionString(databaseName));

            using (connection)
            {
                using (var command = connection.CreateCommand())
                {
                    var columns = new List<string>();
                    var values = new List<string>();
                    var columnName1 = moduleName + "_id";
                    var columnName2 = relatedModuleName + "_id";

                    columns.Add($"[{columnName1}]");
                    columns.Add($"[{columnName2}]");

                    for (int i = 0; i < records.Count; i++)
                    {
                        var record = records[i];
                        command.Parameters.Add(new SqlParameter { ParameterName = columnName1 + i, SqlValue = record[columnName1], SqlDbType = SqlDbType.Int });
                        command.Parameters.Add(new SqlParameter { ParameterName = columnName2 + i, SqlValue = record[columnName2], SqlDbType = SqlDbType.Int });

                        values.Add($"(@{columnName1 + i}, @{columnName2 + i})");
                    }

                    var tableName = moduleName + "_" + relatedModuleName;

                    if (relationId > 0)
                        tableName += "_" + relationId;

                    var sql = $"INSERT INTO [{tableName}_d] (\n\t{string.Join(",\n\t", columns)}\n) \nVALUES {string.Join(",\n\t", values)}";
                    command.CommandText = sql;

                    if (command.Connection.State != ConnectionState.Open)
                        connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteRelation(JObject record, string moduleName, string relatedModuleName, string databaseName, int relationId = 0)
        {
            var connection = new SqlConnection(GetConnectionString(databaseName));

            using (connection)
            {
                using (var command = connection.CreateCommand())
                {
                    var columnName1 = moduleName + "_id";
                    var columnName2 = relatedModuleName + "_id";

                    command.Parameters.Add(new SqlParameter { ParameterName = columnName1, SqlValue = record[columnName1], SqlDbType = SqlDbType.Int });
                    command.Parameters.Add(new SqlParameter { ParameterName = columnName2, SqlValue = record[columnName2], SqlDbType = SqlDbType.Int });

                    var tableName = moduleName + "_" + relatedModuleName;

                    if (relationId > 0)
                        tableName += "_" + relationId;

                    var whereSql = $"{columnName1} = @{columnName1} AND {columnName2} = @{columnName2}";
                    var sql = $"DELETE FROM [{tableName}_d] \nWHERE {whereSql}";
                    command.CommandText = sql;

                    if (command.Connection.State != ConnectionState.Open)
                        connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }

        public void CreateRecordBulk(JArray records, string databaseName, Module module, int currentUserId)
        {
            var connection = new SqlConnection(GetConnectionString(databaseName));

            using (connection)
            {
                using (var command = connection.CreateCommand())
                {
                    var fields = GetFieldsDictionary(module);
                    var columns = new List<string>();
                    var values = new List<string>();

                    foreach (var field in fields)
                    {
                        columns.Add("[" + field.Value + "]");
                    }

                    foreach (JObject record in records)
                    {
                        var recordValues = new List<string>();

                        foreach (var field in fields)
                        {
                            if (!record[field.Value].IsNullOrEmpty())
                                recordValues.Add("'" + record[field.Value].ToString().Trim().Replace("'", "''") + "'");
                            else
                                recordValues.Add("NULL");
                        }

                        values.Add(string.Join(", ", recordValues));
                    }

                    var sql = "";

                    foreach (var value in values)
                    {
                        sql += $"INSERT INTO [{module.Name}_d] \n({string.Join(",", columns)}) \nVALUES ({value})";
                    }

                    //var sql = $"INSERT INTO [{module.Name}_d] \n({string.Join(",", columns)}) \nVALUES ({string.Join("),(", values)})";


                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    command.CommandTimeout = 600;//10 minutes

                    if (command.Connection.State != ConnectionState.Open)
                        connection.Open();

                    command.ExecuteNonQuery();
                }
            }
        }

        public void CreateRecordBulk(int importId, string databaseName, string moduleName, CurrentUser currentUser)
        {
            var module = GetModule(moduleName, currentUser);
            var sqlRecords = $"SELECT * FROM {moduleName}_d WHERE \"import_id\" = '{importId}'";
            _analyticRepository.CurrentUser = currentUser;
            _analyticRepository.TenantId = currentUser.TenantId;
            var records = _analyticRepository.DbContext.Database.SqlQueryDynamic(sqlRecords);

            CreateRecordBulk(records, databaseName, module, currentUser.UserId);
        }

        public void ImportRevert(int importId, string databaseName, string moduleName, CurrentUser currentUser)
        {
            var sql = @"UPDATE {moduleName}_d SET deleted = true WHERE import_id = @importId";

            _analyticRepository.CurrentUser = currentUser;
            _analyticRepository.TenantId = currentUser.TenantId;
            _analyticRepository.DbContext.Database.ExecuteSqlCommand(sql, new SqlParameter("@importId", importId));
        }

        private void CreateColumn(Database database, Table table, Module module, Field field)
        {
            var column = new Column(table, field.Name, GetDataType(field));
            column.Nullable = true;
            table.Columns.Add(column);

            //Create nonclustered index for proper fields
            if (field.DataType != Enums.DataType.TextMulti)
            {
                var ix = new Microsoft.SqlServer.Management.Smo.Index(table, "IX_" + table.Name + "_" + field.Name);
                table.Indexes.Add(ix);
                ix.IndexedColumns.Add(new IndexedColumn(ix, field.Name, true));
                ix.IsClustered = false;
                ix.IsUnique = false;
                ix.IndexKeyType = IndexKeyType.None;
            }

            //Create multiselect tables
            if (field.DataType == Enums.DataType.Multiselect)
            {
                var multiselectTable = new Table(database, module.Name + "_" + field.Name + "_d");

                //Create multiselect table columns
                var multiselectIdColumn = new Column(multiselectTable, module.Name + "_id", DataType.Int);
                multiselectIdColumn.Nullable = false;
                multiselectTable.Columns.Add(multiselectIdColumn);

                var multiselectValueColumn = new Column(multiselectTable, "value", DataType.NVarChar(200));
                multiselectValueColumn.Nullable = true;
                multiselectTable.Columns.Add(multiselectValueColumn);

                //Create nonclustered index for multiselect table fields
                var multiselectIdIndex = new Microsoft.SqlServer.Management.Smo.Index(multiselectTable, "IX_" + multiselectTable.Name + "_id");
                multiselectTable.Indexes.Add(multiselectIdIndex);
                multiselectIdIndex.IndexedColumns.Add(new IndexedColumn(multiselectIdIndex, module.Name + "_id", true));
                multiselectIdIndex.IsClustered = false;
                multiselectIdIndex.IsUnique = false;
                multiselectIdIndex.IndexKeyType = IndexKeyType.None;

                var multiselectValueIndex = new Microsoft.SqlServer.Management.Smo.Index(multiselectTable, "IX_" + multiselectTable.Name + "_value");
                multiselectTable.Indexes.Add(multiselectValueIndex);
                multiselectValueIndex.IndexedColumns.Add(new IndexedColumn(multiselectValueIndex, "value", true));
                multiselectValueIndex.IsClustered = false;
                multiselectValueIndex.IsUnique = false;
                multiselectValueIndex.IndexKeyType = IndexKeyType.None;

                multiselectTable.Create();
            }
        }

        private void CreateJunctionTable(Database database, Module module, Relation relation, List<string> junctionTableNames)
        {
            var relationFieldName1 = module.Name;
            var relationFieldName2 = relation.RelatedModule;

            if (relationFieldName1 == relationFieldName2)
            {
                relationFieldName1 = relationFieldName1 + "1";
                relationFieldName2 = relationFieldName2 + "2";
            }

            var junctionTableName = module.Name + "_" + relation.RelatedModule;

            if (junctionTableNames.Contains(junctionTableName))
                junctionTableName = junctionTableName + "_" + relation.Id;
            else
                junctionTableNames.Add(junctionTableName);

            var junctionTable = new Table(database, junctionTableName + "_d");

            //Create junction table columns
            var firstColumn = new Column(junctionTable, relationFieldName1 + "_id", DataType.Int);
            firstColumn.Nullable = false;
            junctionTable.Columns.Add(firstColumn);

            var secondColumn = new Column(junctionTable, relationFieldName2 + "_id", DataType.Int);
            secondColumn.Nullable = false;
            junctionTable.Columns.Add(secondColumn);

            //Create nonclustered index for junction table fields
            var firstColumnIndex = new Microsoft.SqlServer.Management.Smo.Index(junctionTable, "IX_" + junctionTable.Name + "_" + relationFieldName1 + "_id");
            junctionTable.Indexes.Add(firstColumnIndex);
            firstColumnIndex.IndexedColumns.Add(new IndexedColumn(firstColumnIndex, relationFieldName1 + "_id", true));
            firstColumnIndex.IsClustered = false;
            firstColumnIndex.IsUnique = false;
            firstColumnIndex.IndexKeyType = IndexKeyType.None;

            var secondColumnIndex = new Microsoft.SqlServer.Management.Smo.Index(junctionTable, "IX_" + junctionTable.Name + "_" + relationFieldName2 + "_id");
            junctionTable.Indexes.Add(secondColumnIndex);
            secondColumnIndex.IndexedColumns.Add(new IndexedColumn(secondColumnIndex, relationFieldName2 + "_id", true));
            secondColumnIndex.IsClustered = false;
            secondColumnIndex.IsUnique = false;
            secondColumnIndex.IndexKeyType = IndexKeyType.None;

            junctionTable.Create();
        }

        private void CreateRolesTable(Database database)
        {
            var tableRoles = new Table(database, "roles");

            var columnId = new Column(tableRoles, "id", DataType.Int);
            columnId.Nullable = false;
            tableRoles.Columns.Add(columnId);

            var columnLabel = new Column(tableRoles, "label", DataType.NVarChar(200));
            columnLabel.Nullable = false;
            tableRoles.Columns.Add(columnLabel);

            var columnOwners = new Column(tableRoles, "owners", DataType.VarCharMax);
            columnOwners.Nullable = false;
            tableRoles.Columns.Add(columnOwners);

            var columnDeleted = new Column(tableRoles, "deleted", DataType.Bit);
            columnDeleted.Nullable = false;
            tableRoles.Columns.Add(columnDeleted);

            //Primary key index for user table
            var pkRole = new Microsoft.SqlServer.Management.Smo.Index(tableRoles, "PK_roles");
            tableRoles.Indexes.Add(pkRole);
            pkRole.IndexedColumns.Add(new IndexedColumn(pkRole, columnId.Name));
            pkRole.IsClustered = true;
            pkRole.IsUnique = true;
            pkRole.IndexKeyType = IndexKeyType.DriPrimaryKey;

            tableRoles.Create();
        }

        private void CreateProfilesTables(Database database)
        {
            var tableProfiles = new Table(database, "profiles");

            var columnId = new Column(tableProfiles, "id", DataType.Int);
            columnId.Nullable = false;
            tableProfiles.Columns.Add(columnId);

            var columnName = new Column(tableProfiles, "name", DataType.NVarChar(200));
            columnName.Nullable = false;
            tableProfiles.Columns.Add(columnName);

            var columnDescription = new Column(tableProfiles, "description", DataType.NVarCharMax);
            columnDescription.Nullable = true;
            tableProfiles.Columns.Add(columnDescription);

            var columnHasAdminRights = new Column(tableProfiles, "has_admin_rights", DataType.Bit);
            columnHasAdminRights.Nullable = false;
            tableProfiles.Columns.Add(columnHasAdminRights);

            var columnParentId = new Column(tableProfiles, "parent_id", DataType.Int);
            columnParentId.Nullable = false;
            tableProfiles.Columns.Add(columnParentId);

            var columnOrder = new Column(tableProfiles, "order", DataType.Int);
            columnOrder.Nullable = false;
            tableProfiles.Columns.Add(columnOrder);

            var columnSystemCode = new Column(tableProfiles, "system_code", DataType.NVarCharMax);
            columnSystemCode.Nullable = true;
            tableProfiles.Columns.Add(columnSystemCode);

            ////PK create
            var pkProfile = new Microsoft.SqlServer.Management.Smo.Index(tableProfiles, "PK_profiles");
            tableProfiles.Indexes.Add(pkProfile);
            pkProfile.IndexedColumns.Add(new IndexedColumn(pkProfile, columnName.Name));
            pkProfile.IsClustered = true;
            pkProfile.IsUnique = true;
            pkProfile.IndexKeyType = IndexKeyType.DriPrimaryKey;

            tableProfiles.Create();
        }

        private void CreateUsersTable(Database database)
        {
            var tableUsers = new Table(database, "users");

            var columnId = new Column(tableUsers, "id", DataType.Int);
            columnId.Nullable = false;
            tableUsers.Columns.Add(columnId);

            var columnEmail = new Column(tableUsers, "email", DataType.NVarChar(200));
            columnEmail.Nullable = false;
            tableUsers.Columns.Add(columnEmail);

            var columnFirstName = new Column(tableUsers, "first_name", DataType.NVarChar(40));
            columnFirstName.Nullable = false;
            tableUsers.Columns.Add(columnFirstName);

            var columnLastName = new Column(tableUsers, "last_name", DataType.NVarChar(40));
            columnLastName.Nullable = false;
            tableUsers.Columns.Add(columnLastName);

            var columnFullName = new Column(tableUsers, "full_name", DataType.NVarChar(80));
            columnFullName.Nullable = false;
            tableUsers.Columns.Add(columnFullName);

            var columnProfile = new Column(tableUsers, "profile", DataType.NVarChar(200));
            columnProfile.Nullable = true;
            tableUsers.Columns.Add(columnProfile);

            var columnRole = new Column(tableUsers, "role", DataType.NVarChar(200));
            columnRole.Nullable = true;
            tableUsers.Columns.Add(columnRole);

            var columnRoleId = new Column(tableUsers, "role_id", DataType.Int);
            columnRoleId.Nullable = true;
            tableUsers.Columns.Add(columnRoleId);

            var columnProfilePicture = new Column(tableUsers, "picture", DataType.VarChar(400));
            columnProfilePicture.Nullable = true;
            tableUsers.Columns.Add(columnProfilePicture);

            var columnCreatedBy = new Column(tableUsers, "created_by", DataType.VarChar(100));
            columnCreatedBy.Nullable = false;
            tableUsers.Columns.Add(columnCreatedBy);

            var columnCreatedAt = new Column(tableUsers, "created_at", DataType.DateTime);
            columnCreatedAt.Nullable = false;
            tableUsers.Columns.Add(columnCreatedAt);

            var columnIsActive = new Column(tableUsers, "is_active", DataType.Bit);
            columnIsActive.Nullable = false;
            tableUsers.Columns.Add(columnIsActive);

            var columnIsSubscriber = new Column(tableUsers, "is_subscriber", DataType.Bit);
            columnIsSubscriber.Nullable = false;
            tableUsers.Columns.Add(columnIsSubscriber);

            var columnDeleted = new Column(tableUsers, "deleted", DataType.Bit);
            columnDeleted.Nullable = false;
            tableUsers.Columns.Add(columnDeleted);

            //Primary key index for user table
            var pkUser = new Microsoft.SqlServer.Management.Smo.Index(tableUsers, "PK_users");
            tableUsers.Indexes.Add(pkUser);
            pkUser.IndexedColumns.Add(new IndexedColumn(pkUser, columnId.Name));
            pkUser.IsClustered = true;
            pkUser.IsUnique = true;
            pkUser.IndexKeyType = IndexKeyType.DriPrimaryKey;

            tableUsers.Create();
        }

        private void CreateView(Database database, Module module, string tenantLanguage)
        {
            var viewName = module.Name;
            var view = new View(database, viewName, "dbo");
            var columnsView = GetViewColumns(module, tenantLanguage);

            view.TextHeader = $"CREATE VIEW [dbo].[{viewName}] AS";
            view.TextBody = $"SELECT \n{string.Join(",\n", columnsView)} \nFROM [dbo].[{module.Name + "_d"}] \nWHERE deleted = 0";
            view.Create();
        }

        private void AlterView(Database database, Module module, string tenantLanguage, bool createIfNotExist = false)
        {
            var viewName = module.Name;
            var view = database.Views[viewName];

            if (createIfNotExist && view == null)
            {
                CreateView(database, module, tenantLanguage);
                return;
            }

            var columnsView = GetViewColumns(module, tenantLanguage);

            view.TextHeader = $"ALTER VIEW [dbo].[{viewName}] AS";
            view.TextBody = $"SELECT \n{string.Join(",\n", columnsView)} \nFROM [dbo].[{module.Name + "_d"}] \nWHERE deleted = 0";
            view.Alter();
        }

        private List<string> GetViewColumns(Module module, string tenantLanguage)
        {
            var columnsView = new List<string>();
            var labelsView = new List<string>();
            var fields = module.Fields.OrderBy(x => x.Order);

            columnsView.Add("[id] AS [Id]");
            labelsView.Add("Id");

            if (module.Name == "opportunities")
            {
                var labelForecastType = tenantLanguage == "tr" ? "Öngörü Tipi" : "Forecast Type";
                var labelForecastCategory = tenantLanguage == "tr" ? "Öngörü Kategorisi" : "Forecast Category";
                var labelForecastYear = tenantLanguage == "tr" ? "Öngörü Yılı" : "Forecast Year";
                var labelForecastMonth = tenantLanguage == "tr" ? "Öngörü Ayı" : "Forecast Month";
                var labelForecastQuarter = tenantLanguage == "tr" ? "Öngörü Çeyreği" : "Forecast Quarter";

                columnsView.Add($"[forecast_type] AS [{labelForecastType}]");
                labelsView.Add(labelForecastType);

                columnsView.Add($"[forecast_category] AS [{labelForecastCategory}]");
                labelsView.Add(labelForecastCategory);

                columnsView.Add($"[forecast_year] AS [{labelForecastYear}]");
                labelsView.Add(labelForecastYear);

                columnsView.Add($"[forecast_month] AS [{labelForecastMonth}]");
                labelsView.Add(labelForecastMonth);

                columnsView.Add($"[forecast_quarter] AS [{labelForecastQuarter}]");
                labelsView.Add(labelForecastQuarter);
            }

            foreach (var field in fields)
            {
                if (ModuleHelper.StandardFields.Contains(field.Name))
                    AddViewColumn(columnsView, labelsView, field, tenantLanguage);

                if (ModuleHelper.SystemFieldsExtended.Contains(field.Name))
                    continue;

                AddViewColumn(columnsView, labelsView, field, tenantLanguage);
            }

            return columnsView;
        }

        private void AddViewColumn(List<string> columnsView, List<string> labelsView, Field field, string language)
        {
            var columnViewTemplate = "[{0}] AS [{1}]";
            var label = language == "tr" ? field.LabelTr : field.LabelEn;

            if (labelsView.Contains(label) || ModuleHelper.SystemFieldsExtended.Contains(label.ToLower()))
                label += " " + field.Name;

            labelsView.Add(label);
            var columnViewSql = string.Format(columnViewTemplate, field.Name, label);
            columnsView.Add(columnViewSql);
        }

        private void AddCommandParameters(SqlCommand command, JObject record, Module module)
        {
            foreach (var property in record)
            {
                if (ModuleHelper.SystemFieldsExtended.Contains(property.Key))
                    continue;

                if (ModuleHelper.ModuleSpecificFields(module).Contains(property.Key))
                    continue;

                var key = property.Key;
                var value = property.Value.ToString();
                var field = module.Fields.FirstOrDefault(x => x.Name == key);

                if (field == null)
                    continue;

                if (record["id"].IsNullOrEmpty() && string.IsNullOrWhiteSpace(value))
                    continue;

                switch (field.DataType)
                {
                    case Enums.DataType.TextSingle:
                    case Enums.DataType.TextMulti:
                    case Enums.DataType.Email:
                    case Enums.DataType.Picklist:
                    case Enums.DataType.Multiselect:
                    case Enums.DataType.Url:
                    case Enums.DataType.Image:
                    case Enums.DataType.Location:
                    case Enums.DataType.Document:
                        if (!string.IsNullOrWhiteSpace(value))
                            command.Parameters.Add(new SqlParameter { ParameterName = key, SqlValue = value, SqlDbType = SqlDbType.NVarChar });
                        else
                            command.Parameters.Add(new SqlParameter { ParameterName = key, SqlValue = DBNull.Value, SqlDbType = SqlDbType.NVarChar });
                        break;
                    case Enums.DataType.Number:
                    case Enums.DataType.NumberDecimal:
                    case Enums.DataType.Rating:
                        if (!string.IsNullOrWhiteSpace(value))
                            command.Parameters.Add(new SqlParameter { ParameterName = key, SqlValue = decimal.Parse(value), SqlDbType = SqlDbType.Float });
                        else
                            command.Parameters.Add(new SqlParameter { ParameterName = key, SqlValue = DBNull.Value, SqlDbType = SqlDbType.Float });
                        break;
                    case Enums.DataType.NumberAuto:
                        if (!string.IsNullOrWhiteSpace(value))
                            command.Parameters.Add(new SqlParameter { ParameterName = key, SqlValue = decimal.Parse(value), SqlDbType = SqlDbType.Int });
                        else
                            command.Parameters.Add(new SqlParameter { ParameterName = key, SqlValue = DBNull.Value, SqlDbType = SqlDbType.Int });
                        break;
                    case Enums.DataType.Currency:
                        if (!string.IsNullOrWhiteSpace(value))
                            command.Parameters.Add(new SqlParameter { ParameterName = key, SqlValue = decimal.Parse(value), SqlDbType = SqlDbType.Money });
                        else
                            command.Parameters.Add(new SqlParameter { ParameterName = key, SqlValue = DBNull.Value, SqlDbType = SqlDbType.Money });
                        break;
                    case Enums.DataType.Date:
                    case Enums.DataType.DateTime:
                    case Enums.DataType.Time:
                        DateTime sqlValue;
                        var parsed = DateTime.TryParse(value, out sqlValue);

                        if (!string.IsNullOrWhiteSpace(value) && parsed && !(sqlValue == DateTime.MinValue || sqlValue == DateTime.MaxValue || sqlValue < new DateTime(1753, 1, 1)))
                            command.Parameters.Add(new SqlParameter { ParameterName = key, SqlValue = sqlValue.ToUniversalTime(), SqlDbType = SqlDbType.DateTime });
                        else
                            command.Parameters.Add(new SqlParameter { ParameterName = key, SqlValue = DBNull.Value, SqlDbType = SqlDbType.DateTime });
                        break;
                    case Enums.DataType.Lookup:
                        if (!string.IsNullOrWhiteSpace(value))
                            command.Parameters.Add(new SqlParameter { ParameterName = key, SqlValue = int.Parse(value), SqlDbType = SqlDbType.Int });
                        else
                            command.Parameters.Add(new SqlParameter { ParameterName = key, SqlValue = DBNull.Value, SqlDbType = SqlDbType.Int });
                        break;
                    case Enums.DataType.Checkbox:
                        if (!string.IsNullOrWhiteSpace(value))
                            command.Parameters.Add(new SqlParameter { ParameterName = key, SqlValue = bool.Parse(value), SqlDbType = SqlDbType.Bit });
                        else
                            command.Parameters.Add(new SqlParameter { ParameterName = key, SqlValue = DBNull.Value, SqlDbType = SqlDbType.Bit });
                        break;
                }
            }
        }

        private void AddCommandStandardParametersCreate(SqlCommand command, JObject record, Module module, List<string> columns, List<string> values)
        {
            if ((DateTime)record["created_at"] < new DateTime(1753, 1, 1))
                record["created_at"] = new DateTime(1753, 1, 1);

            command.Parameters.Add(new SqlParameter { ParameterName = "id", SqlValue = (int)record["id"], SqlDbType = SqlDbType.Int });
            command.Parameters.Add(new SqlParameter { ParameterName = "created_by", SqlValue = (int)record["created_by"], SqlDbType = SqlDbType.Int });
            command.Parameters.Add(new SqlParameter { ParameterName = "created_at", SqlValue = (DateTime)record["created_at"], SqlDbType = SqlDbType.DateTime });
            command.Parameters.Add(new SqlParameter { ParameterName = "deleted", SqlValue = (bool)record["deleted"], SqlDbType = SqlDbType.Bit });
            command.Parameters.Add(new SqlParameter { ParameterName = "is_converted", SqlValue = false, SqlDbType = SqlDbType.Bit });
            columns.Add("[id]");
            columns.Add("[created_by]");
            columns.Add("[created_at]");
            columns.Add("[is_converted]");
            columns.Add("[deleted]");
            values.Add("@id");
            values.Add("@created_by");
            values.Add("@created_at");
            values.Add("@deleted");
            values.Add("@is_converted");

            if (!record["updated_by"].IsNullOrEmpty())
            {
                command.Parameters.Add(new SqlParameter { ParameterName = "updated_by", SqlValue = (int)record["updated_by"], SqlDbType = SqlDbType.Int });
                columns.Add("[updated_by]");
                values.Add("@updated_by");
            }

            if (!record["updated_at"].IsNullOrEmpty())
            {
                command.Parameters.Add(new SqlParameter { ParameterName = "updated_at", SqlValue = (DateTime)record["updated_at"], SqlDbType = SqlDbType.DateTime });
                columns.Add("[updated_at]");
                values.Add("@updated_at");
            }

            if (!record["master_id"].IsNullOrEmpty())
            {
                command.Parameters.Add(new SqlParameter { ParameterName = "master_id", SqlValue = (int)record["master_id"], SqlDbType = SqlDbType.Int });
                columns.Add("[master_id]");
                values.Add("@master_id");
            }

            if (!record["import_id"].IsNullOrEmpty())
            {
                command.Parameters.Add(new SqlParameter { ParameterName = "import_id", SqlValue = (int)record["import_id"], SqlDbType = SqlDbType.Int });
                columns.Add("[import_id]");
                values.Add("@import_id");
            }

            //Module specific fields
            switch (module.Name)
            {
                case "activities":
                    command.Parameters.Add(new SqlParameter { ParameterName = "activity_type_system", SqlValue = (string)record["activity_type_system"], SqlDbType = SqlDbType.VarChar });
                    columns.Add("[activity_type_system]");
                    values.Add("@activity_type_system");
                    break;
                case "opportunities":
                    if (!record["forecast_type"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "forecast_type", SqlValue = (string)record["forecast_type"], SqlDbType = SqlDbType.NVarChar });
                        columns.Add("[forecast_type]");
                        values.Add("@forecast_type");
                    }

                    if (!record["forecast_category"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "forecast_category", SqlValue = (string)record["forecast_category"], SqlDbType = SqlDbType.NVarChar });
                        columns.Add("[forecast_category]");
                        values.Add("@forecast_category");
                    }

                    if (!record["forecast_year"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "forecast_year", SqlValue = (int)record["forecast_year"], SqlDbType = SqlDbType.Int });
                        columns.Add("[forecast_year]");
                        values.Add("@forecast_year");
                    }

                    if (!record["forecast_month"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "forecast_month", SqlValue = (int)record["forecast_month"], SqlDbType = SqlDbType.Int });
                        columns.Add("[forecast_month]");
                        values.Add("@forecast_month");
                    }

                    if (!record["forecast_quarter"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "forecast_quarter", SqlValue = (int)record["forecast_quarter"], SqlDbType = SqlDbType.Int });
                        columns.Add("[forecast_quarter]");
                        values.Add("@forecast_quarter");
                    }

                    break;
                case "current_accounts":
                    command.Parameters.Add(new SqlParameter { ParameterName = "transaction_type_system", SqlValue = (string)record["transaction_type_system"], SqlDbType = SqlDbType.VarChar });
                    columns.Add("[transaction_type_system]");
                    values.Add("@transaction_type_system");
                    break;
            }
        }

        public static void AddCommandStandardParametersUpdate(SqlCommand command, JObject record, Module module, List<string> sets, bool delete)
        {
            command.Parameters.Add(new SqlParameter { ParameterName = "updated_by", SqlValue = (int)record["updated_by"], SqlDbType = SqlDbType.Int });
            command.Parameters.Add(new SqlParameter { ParameterName = "updated_at", SqlValue = (DateTime)record["updated_at"], SqlDbType = SqlDbType.DateTime });

            sets.Add("[updated_by] = @updated_by");
            sets.Add("[updated_at] = @updated_at");

            if (delete)
            {
                command.Parameters.Add(new SqlParameter { ParameterName = "deleted", SqlValue = true, SqlDbType = SqlDbType.Bit });
                sets.Add("[deleted] = @deleted");
            }

            if (!record["is_converted"].IsNullOrEmpty())
            {
                command.Parameters.Add(new SqlParameter { ParameterName = "is_converted", SqlValue = (int)record["is_converted"], SqlDbType = SqlDbType.Bit });
                sets.Add("[is_converted] = @is_converted");
            }

            if (record["master_id"] != null)
            {
                if (!string.IsNullOrWhiteSpace(record["master_id"].ToString()))
                    command.Parameters.Add(new SqlParameter { ParameterName = "master_id", SqlValue = (int)record["master_id"], SqlDbType = SqlDbType.Int });
                else
                    command.Parameters.Add(new SqlParameter { ParameterName = "master_id", SqlValue = DBNull.Value, SqlDbType = SqlDbType.Int });

                sets.Add("[master_id] = @master_id");
            }

            //Module specific fields
            switch (module.Name)
            {
                case "activities":
                    if (!record["activity_type_system"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "activity_type_system", SqlValue = (string)record["activity_type_system"], SqlDbType = SqlDbType.VarChar });
                        sets.Add("[activity_type_system] = @activity_type_system");
                    }

                    break;
                case "opportunities":
                    if (!record["forecast_type"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "forecast_type", SqlValue = (string)record["forecast_type"], SqlDbType = SqlDbType.NVarChar });
                        sets.Add("[forecast_type] = @forecast_type");
                    }

                    if (!record["forecast_category"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "forecast_category", SqlValue = (string)record["forecast_category"], SqlDbType = SqlDbType.NVarChar });
                        sets.Add("[forecast_category] = @forecast_category");
                    }

                    if (!record["forecast_year"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "forecast_year", SqlValue = (int)record["forecast_year"], SqlDbType = SqlDbType.Int });
                        sets.Add("[forecast_year] = @forecast_year");
                    }

                    if (!record["forecast_month"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "forecast_month", SqlValue = (int)record["forecast_month"], SqlDbType = SqlDbType.Int });
                        sets.Add("[forecast_month] = @forecast_month");
                    }

                    if (!record["forecast_quarter"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "forecast_quarter", SqlValue = (int)record["forecast_quarter"], SqlDbType = SqlDbType.Int });
                        sets.Add("[forecast_quarter] = @forecast_quarter");
                    }

                    break;
                case "current_accounts":
                    if (!record["transaction_type_system"].IsNullOrEmpty())
                    {
                        command.Parameters.Add(new SqlParameter { ParameterName = "transaction_type_system", SqlValue = (string)record["transaction_type_system"], SqlDbType = SqlDbType.VarChar });
                        sets.Add("[transaction_type_system] = @transaction_type_system");
                    }

                    break;
            }
        }

        private DataType GetDataType(Field field)
        {
            switch (field.DataType)
            {
                case Enums.DataType.TextSingle:
                    return DataType.NVarChar(400);
                case Enums.DataType.Email:
                    return DataType.NVarChar(100);
                case Enums.DataType.TextMulti:
                    if (field.MultilineType == MultilineType.Large)
                        return DataType.NVarCharMax;

                    return DataType.NVarChar(2000);
                case Enums.DataType.Number:
                case Enums.DataType.NumberDecimal:
                    return DataType.Float;
                case Enums.DataType.NumberAuto:
                    return DataType.Int;
                case Enums.DataType.Currency:
                    return DataType.Money;
                case Enums.DataType.Date:
                case Enums.DataType.DateTime:
                case Enums.DataType.Time:
                    return DataType.DateTime;
                case Enums.DataType.Picklist:
                    return DataType.NVarChar(101);
                case Enums.DataType.Multiselect:
                    return DataType.NVarChar(4000);
                case Enums.DataType.Lookup:
                    return DataType.Int;
                case Enums.DataType.Checkbox:
                    return DataType.Bit;
                default:
                    return DataType.NVarChar(200);
            }
        }

        private Role GetRole(int roleId, CurrentUser currentUser)
        {
            _analyticRepository.CurrentUser = currentUser;
            _analyticRepository.TenantId = currentUser.TenantId;

            var role = _analyticRepository.DbContext.Roles
                .Single(x => x.Id == roleId);

            return role;
        }

        private Profile GetProfile(int profileId, int tenantId)
        {
            _analyticRepository.TenantId = tenantId;

            var profile = _analyticRepository.DbContext.Profiles.Single(x => x.Id == profileId && !x.Deleted);

            return profile;
        }

        private TenantUser GetTenantUser(int userId, CurrentUser currentUser)
        {
            _analyticRepository.CurrentUser = currentUser;
            _analyticRepository.TenantId = currentUser.TenantId;

            var user = _analyticRepository.DbContext.Users
                .Include(x => x.Profile)
                .Include(x => x.Role)
                .Single(x => x.Id == userId);

            return user;
        }

        private Module GetModule(string moduleName, CurrentUser currentUser)
        {
            _analyticRepository.CurrentUser = currentUser;
            _analyticRepository.TenantId = currentUser.TenantId;

            var module = _analyticRepository.DbContext.Modules
                .Include(x => x.Sections)
                .Include(x => x.Fields).ThenInclude(y => (y as Field).Validation)
                .Include(x => x.Fields).ThenInclude(y => (y as Field).Combination)
                .Include(x => x.Relations)
                .Include(x => x.Dependencies)
                .Include(x => x.Calculations)
                .FirstOrDefault(x => x.Name == moduleName && !x.Deleted);

            return module;
        }

        private static Dictionary<int, string> GetFieldsDictionary(Module module)
        {
            var fieldsDictionary = new Dictionary<int, string>();
            var standardFields = new List<string> { "id", "deleted", "shared_users", "shared_user_groups", "is_converted", "master_id", "import_id" };

            for (var i = 0; i < standardFields.Count; i++)
            {
                fieldsDictionary.Add(i, standardFields[i]);
            }

            var moduleFields = module.Fields.Where(x => !x.Deleted && x.DataType != Enums.DataType.NumberAuto).ToList();

            for (var i = 0; i < moduleFields.Count; i++)
            {
                var field = moduleFields[i];

                fieldsDictionary.Add(i + standardFields.Count, field.Name);
            }

            return fieldsDictionary;
        }

        private string GetConnectionString(string databaseName)
        {
            var connectionStringBuilder = new DbConnectionStringBuilder(false);
            connectionStringBuilder.ConnectionString = _configuration.GetConnectionString("WarehouseConnection");
            connectionStringBuilder["Database"] = databaseName;

            return connectionStringBuilder.ConnectionString;
        }
    }
}