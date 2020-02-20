using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PrimeApps.Migrator.Helpers;
using PrimeApps.Model.Context;
using System;
using System.IO;
using PrimeApps.Model.Helpers;
using HistoryRepository = PrimeApps.Model.Context.HistoryRepository;

namespace PrimeApps.Migrator
{
    internal class Program
    {
        public static ILoggerFactory LoggerFactory;
        public static IConfigurationRoot Configuration;

        private static void Main(string[] args)
        {
            if (args == null || args.Length < 1 || string.IsNullOrEmpty(args[0]))
            {
                Console.WriteLine("No command specified. Migrator will exit now.");
                Environment.Exit(0);
            }

            // Adding JSON file into IConfiguration.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            // Setup DI
            var serviceProvider = new ServiceCollection()
                .AddLogging(config => config.SetMinimumLevel(LogLevel.Error))
                .AddEntityFrameworkNpgsql()
                .AddSingleton<IConfiguration>(Configuration)
                .AddDbContext<TenantDBContext>(options => options.UseNpgsql(Configuration.GetConnectionString("TenantDBConnection"), x => x.MigrationsHistoryTable("_migration_history", "public")).ReplaceService<IHistoryRepository, HistoryRepository>())
                .AddDbContext<PlatformDBContext>(options => options.UseNpgsql(Configuration.GetConnectionString("PlatformDBConnection"), x => x.MigrationsHistoryTable("_migration_history", "public")).ReplaceService<IHistoryRepository, HistoryRepository>())
                .AddDbContext<StudioDBContext>(options => options.UseNpgsql(Configuration.GetConnectionString("StudioDBConnection"), x => x.MigrationsHistoryTable("_migration_history", "public")).ReplaceService<IHistoryRepository, HistoryRepository>())
                .AddSingleton<IMigrationHelper, MigrationHelper>()
                .AddHttpContextAccessor()
                .BuildServiceProvider();

            LoggerFactory = serviceProvider.GetService<ILoggerFactory>();

            var logger = LoggerFactory.CreateLogger<Program>();
            logger.LogDebug("Starting application");

            var databaseMigration = serviceProvider.GetService<IMigrationHelper>();
            var command = args[0];
            string connectionString = null;
            string sqlFile = null;
            string app = null;
            Exception exception = null;

            switch (args.Length)
            {
                case 2:
                    connectionString = args[1];
                    break;
                case 3:
                    sqlFile = args[1];

                    if (!int.TryParse(args[2], out var appId))
                        connectionString = args[2];
                    else
                        app = args[2];
                    break;
                case 4:
                    sqlFile = args[1];
                    connectionString = args[2];
                    app = args[3];
                    break;
            }

            var result = new JObject();

            try
            {
                switch (command)
                {
                    case "update-tenants":
                        result = databaseMigration.UpdateTenantOrAppDatabases("tenant", connectionString);
                        break;
                    case "update-apps":
                        result = databaseMigration.UpdateTenantOrAppDatabases("app", connectionString ?? Configuration.GetConnectionString("StudioDBConnection"));
                        break;
                    case "update-templates":
                        result = databaseMigration.UpdateTemplateDatabases(connectionString);
                        break;
                    case "update-templets":
                        result = databaseMigration.UpdateTempletDatabases(connectionString);
                        break;
                    case "update-platform":
                        result = databaseMigration.UpdatePlatformDatabase(connectionString);
                        break;
                    case "update-studio":
                        result = databaseMigration.UpdateStudioDatabase(connectionString);
                        break;
                    case "update-pre":
                        result = databaseMigration.UpdatePre(connectionString);
                        break;
                    case "update-pde":
                        result = databaseMigration.UpdatePde(connectionString);
                        break;
                    case "runsql-tenants":
                        result = databaseMigration.RunSqlTenantDatabases(sqlFile, connectionString, app);
                        break;
                    case "runsql-apps":
                        result = databaseMigration.RunSqlAppDatabases(sqlFile, connectionString);
                        break;
                    case "runsql-templates":
                        result = databaseMigration.RunSqlTemplateDatabases(sqlFile, connectionString);
                        break;
                    case "runsql-templets":
                        result = databaseMigration.RunSqlTempletDatabases(sqlFile, connectionString);
                        break;
                    default:
                        Console.WriteLine("No command specified. Migrator will exit now.");
                        break;
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                result["has_error"] = "true";
            }
            finally
            {
                if (!result["platform"].IsNullOrEmpty() && !result["platform"]["failed"].IsNullOrEmpty())
                    result["has_error"] = "true";
                
                if (!result["templates"].IsNullOrEmpty() && !result["templates"]["failed"].IsNullOrEmpty())
                    result["has_error"] = "true";
                
                if (!result["tenants"].IsNullOrEmpty() && !result["tenants"]["failed"].IsNullOrEmpty())
                    result["has_error"] = "true";
                
                if (!result["studio"].IsNullOrEmpty() && !result["studio"]["failed"].IsNullOrEmpty())
                    result["has_error"] = "true";
                
                if (!result["templets"].IsNullOrEmpty() && !result["templets"]["failed"].IsNullOrEmpty())
                    result["has_error"] = "true";

                if (!result["failed"].IsNullOrEmpty())
                    result["has_error"] = "true";
                
                Console.WriteLine(result);
            }

            if (exception != null)
                throw exception;
        }
    }
}