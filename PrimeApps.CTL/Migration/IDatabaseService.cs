using Newtonsoft.Json.Linq;

namespace PrimeApps.CTL.Migration
{
    public interface IDatabaseService
    {
        JObject MigrateDatabase(string databaseName, string targetVersion = null, string externalConnectionString = null);
        JObject MigrateTemplateDatabases(string targetVersion = null, string externalConnectionString = null);
        JObject MigrateTenantDatabases(string prefix, string targetVersion = null, string externalConnectionString = null);
        JObject RunSqlTemplateDatabases(string sqlFilePath, string externalConnectionString = null);
        JObject RunSqlTenantDatabases(string prefix, string sqlFilePath, string externalConnectionString = null, string app = null);
    }
}