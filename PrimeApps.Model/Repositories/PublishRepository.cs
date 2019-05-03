using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class PublishRepository : RepositoryBaseTenant, IPublishRepository
    {
        public PublishRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext,
            configuration)
        {
        }

        public JArray GetAllDynamicTables()
        {
            var dynamicTablesSql = PublishHelper.GetAllDynamicTablesSql();
            return DbContext.Database.SqlQueryDynamic(dynamicTablesSql);
        }

        public bool CleanUp(JArray tableNames = null)
        {
            if (tableNames == null)
                tableNames = GetAllDynamicTables();

            var cleanUpSystemTablesSql = PublishHelper.CleanUpSystemTables();
            var cleanUpSystemTablesResult = DbContext.Database.SqlQueryDynamic(cleanUpSystemTablesSql);

            CleanUpTables(tableNames);
            
            var setIsSampleSql = PublishHelper.SetRecordsIsSample();
            var setIsSampleResult = DbContext.Database.SqlQueryDynamic(setIsSampleSql);

            var systemTablesSql = PublishHelper.GetAllSystemTablesSql();
            var tableData = DbContext.Database.SqlQueryDynamic(systemTablesSql);
            if (!tableData.HasValues) return false;

            foreach (var t in tableData)
            {
                var table = t["table_name"];
                var columnNamesSql = PublishHelper.GetUserFKColumnsSql(table.ToString());

                var columns = DbContext.Database.SqlQueryDynamic(columnNamesSql);

                if (!columns.HasValues) continue;

                var recordsSql = PublishHelper.GetAllRecordsWithColumns(table.ToString(), columns);

                var records = DbContext.Database.SqlQueryDynamic(recordsSql);

                foreach (var record in records)
                {
                    var updateRecordSql = PublishHelper.UpdateRecordSql(record, table.ToString(), columns, 1);

                    var result = DbContext.Database.SqlQueryDynamic(updateRecordSql);
                }
            }

            return true;
        }

        public bool CleanUpTables(JArray tableNames = null)
        {
            foreach (var table in tableNames)
            {
                var sql = $"DELETE FROM {table.ToString()}];";
                var result = DbContext.Database.SqlQueryDynamic(sql);
            }


            return true;
        }
    }
}