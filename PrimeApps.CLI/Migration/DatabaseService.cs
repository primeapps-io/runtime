using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace PrimeApps.CLI.Migration
{
    public class DatabaseService : IDatabaseService
    {
        private readonly TenantDBContext _tenantDbContext;
        private readonly PlatformDBContext _platformDBContext;
        private readonly IConfiguration _configuration;

        public DatabaseService(TenantDBContext tenantDbContext, PlatformDBContext platformDbContext, IConfiguration configuration)
        {
            _tenantDbContext = tenantDbContext;
            _platformDBContext = platformDbContext;
            _configuration = configuration;

        }
        public JObject MigrateDatabase(string _databaseName, string targetVersion = null, string externalConnectionString = null)
        {

            string connectionString = string.Empty;
            JObject dbStatus = new JObject();
            try
            {
                connectionString = string.IsNullOrWhiteSpace(externalConnectionString) ? _configuration.GetConnectionString("TenantDBConnection") : externalConnectionString;

                _tenantDbContext.Database.GetDbConnection().ConnectionString = Postgres.GetConnectionString(_configuration.GetConnectionString("TenantDBConnection"), _databaseName, connectionString);

                if (targetVersion != null)
                {
                    _tenantDbContext.GetService<IMigrator>().Migrate(targetVersion);
                }
                else
                {
                    _tenantDbContext.GetService<IMigrator>().Migrate();

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

            JObject result = new JObject();
            result["successful"] = new JArray();
            result["failed"] = new JArray();

            IEnumerable<string> dbs = Postgres.GetTenantDatabases(_configuration.GetConnectionString("TenantDBConnection"), externalConnectionString);

            foreach (string databaseName in dbs)
            {
                using (var dbContext = new Model.Context.TenantDBContext(databaseName))
                {
                    try
                    {

                        if (targetVersion != null)
                        {
                            dbContext.GetService<IMigrator>().Migrate(targetVersion);
                        }
                        else
                        {
                            dbContext.GetService<IMigrator>().Migrate();
                        }

                        JObject dbStatus = new JObject();
                        dbStatus["name"] = databaseName;
                        dbStatus["result"] = "success";
                        ((JArray)result["successful"]).Add(dbStatus);
                    }
                    catch (Exception ex)
                    {
                        JObject dbStatus = new JObject();
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
            JObject result = new JObject();
            result["successful"] = new JArray();
            result["failed"] = new JArray();

            IEnumerable<string> dbs = Postgres.GetTemplateDatabases(_configuration.GetConnectionString("TenantDBConnection"), externalConnectionString);

            foreach (string databaseName in dbs)
            {
                using (var dbContext = new Model.Context.TenantDBContext(databaseName))
                {

                    try
                    {
                        Postgres.PrepareTemplateDatabaseForUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, externalConnectionString);

                        if (targetVersion != null)
                        {
                            dbContext.GetService<IMigrator>().Migrate(targetVersion);
                        }
                        else
                        {
                            dbContext.GetService<IMigrator>().Migrate();
                        }

                        Postgres.FinalizeTemplateDatabaseUpgrade(_configuration.GetConnectionString("TenantDBConnection"), databaseName, externalConnectionString);

                        JObject dbStatus = new JObject
                        {
                            ["name"] = databaseName,
                            ["result"] = "success"
                        };

                        ((JArray)result["successful"]).Add(dbStatus);
                    }
                    catch (Exception ex)
                    {
                        JObject dbStatus = new JObject
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

        public JObject RunSqlTenantDatabases(string sqlFilePath, string externalConnectionString = null)
        {
            var result = new JObject();
            result["successful"] = new JArray();
            result["failed"] = new JArray();

            var sql = File.ReadAllText(sqlFilePath);
            var dbs = Postgres.GetTenantDatabases(_configuration.GetConnectionString("TenantDBConnection"), externalConnectionString);

            foreach (var databaseName in dbs)
            {
                try
                {
                    var rslt = Postgres.ExecuteSql(_configuration.GetConnectionString("TenantDBConnection"), databaseName, sql, externalConnectionString);

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

                    var rslt = Postgres.ExecuteSql(_configuration.GetConnectionString("TenantDBConnection"), databaseName + "_new", sql, externalConnectionString);

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
    }
}
