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
        public JObject MigrateDatabase(string _databaseName, string targetVersion = null, string externalConnectionString = null)
        {

            var connectionString = string.Empty;
            var dbStatus = new JObject();
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    connectionString = string.IsNullOrWhiteSpace(externalConnectionString) ? _configuration.GetConnectionString("TenantDBConnection") : externalConnectionString;
                    var tenantDatabaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                    tenantDatabaseContext.Database.GetDbConnection().ConnectionString = Postgres.GetConnectionString(_configuration.GetConnectionString("TenantDBConnection"), _databaseName, connectionString);

                    if (targetVersion != null)
                    {
                        tenantDatabaseContext.GetService<IMigrator>().Migrate(targetVersion);
                    }
                    else
                    {
                        tenantDatabaseContext.GetService<IMigrator>().Migrate();
                    }
                }

                dbStatus["name"] = _databaseName;
                dbStatus["result"] = "Successful";
            }
            catch (Exception ex)
            {
                dbStatus["name"] = _databaseName;
                dbStatus["result"] = ex.Message;
            }

            return dbStatus;
        }

        public JObject MigrateTenantDatabases(string targetVersion = null, string externalConnectionString = null)
        {

            var result = new JObject();
            result["successful"] = new JArray();
            result["failed"] = new JArray();

            var dbs = Postgres.GetTenantDatabases(_configuration.GetConnectionString("TenantDBConnection"), externalConnectionString);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var tenantDatabaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                foreach (var databaseName in dbs)
                {
                    tenantDatabaseContext.SetCustomConnectionDatabaseName(databaseName);

                    try
                    {

                        if (targetVersion != null)
                        {
                            tenantDatabaseContext.GetService<IMigrator>().Migrate(targetVersion);
                        }
                        else
                        {
                            tenantDatabaseContext.GetService<IMigrator>().Migrate();
                        }

                        var dbStatus = new JObject();
                        dbStatus["name"] = databaseName;
                        dbStatus["result"] = "success";
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
            }

            return result;
        }

        public JObject MigrateTemplateDatabases(string targetVersion = null, string externalConnectionString = null)
        {
            var result = new JObject();
            result["successful"] = new JArray();
            result["failed"] = new JArray();

            var dbs = Postgres.GetTemplateDatabases(_configuration.GetConnectionString("TenantDBConnection"), externalConnectionString);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var tenantDatabaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                foreach (var databaseName in dbs)
                {
                    tenantDatabaseContext.SetCustomConnectionDatabaseName(databaseName);

                    try
                    {
                        Postgres.PrepareTemplateDatabaseForUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, externalConnectionString);

                        if (targetVersion != null)
                        {
                            tenantDatabaseContext.GetService<IMigrator>().Migrate(targetVersion);
                        }
                        else
                        {
                            tenantDatabaseContext.GetService<IMigrator>().Migrate();
                        }

                        Postgres.FinalizeTemplateDatabaseUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, externalConnectionString);

                        var dbStatus = new JObject
                        {
                            ["name"] = databaseName,
                            ["result"] = "success"
                        };

                        ((JArray)result["successful"]).Add(dbStatus);
                    }
                    catch (Exception ex)
                    {
                        var dbStatus = new JObject
                        {
                            ["name"] = databaseName,
                            ["result"] = ex.Message
                        };

                        ((JArray)result["failed"]).Add(dbStatus);
                    }
                }
            }

            return result;
        }

        public JObject RunSqlTenantDatabases(string sqlFilePath, string externalConnectionString = null, string app = null)
        {
            var result = new JObject();
            result["successful"] = new JArray();
            result["failed"] = new JArray();

            var sql = File.ReadAllText(sqlFilePath);
            var dbs = Postgres.GetTenantDatabases(_configuration.GetConnectionString("TenantDBConnection"), externalConnectionString);

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
            switch (appId)
            {
                case 1:
                    var sqlCrm = "SELECT * FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'leads_d'";
                    var resultCrm = Postgres.ExecuteReader(databaseName, sqlCrm, externalConnectionString);

                    if (!resultCrm.IsNullOrEmpty())
                        return true;

                    break;
                case 4:
                    var sqlIk = "SELECT * FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'calisanlar_d'";
                    var resultIk = Postgres.ExecuteReader(databaseName, sqlIk, externalConnectionString);

                    if (!resultIk.IsNullOrEmpty())
                        return true;

                    break;
            }

            return false;
        }
    }
}
