using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.Model.Context
{
    public abstract class DbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext> where TContext : DbContext
    {
        public TContext CreateDbContext(string[] args)
        {
            return Create(Directory.GetCurrentDirectory(), Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        }

        protected abstract TContext CreateNewInstance(DbContextOptions<TContext> options, IConfiguration _configuration);

        public TContext Create()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var basePath = AppContext.BaseDirectory;

            return Create(basePath, environmentName);
        }

        private TContext Create(string basePath, string environmentName)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .AddEnvironmentVariables();

            var config = builder.Build();
            var connectionString = "";

            if (typeof(TContext) == typeof(TenantDBContext))
                connectionString = config.GetConnectionString("TenantDBConnection");
            else if (typeof(TContext) == typeof(PlatformDBContext))
                connectionString = config.GetConnectionString("PlatformDBConnection");
            else if (typeof(TContext) == typeof(StudioDBContext))
                connectionString = config.GetConnectionString("StudioDBConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Could not find a connection string!");

            return Create(connectionString, config);
        }

        private TContext Create(string connectionString, IConfiguration _configuration)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException($"{nameof(connectionString)} is null or empty.", nameof(connectionString));

            var optionsBuilder = new DbContextOptionsBuilder<TContext>();

            Console.WriteLine("DbContextFactory.Create(string): Connection string: {0}", connectionString);

            optionsBuilder.UseNpgsql(connectionString, x => x.MigrationsHistoryTable("_migration_history", "public"))
                .ReplaceService<IHistoryRepository, PostgreHistoryContext>();

            return CreateNewInstance(optionsBuilder.Options, _configuration);
        }
    }
}