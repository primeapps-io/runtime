using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using HistoryRepository = PrimeApps.Model.Context.HistoryRepository;

namespace PrimeApps.Migrator.Helpers
{
    public interface IMigrationHelper
    {
        JObject UpdateTenantOrAppDatabases(string prefix, string connectionString = null);
        JObject UpdateTemplateDatabases(string connectionString = null);
        JObject UpdateTempletDatabases(string connectionString = null);
        JObject UpdatePlatformDatabase(string connectionString = null);
        JObject UpdateStudioDatabase(string connectionString = null);
        JObject MigratePre(string connectionString = null);
        JObject MigratePde(string connectionString = null);
        JObject RunSqlTenantDatabases(string sqlFile, string connectionString = null, string app = null);
        JObject RunSqlAppDatabases(string sqlFile, string connectionString = null);
        JObject RunSqlTemplateDatabases(string sqlFile, string connectionString = null);
        JObject RunSqlTempletDatabases(string sqlFile, string connectionString = null);
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

        public JObject UpdateTenantOrAppDatabases(string prefix, string connectionString = null)
        {
            var result = new JObject { ["successful"] = new JArray(), ["failed"] = new JArray() };
            var dbs = Postgres.GetTenantOrAppDatabases(_configuration.GetConnectionString("TenantDBConnection"), prefix, connectionString);

            foreach (var databaseName in dbs)
            {
                var optionsBuilder = new DbContextOptionsBuilder<TenantDBContext>();
                var connString = Postgres.GetConnectionString(_configuration.GetConnectionString("TenantDBConnection"), databaseName, connectionString);
                optionsBuilder.UseNpgsql(connString, x => x.MigrationsHistoryTable("_migration_history", "public")).ReplaceService<IHistoryRepository, HistoryRepository>();

                using (var tenantDatabaseContext = new TenantDBContext(optionsBuilder.Options, _configuration))
                {
                    var migrator = tenantDatabaseContext.Database.GetService<IMigrator>();
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

        public JObject UpdateTemplateDatabases(string connectionString = null)
        {
            var result = new JObject { ["successful"] = new JArray(), ["failed"] = new JArray() };
            var dbs = Postgres.GetTemplateDatabases(_configuration.GetConnectionString("TenantDBConnection"), connectionString);

            foreach (var databaseName in dbs)
            {
                var optionsBuilder = new DbContextOptionsBuilder<TenantDBContext>();
                var connString = Postgres.GetConnectionString(_configuration.GetConnectionString("TenantDBConnection"), databaseName, connectionString);
                optionsBuilder.UseNpgsql(connString, x => x.MigrationsHistoryTable("_migration_history", "public")).ReplaceService<IHistoryRepository, HistoryRepository>();

                using (var tenantDatabaseContext = new TenantDBContext(optionsBuilder.Options, _configuration))
                {
                    var migrator = tenantDatabaseContext.Database.GetService<IMigrator>();
                    var pendingMigrations = tenantDatabaseContext.Database.GetPendingMigrations().ToList();

                    if (pendingMigrations.Any())
                    {
                        try
                        {
                            Postgres.PrepareTemplateDatabaseForUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, connectionString);

                            foreach (var targetMigration in pendingMigrations)
                            {
                                migrator.Migrate(targetMigration);
                            }

                            Postgres.FinalizeTemplateDatabaseUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, connectionString);

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

        public JObject UpdateTempletDatabases(string connectionString = null)
        {
            var result = new JObject { ["successful"] = new JArray(), ["failed"] = new JArray() };
            var dbs = Postgres.GetTempletDatabases(_configuration.GetConnectionString("TenantDBConnection"), connectionString);

            foreach (var databaseName in dbs)
            {
                var optionsBuilder = new DbContextOptionsBuilder<TenantDBContext>();
                var connString = Postgres.GetConnectionString(_configuration.GetConnectionString("TenantDBConnection"), databaseName, connectionString);
                optionsBuilder.UseNpgsql(connString, x => x.MigrationsHistoryTable("_migration_history", "public")).ReplaceService<IHistoryRepository, HistoryRepository>();

                using (var tenantDatabaseContext = new TenantDBContext(optionsBuilder.Options, _configuration))
                {
                    var migrator = tenantDatabaseContext.Database.GetService<IMigrator>();
                    var pendingMigrations = tenantDatabaseContext.Database.GetPendingMigrations().ToList();

                    if (pendingMigrations.Any())
                    {
                        try
                        {
                            Postgres.PrepareTemplateDatabaseForUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, connectionString);

                            foreach (var targetMigration in pendingMigrations)
                            {
                                migrator.Migrate(targetMigration);
                            }

                            Postgres.FinalizeTemplateDatabaseUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, connectionString);

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

        public JObject UpdatePlatformDatabase(string connectionString = null)
        {
            var result = new JObject { ["successful"] = new JObject(), ["failed"] = new JObject() };

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var platformDbContext = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                var migrator = platformDbContext.Database.GetService<IMigrator>();

                if (!string.IsNullOrWhiteSpace(connectionString))
                    platformDbContext.SetConnectionString(connectionString);

                var pendingMigrations = platformDbContext.Database.GetPendingMigrations().ToList();

                if (pendingMigrations.Any())
                {
                    try
                    {
                        foreach (var targetMigration in pendingMigrations)
                        {
                            migrator.Migrate(targetMigration);
                        }

                        result["successful"] = new JObject { ["name"] = "platform", ["result"] = "success" };
                    }
                    catch (Exception ex)
                    {
                        result["failed"] = new JObject { ["name"] = "platform", ["result"] = ex.Message };
                    }
                }
            }

            return result;
        }

        public JObject UpdateStudioDatabase(string connectionString = null)
        {
            var result = new JObject { ["successful"] = new JObject(), ["failed"] = new JObject() };

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var studioDbContext = scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                var migrator = studioDbContext.Database.GetService<IMigrator>();

                if (!string.IsNullOrWhiteSpace(connectionString))
                    studioDbContext.SetConnectionString(connectionString);

                var pendingMigrations = studioDbContext.Database.GetPendingMigrations().ToList();

                if (pendingMigrations.Any())
                {
                    try
                    {
                        foreach (var targetMigration in pendingMigrations)
                        {
                            migrator.Migrate(targetMigration);
                        }

                        result["successful"] = new JObject { ["name"] = "studio", ["result"] = "success" };
                    }
                    catch (Exception ex)
                    {
                        result["failed"] = new JObject { ["name"] = "studio", ["result"] = ex.Message };
                    }
                }
            }

            return result;
        }

        public JObject MigratePre(string connectionString = null)
        {
            var resultPlatform = UpdatePlatformDatabase(connectionString);
            var resultTemplates = UpdateTemplateDatabases(connectionString);
            var resultTenants = UpdateTenantOrAppDatabases("tenant", connectionString);
            var result = new JObject { ["platform"] = resultPlatform, ["templates"] = resultTemplates, ["tenants"] = resultTenants };

            return result;
        }

        public JObject MigratePde(string connectionString = null)
        {
            var resultStudio = UpdateStudioDatabase(connectionString);
            var resultTemplets = UpdateTempletDatabases(connectionString);
            var resultTenants = UpdateTenantOrAppDatabases("app", connectionString);
            var result = new JObject { ["studio"] = resultStudio, ["templets"] = resultTemplets, ["tenants"] = resultTenants };

            return result;
        }

        public JObject RunSqlTenantDatabases(string sqlFile, string connectionString = null, string app = null)
        {
            if (string.IsNullOrEmpty(sqlFile))
                throw new Exception("sqlFilePath cannot be null.");

            var result = new JObject();
            result["successful"] = new JArray();
            result["failed"] = new JArray();

            var sql = File.ReadAllText(sqlFile);
            var dbs = Postgres.GetTenantOrAppDatabases(_configuration.GetConnectionString("TenantDBConnection"), "tenant", connectionString);

            var appId = 0;

            if (app != null)
                appId = int.Parse(app);

            foreach (var databaseName in dbs)
            {
                try
                {
                    if (appId > 0 && !CheckDatabaseApp(databaseName, appId, connectionString))
                        continue;

                    var rslt = Postgres.ExecuteNonQuery(_configuration.GetConnectionString("TenantDBConnection"), databaseName, sql, connectionString);

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

        public JObject RunSqlAppDatabases(string sqlFile, string connectionString = null)
        {
            if (string.IsNullOrEmpty(sqlFile))
                throw new Exception("sqlFilePath cannot be null.");

            var result = new JObject();
            result["successful"] = new JArray();
            result["failed"] = new JArray();

            var sql = File.ReadAllText(sqlFile);
            var dbs = Postgres.GetTenantOrAppDatabases(_configuration.GetConnectionString("TenantDBConnection"), "app", connectionString);

            foreach (var databaseName in dbs)
            {
                try
                {
                    var rslt = Postgres.ExecuteNonQuery(_configuration.GetConnectionString("TenantDBConnection"), databaseName, sql, connectionString);

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

        public JObject RunSqlTemplateDatabases(string sqlFile, string connectionString = null)
        {
            if (string.IsNullOrEmpty(sqlFile))
                throw new Exception("sqlFilePath cannot be null.");

            var result = new JObject();
            result["successful"] = new JArray();
            result["failed"] = new JArray();

            var sql = File.ReadAllText(sqlFile);
            var dbs = Postgres.GetTemplateDatabases(_configuration.GetConnectionString("TenantDBConnection"), connectionString);

            foreach (var databaseName in dbs)
            {
                try
                {
                    Postgres.PrepareTemplateDatabaseForUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, connectionString);

                    var rslt = Postgres.ExecuteNonQuery(_configuration.GetConnectionString("TenantDBConnection"), databaseName + "_new", sql, connectionString);

                    Postgres.FinalizeTemplateDatabaseUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, connectionString);

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

        public JObject RunSqlTempletDatabases(string sqlFile, string connectionString = null)
        {
            if (string.IsNullOrEmpty(sqlFile))
                throw new Exception("sqlFilePath cannot be null.");

            var result = new JObject();
            result["successful"] = new JArray();
            result["failed"] = new JArray();

            var sql = File.ReadAllText(sqlFile);
            var dbs = Postgres.GetTempletDatabases(_configuration.GetConnectionString("TenantDBConnection"), connectionString);

            foreach (var databaseName in dbs)
            {
                try
                {
                    Postgres.PrepareTemplateDatabaseForUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, connectionString);

                    var rslt = Postgres.ExecuteNonQuery(_configuration.GetConnectionString("TenantDBConnection"), databaseName + "_new", sql, connectionString);

                    Postgres.FinalizeTemplateDatabaseUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, connectionString);

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