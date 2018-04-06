using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace PrimeApps.CLI.Base
{
    public class Migration
    {
        public class Database
        {
            public JObject MigrateDatabase(string _databaseName, string targetVersion = null, string externalConnectionString = null)
            {

                JObject dbStatus = new JObject();

                using (var dbContext = new Model.Context.TenantDBContext(_databaseName))
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(externalConnectionString))
                        {
                            dbContext.Database.GetDbConnection().ConnectionString = Postgres.GetConnectionString(_databaseName, externalConnectionString);
                        }

                        if (targetVersion != null)
                        {
                            dbContext.GetService<IMigrator>().Migrate(targetVersion);
                        }
                        else
                        {
                            dbContext.GetService<IMigrator>().Migrate();

                        }

                        dbStatus["name"] = _databaseName;
                        dbStatus["result"] = "Successful";
                    }
                    catch (Exception ex)
                    {
                        dbStatus["name"] = _databaseName;
                        dbStatus["result"] = ex.Message;
                    }
                }

                return dbStatus;
            }

            public JObject MigrateTenantDatabases(string targetVersion = null, string externalConnectionString = null)
            {

                JObject result = new JObject();
                result["successful"] = new JArray();
                result["failed"] = new JArray();

                IEnumerable<string> dbs = Postgres.GetTenantDatabases(externalConnectionString);

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

                IEnumerable<string> dbs = Postgres.GetTemplateDatabases(externalConnectionString);

                foreach (string databaseName in dbs)
                {
                    using (var dbContext = new Model.Context.TenantDBContext(databaseName))
                    {

                        try
                        {
                            Postgres.PrepareTemplateDatabaseForUpgrade(databaseName, externalConnectionString);

                            if (targetVersion != null)
                            {
                                dbContext.GetService<IMigrator>().Migrate(targetVersion);
                            }
                            else
                            {
                                dbContext.GetService<IMigrator>().Migrate();
                            }

                            Postgres.FinalizeTemplateDatabaseUpgrade(databaseName, externalConnectionString);

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
                var dbs = Postgres.GetTenantDatabases(externalConnectionString);

                foreach (var databaseName in dbs)
                {
                    try
                    {
                        var rslt = Postgres.ExecuteSql(databaseName, sql, externalConnectionString);

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
                var dbs = Postgres.GetTemplateDatabases(externalConnectionString);

                foreach (var databaseName in dbs)
                {
                    try
                    {
                        Postgres.PrepareTemplateDatabaseForUpgrade(databaseName, externalConnectionString);

                        var rslt = Postgres.ExecuteSql(databaseName + "_new", sql, externalConnectionString);

                        Postgres.FinalizeTemplateDatabaseUpgrade(databaseName, externalConnectionString);

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
}
