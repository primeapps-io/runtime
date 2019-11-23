using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Migrator.Helpers
{
    public interface IMigrationHelper
    {
        JObject MigrateTemplateDatabases(string externalConnectionString = null);
        JObject MigrateTenantOrAppDatabases(string prefix, string externalConnectionString = null);
        JObject RunSqlTenantDatabases(string sqlFile, string externalConnectionString = null, string app = null);
        JObject RunSqlAppDatabases(string sqlFile, string externalConnectionString = null);
        JObject RunSqlTemplateDatabases(string sqlFile, string externalConnectionString = null);
    }

    public class MigrationHelper : IMigrationHelper
    {
        private readonly IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;

        public MigrationHelper(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public JObject MigrateTenantOrAppDatabases(string prefix, string externalConnectionString = null)
        {
            var result = new JObject { ["successful"] = new JArray(), ["failed"] = new JArray() };

            var dbs = Postgres.GetTenantOrAppDatabases(_configuration.GetConnectionString("TenantDBConnection"), prefix, externalConnectionString);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var tenantDatabaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                var migrator = tenantDatabaseContext.Database.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();

                foreach (var databaseName in dbs)
                {
                    tenantDatabaseContext.SetConnectionDatabaseName(databaseName, _configuration, externalConnectionString);

                    var pendingMigrations = tenantDatabaseContext.Database.GetPendingMigrations().ToList();

                    if (pendingMigrations.Any())
                    {
                        try
                        {
                            foreach (var targetMigration in pendingMigrations)
                            {
                                migrator.Migrate(targetMigration);
                            }

                            ((JArray)result["successful"]).Add(new JObject { ["name"] = databaseName, ["result"] = "success" });
                        }
                        catch (Exception ex)
                        {
                            ((JArray)result["failed"]).Add(new JObject { ["name"] = databaseName, ["result"] = ex.Message });
                        }
                    }
                }
            }

            return result;
        }

        public JObject MigrateTemplateDatabases(string externalConnectionString = null)
        {
            var result = new JObject { ["successful"] = new JArray(), ["failed"] = new JArray() };
            var dbs = Postgres.GetTemplateDatabases(_configuration.GetConnectionString("TenantDBConnection"), externalConnectionString);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var tenantDatabaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                var migrator = tenantDatabaseContext.Database.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();

                foreach (var databaseName in dbs)
                {
                    tenantDatabaseContext.SetConnectionDatabaseName(databaseName, _configuration, externalConnectionString);

                    var pendingMigrations = tenantDatabaseContext.Database.GetPendingMigrations().ToList();

                    if (pendingMigrations.Any())
                    {
                        try
                        {
                            Postgres.PrepareTemplateDatabaseForUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, externalConnectionString);

                            foreach (var targetMigration in pendingMigrations)
                            {
                                migrator.Migrate(targetMigration);
                            }

                            Postgres.FinalizeTemplateDatabaseUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, externalConnectionString);

                            ((JArray)result["successful"]).Add(new JObject { ["name"] = databaseName, ["result"] = "success" });
                        }
                        catch (Exception ex)
                        {
                            ((JArray)result["failed"]).Add(new JObject { ["name"] = databaseName, ["result"] = ex.Message });
                        }
                    }
                }
            }

            return result;
        }

        public JObject RunSqlTenantDatabases(string sqlFile, string externalConnectionString = null, string app = null)
        {
            if (string.IsNullOrEmpty(sqlFile))
                throw new Exception("sqlFilePath cannot be null.");

            var result = new JObject();
            result["successful"] = new JArray();
            result["failed"] = new JArray();

            var sql = File.ReadAllText(sqlFile);
            var dbs = Postgres.GetTenantOrAppDatabases(_configuration.GetConnectionString("TenantDBConnection"), "tenant", externalConnectionString);

            var appId = 0;

            if (app != null)
                appId = int.Parse(app);

            foreach (var databaseName in dbs)
            {
                try
                {
                    if (appId > 0 && !CheckDatabaseApp(databaseName, appId, externalConnectionString))
                        continue;

                    var rslt = Postgres.ExecuteNonQuery(_configuration.GetConnectionString("TenantDBConnection"), databaseName, sql, externalConnectionString);

                    var dbStatus = new JObject();
                    dbStatus["name"] = databaseName;
                    dbStatus["result"] = "success. result: " + rslt;
                    ((JArray)result["successful"]).Add(dbStatus);
                }
                catch (Exception ex)
                {
                    var dbStatus = new JObject();
                    dbStatus["name"] = databaseName;
                    dbStatus["result"] = ex.Message;
                    ((JArray)result["failed"]).Add(dbStatus);
                }
            }

            return result;
        }

        public JObject RunSqlAppDatabases(string sqlFile, string externalConnectionString = null)
        {
            if (string.IsNullOrEmpty(sqlFile))
                throw new Exception("sqlFilePath cannot be null.");

            var result = new JObject();
            result["successful"] = new JArray();
            result["failed"] = new JArray();

            var sql = File.ReadAllText(sqlFile);
            var dbs = Postgres.GetTenantOrAppDatabases(_configuration.GetConnectionString("TenantDBConnection"), "app", externalConnectionString);

            foreach (var databaseName in dbs)
            {
                try
                {
                    var rslt = Postgres.ExecuteNonQuery(_configuration.GetConnectionString("TenantDBConnection"), databaseName, sql, externalConnectionString);

                    var dbStatus = new JObject();
                    dbStatus["name"] = databaseName;
                    dbStatus["result"] = "success. result: " + rslt;
                    ((JArray)result["successful"]).Add(dbStatus);
                }
                catch (Exception ex)
                {
                    var dbStatus = new JObject();
                    dbStatus["name"] = databaseName;
                    dbStatus["result"] = ex.Message;
                    ((JArray)result["failed"]).Add(dbStatus);
                }
            }

            return result;
        }

        public JObject RunSqlTemplateDatabases(string sqlFile, string externalConnectionString = null)
        {
            if (string.IsNullOrEmpty(sqlFile))
                throw new Exception("sqlFilePath cannot be null.");

            var result = new JObject();
            result["successful"] = new JArray();
            result["failed"] = new JArray();

            var sql = File.ReadAllText(sqlFile);
            var dbs = Postgres.GetTemplateDatabases(_configuration.GetConnectionString("TenantDBConnection"), externalConnectionString);

            foreach (var databaseName in dbs)
            {
                try
                {
                    Postgres.PrepareTemplateDatabaseForUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, externalConnectionString);

                    var rslt = Postgres.ExecuteNonQuery(_configuration.GetConnectionString("TenantDBConnection"), databaseName + "_new", sql, externalConnectionString);

                    Postgres.FinalizeTemplateDatabaseUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, externalConnectionString);

                    var dbStatus = new JObject();
                    dbStatus["name"] = databaseName;
                    dbStatus["result"] = "success. result: " + rslt;
                    ((JArray)result["successful"]).Add(dbStatus);
                }
                catch (Exception ex)
                {
                    var dbStatus = new JObject();
                    dbStatus["name"] = databaseName;
                    dbStatus["result"] = ex.Message;
                    ((JArray)result["failed"]).Add(dbStatus);
                }
            }

            return result;
        }

        private bool CheckDatabaseApp(string databaseName, int appId, string externalConnectionString = null)
        {
            var tenantId = int.Parse(databaseName.Replace("tenant", ""));
            var sql = $"SELECT * FROM tenants WHERE app_id={appId} AND id={tenantId};";
            var result = Postgres.ExecuteReader("platform", sql, externalConnectionString);

            return !result.IsNullOrEmpty();
        }
    }
}