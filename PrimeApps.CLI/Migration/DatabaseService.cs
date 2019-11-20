using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using System;
using System.IO;
using System.Linq;

namespace PrimeApps.CLI.Migration
{
    public class DatabaseService : IDatabaseService
    {
        private readonly IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;

        public DatabaseService(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public JObject MigrateDatabase(string databaseName, string targetVersion = null, string externalConnectionString = null)
        {
            var dbStatus = new JObject();

            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var connectionString = string.IsNullOrWhiteSpace(externalConnectionString) ? _configuration.GetConnectionString("TenantDBConnection") : externalConnectionString;
                    var tenantDatabaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                    var migrator = tenantDatabaseContext.Database.GetService<IMigrator>();

                    tenantDatabaseContext.Database.GetDbConnection().ConnectionString = Postgres.GetConnectionString(_configuration.GetConnectionString("TenantDBConnection"), databaseName, connectionString);

                    if (string.IsNullOrEmpty(targetVersion))
                    {
                        var pendingMigrations = tenantDatabaseContext.Database.GetPendingMigrations().ToList();

                        if (pendingMigrations.Any())
                        {
                            foreach (var targetMigration in pendingMigrations)
                            {
                                migrator.Migrate(targetMigration);
                            }
                        }
                    }
                    else
                    {
                        migrator.Migrate(targetVersion);
                    }
                }

                dbStatus["name"] = databaseName;
                dbStatus["result"] = "Successful";
            }
            catch (Exception ex)
            {
                dbStatus["name"] = databaseName;
                dbStatus["result"] = ex.Message;
            }

            return dbStatus;
        }

        public JObject MigrateTenantDatabases(string prefix, string targetVersion = null, string externalConnectionString = null)
        {
            var result = new JObject { ["successful"] = new JArray(), ["failed"] = new JArray() };

            var dbs = Postgres.GetTenantDatabases(_configuration.GetConnectionString("TenantDBConnection"), prefix, externalConnectionString);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var tenantDatabaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                var migrator = tenantDatabaseContext.Database.GetService<IMigrator>();

                foreach (var databaseName in dbs)
                {
                    tenantDatabaseContext.SetConnectionDatabaseName(databaseName, _configuration, externalConnectionString);

                    if (string.IsNullOrEmpty(targetVersion))
                    {
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
                    else
                    {
                        try
                        {
                            migrator.Migrate(targetVersion);
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

        public JObject MigrateTemplateDatabases(string targetVersion = null, string externalConnectionString = null)
        {
            var result = new JObject { ["successful"] = new JArray(), ["failed"] = new JArray() };
            var dbs = Postgres.GetTemplateDatabases(_configuration.GetConnectionString("TenantDBConnection"), externalConnectionString);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var tenantDatabaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                var migrator = tenantDatabaseContext.Database.GetService<IMigrator>();

                foreach (var databaseName in dbs)
                {
                    var newDatabaseName = databaseName + "_new";
                    tenantDatabaseContext.SetConnectionDatabaseName(newDatabaseName, _configuration, externalConnectionString);

                    Postgres.PrepareTemplateDatabaseForUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, externalConnectionString);

                    if (string.IsNullOrEmpty(targetVersion))
                    {
                        var pendingMigrations = tenantDatabaseContext.Database.GetPendingMigrations().ToList();

                        if (pendingMigrations.Any())
                        {
                            try
                            {
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
                    else
                    {
                        try
                        {
                            Postgres.PrepareTemplateDatabaseForUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, externalConnectionString);

                            migrator.Migrate(targetVersion);

                            Postgres.FinalizeTemplateDatabaseUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, externalConnectionString);
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

        public JObject RunSqlTenantDatabases(string prefix, string sqlFilePath, string externalConnectionString = null, string app = null)
        {
            var result = new JObject();
            result["successful"] = new JArray();
            result["failed"] = new JArray();

            var sql = File.ReadAllText(sqlFilePath);
            var dbs = Postgres.GetTenantDatabases(_configuration.GetConnectionString("TenantDBConnection"), prefix, externalConnectionString);

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

        public JObject RunSqlTemplateDatabases(string sqlFilePath, string externalConnectionString = null)
        {
            var result = new JObject();
            result["successful"] = new JArray();
            result["failed"] = new JArray();

            var sql = File.ReadAllText(sqlFilePath);
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