using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using PrimeApps.CLI.Migration;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PrimeApps.Model.Context;

namespace PrimeApps.CLI
{
    class Program
    {
        public static ILoggerFactory LoggerFactory;
        public static IConfigurationRoot Configuration;

        static void Main(string[] args)
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
                .AddLogging(config=>config.SetMinimumLevel(LogLevel.Error))
                .AddEntityFrameworkNpgsql()
                .AddDbContext<TenantDBContext>(options => options.UseNpgsql(Configuration.GetConnectionString("TenantDBConnection")))
                .AddDbContext<PlatformDBContext>(options=>options.UseNpgsql(Configuration.GetConnectionString("PlatformDBConnection")))
                .AddSingleton<IDatabaseService, DatabaseService>()
                .AddSingleton<IConfiguration>(Configuration)
                .BuildServiceProvider();

            LoggerFactory = serviceProvider.GetService<ILoggerFactory>();

            var logger = LoggerFactory.CreateLogger<Program>();
            logger.LogDebug("Starting application");

            var databaseMigration = serviceProvider.GetService<IDatabaseService>();



            string commandArea = args[0];
            string command = args[1];
            string param1 = args.Length > 2 ? args[2] : null;
            string param2 = args.Length > 3 ? args[3] : null;
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
                                result = databaseMigration.MigrateTenantDatabases(param1);
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

                                result = databaseMigration.MigrateTenantDatabases(version, connStr);

                                break;
                            default:
                                var tenantDatabaseName = command;
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
                            default:
                                result = databaseMigration.RunSqlTenantDatabases(param1, param2);
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

        static void PrintResult(JObject result, bool deploy, string version = null, Exception ex = null)
        {
            if (version != null)
            {
                Console.WriteLine("DbStatus:" + result.ToString() + " for version " + version);
            }
            else
            {
                Console.WriteLine("DbStatus:" + result.ToString());
            }
            if (((JArray)result["failed"]).Count() > 0 && deploy) throw new Exception("Aborted deployment because of unsuccesful migration.");

            if (ex != null) throw ex;
        }
    }
}
