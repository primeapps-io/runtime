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

namespace PrimeApps.Migrator
{
    internal class Program
    {
        public static ILoggerFactory LoggerFactory;
        public static IConfigurationRoot Configuration;

        private static void Main(string[] args)
        {
            if (args == null || args.Length < 2 || string.IsNullOrEmpty(args[0]) || string.IsNullOrEmpty(args[1]))
            {
                Console.WriteLine("No command area and/or command specified. Migrator will exit now.");
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
                .AddSingleton<IMigrationHelper, MigrationHelper>()
                .AddSingleton<IConfiguration>(Configuration)
                .BuildServiceProvider();

            LoggerFactory = serviceProvider.GetService<ILoggerFactory>();

            var logger = LoggerFactory.CreateLogger<Program>();
            logger.LogDebug("Starting application");

            var databaseMigration = serviceProvider.GetService<IMigrationHelper>();
            var command = args[0];
            string connectionString = null;
            string sqlFile = null;
            string app = null;

            switch (args.Length)
            {
                case 2:
                    connectionString = args[1];
                    break;
                case 3:
                    connectionString = args[2];
                    sqlFile = args[1];
                    break;
                case 4:
                    connectionString = args[2];
                    sqlFile = args[1];
                    app = args[3];
                    break;
            }

            var result = new JObject();

            try
            {
                switch (command)
                {
                    case "updateTenants":
                        result = databaseMigration.MigrateTenantOrAppDatabases("tenant", connectionString);
                        break;
                    case "updateApps":
                        result = databaseMigration.MigrateTenantOrAppDatabases("app", connectionString);
                        break;
                    case "updateTemplates":
                        result = databaseMigration.MigrateTemplateDatabases(connectionString);
                        break;
                    case "runSqlTenants":
                        result = databaseMigration.RunSqlTenantDatabases(sqlFile, connectionString, app);
                        break;
                    case "runSqlApps":
                        result = databaseMigration.RunSqlAppDatabases(sqlFile, connectionString);
                        break;
                    case "runSqlTemplates":
                        result = databaseMigration.RunSqlTemplateDatabases(sqlFile, connectionString);
                        break;
                    default:
                        Console.WriteLine("No command specified. Migrator will exit now.");
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Console.WriteLine("Results:" + result);
            }
        }
    }
}