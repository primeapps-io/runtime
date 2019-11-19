using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PrimeApps.CLI.Migration;
using PrimeApps.Model.Context;
using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using PrimeApps.Model.Helpers;

namespace PrimeApps.CLI
{
    internal class Program
    {
        public static ILoggerFactory LoggerFactory;
        public static IConfigurationRoot Configuration;

        private static void Main(string[] args)
        {
            if (args == null || args.Length < 2 || string.IsNullOrEmpty(args[0]) || string.IsNullOrEmpty(args[1]))
            {
                Console.WriteLine("No command area and/or command specified. CLI will exit.");
                Environment.Exit(0);
            }

            // Adding JSON file into IConfiguration.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            // Setup DI
            var serviceProvider = new ServiceCollection()
                .AddLogging(config => config.SetMinimumLevel(LogLevel.Error))
                .AddEntityFrameworkNpgsql()
                .AddDbContext<TenantDBContext>(options => options.UseNpgsql(Configuration.GetConnectionString("TenantDBConnection"), x => x.MigrationsHistoryTable("_migration_history", "public")).ReplaceService<IHistoryRepository, PostgreHistoryContext>())
                .AddDbContext<PlatformDBContext>(options => options.UseNpgsql(Configuration.GetConnectionString("PlatformDBConnection"), x => x.MigrationsHistoryTable("_migration_history", "public")).ReplaceService<IHistoryRepository, PostgreHistoryContext>())
                .AddDbContext<StudioDBContext>(options => options.UseNpgsql(Configuration.GetConnectionString("StudioDBConnection"), x => x.MigrationsHistoryTable("_migration_history", "public")).ReplaceService<IHistoryRepository, PostgreHistoryContext>())
                .AddHttpContextAccessor()
                .AddSingleton<IDatabaseService, DatabaseService>()
                .AddSingleton<IConfiguration>(Configuration)
                .BuildServiceProvider();

            LoggerFactory = serviceProvider.GetService<ILoggerFactory>();

            var logger = LoggerFactory.CreateLogger<Program>();
            logger.LogDebug("Starting application");

            var databaseMigration = serviceProvider.GetService<IDatabaseService>();
            var commandArea = args[0];
            var command = args[1];
            var param1 = args.Length > 2 ? args[2] : null;
            var param2 = args.Length > 3 ? args[3] : null;
            var param3 = args.Length > 4 ? args[4] : null;
            string connStr = null;
            string version = null;

            var result = new JObject();
            Exception exception = null;
            var deploy = false;

            try
            {
                switch (commandArea)
                {
                    case "tenantUpdate":
                        switch (command)
                        {
                            case "-all":
                                result = databaseMigration.MigrateTenantDatabases("tenant", param1);
                                break;
                            case "-all-deploy":
                                /*
                                 * Verdiğini database connectiona göre tüm tenant databaslerine migration varsa uyguluyor
                                 * example  tenantUpdate -all-deploy "server=localhost;port=5432;username=postgres;password=123456;database=dev;command timeout=0;keepalive=30;" 
                                 */
                                
                                deploy = true;
                                if (!string.IsNullOrWhiteSpace(param1) && param1.Contains("server="))
                                {
                                    connStr = param1;
                                    version = param2;
                                }
                                else if (!string.IsNullOrWhiteSpace(param2) && param2.Contains("server="))
                                {
                                    connStr = param2;
                                    version = param1;
                                }

                                result = databaseMigration.MigrateTenantDatabases("tenant", version, connStr);

                                break;
                            default:
                                string tenantDatabaseName = command;
                                result = databaseMigration.MigrateDatabase(tenantDatabaseName, param1);
                                break;
                        }

                        break;
                    case "appUpdate":
                        switch (command)
                        {
                            case "-all":
                                result = databaseMigration.MigrateTenantDatabases("app", param1);
                                break;
                            case "-all-deploy":
                                deploy = true;
                                if (!string.IsNullOrWhiteSpace(param1) && param1.Contains("server="))
                                {
                                    connStr = param1;
                                    version = param2;
                                }
                                else if (!string.IsNullOrWhiteSpace(param2) && param2.Contains("server="))
                                {
                                    connStr = param2;
                                    version = param1;
                                }

                                result = databaseMigration.MigrateTenantDatabases("app", version, connStr);

                                break;
                            default:
                                string tenantDatabaseName = command;
                                result = databaseMigration.MigrateDatabase(tenantDatabaseName, param1);
                                break;
                        }

                        break;
                    case "templateUpdate":
                        switch (command)
                        {
                            case "-all":
                                result = databaseMigration.MigrateTemplateDatabases(param1);
                                break;
                            case "-all-deploy":
                                deploy = true;

                                if (!string.IsNullOrWhiteSpace(param1) && param1.Contains("server="))
                                {
                                    connStr = param1;
                                    version = param2;
                                }
                                else if (!string.IsNullOrWhiteSpace(param2) && param2.Contains("server="))
                                {
                                    connStr = param2;
                                    version = param1;
                                }

                                result = databaseMigration.MigrateTemplateDatabases(version, connStr);
                                break;
                        }

                        break;
                    case "runSql":
                        switch (command)
                        {
                            case "-template":
                                result = databaseMigration.RunSqlTemplateDatabases(param1, param2);
                                break;
                            case "-app":
                                result = databaseMigration.RunSqlTenantDatabases("app", param1, param2, param3);
                                break;
                            case "-tenant":
                                result = databaseMigration.RunSqlTenantDatabases("tenant", param1, param2, param3);
                                break;
                        }

                        break;
                    default:
                        Console.WriteLine("No command area specified. CLI will exit.");
                        break;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                PrintResult(result, deploy, version, exception);
            }
        }

        private static void PrintResult(JObject result, bool deploy, string version = null, Exception ex = null)
        {
            if (version != null)
            {
                Console.WriteLine("DbStatus:" + result + " for version " + version);
            }
            else
            {
                Console.WriteLine("DbStatus:" + result);
            }

            if (!result["failed"].IsNullOrEmpty() && ((JArray)result["failed"]).Any() && deploy)
            {
                throw new Exception("Aborted deployment because of unsuccesful migration.");
            }

            if (ex != null)
            {
                throw ex;
            }
        }
    }
}