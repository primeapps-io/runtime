using Newtonsoft.Json.Linq;

namespace PrimeApps.CLI.Migration
{
    public interface IDatabaseService
    {
        JObject MigrateDatabase(string _databaseName, string targetVersion = null, string externalConnectionString = null);
        JObject MigrateTemplateDatabases(string targetVersion = null, string externalConnectionString = null);
        JObject MigrateTenantDatabases(string targetVersion = null, string externalConnectionString = null);
        JObject RunSqlTemplateDatabases(string sqlFilePath, string externalConnectionString = null);
        JObject RunSqlTenantDatabases(string sqlFilePath, string externalConnectionString = null);
    }
}