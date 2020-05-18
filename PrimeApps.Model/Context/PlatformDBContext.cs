using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Context
{
    public class PlatformDBContext : DbContext
    {
        private IConfiguration _configuration;
        public int? UserId { get; set; }

        public PlatformDBContext(DbContextOptions<PlatformDBContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        public PlatformDBContext(IConfiguration configuration)
        {
            Database.GetDbConnection().ConnectionString = configuration.GetConnectionString("PlatformDBConnection");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CreateModelMapping(modelBuilder);
            modelBuilder.HasDefaultSchema("public");

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            SetDefaultValues();

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            SetDefaultValues();

            return base.SaveChangesAsync();
        }

        public int GetCurrentUserId()
        {
            return UserId ?? 0;
        }

        public void SetConnectionString(string connectionString, IConfiguration configuration)
        {
            var dbConnection = Database.GetDbConnection();

            if (dbConnection.State != System.Data.ConnectionState.Open)
                dbConnection.ConnectionString = Postgres.GetConnectionString(configuration.GetConnectionString("PlatformDBConnection"), "platform", connectionString);
        }

        private void SetDefaultValues()
        {
            var currentUserId = GetCurrentUserId();

            var entities = ChangeTracker.Entries().Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    if (((BaseEntity)entity.Entity).CreatedAt == DateTime.MinValue)
                        ((BaseEntity)entity.Entity).CreatedAt = DateTime.UtcNow;

                    if (((BaseEntity)entity.Entity).CreatedById < 1 && currentUserId > 0)
                        ((BaseEntity)entity.Entity).CreatedById = currentUserId;
                }
                else
                {
                    if (!((BaseEntity)entity.Entity).UpdatedAt.HasValue)
                        ((BaseEntity)entity.Entity).UpdatedAt = DateTime.UtcNow;

                    if (!((BaseEntity)entity.Entity).UpdatedById.HasValue && currentUserId > 0)
                        ((BaseEntity)entity.Entity).UpdatedById = currentUserId;
                }
            }
        }

        private void CreateModelMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PlatformUser>()
                .HasOne(x => x.Setting)
                .WithOne(i => i.User)
                .HasForeignKey<PlatformUserSetting>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tenant>()
                .HasOne(x => x.Setting)
                .WithOne(i => i.Tenant)
                .HasForeignKey<TenantSetting>(b => b.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tenant>()
                .HasOne(x => x.License)
                .WithOne(i => i.Tenant)
                .HasForeignKey<TenantLicense>(b => b.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<App>()
                .HasOne(x => x.Setting)
                .WithOne(i => i.App)
                .HasForeignKey<AppSetting>(b => b.AppId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserTenant>()
                .HasKey(t => new { t.UserId, t.TenantId });

            modelBuilder.Entity<UserTenant>()
                .HasOne(pt => pt.PlatformUser)
                .WithMany(p => p.TenantsAsUser)
                .HasForeignKey(pt => pt.UserId);

            modelBuilder.Entity<UserTenant>()
                .HasOne(pt => pt.Tenant)
                .WithMany(t => t.TenantUsers)
                .HasForeignKey(pt => pt.TenantId);

            modelBuilder.Entity<App>()
                .HasMany(p => p.Tenants)
                .WithOne(i => i.App)
                .HasForeignKey(b => b.AppId);

            modelBuilder.Entity<Tenant>()
                .HasOne(p => p.Owner)
                .WithMany(b => b.TenantsAsOwner)
                .HasForeignKey(p => p.OwnerId);

            modelBuilder.Entity<Tenant>()
                .HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById);

            BuildIndexes(modelBuilder);
        }

        public void BuildIndexes(ModelBuilder modelBuilder)
        {
            //PlatformUser
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.Id);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.Email);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.FirstName);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.LastName);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.UpdatedAt);

            //PlatformUserSettings
            modelBuilder.Entity<PlatformUserSetting>().HasIndex(x => x.Culture);
            modelBuilder.Entity<PlatformUserSetting>().HasIndex(x => x.Currency);
            modelBuilder.Entity<PlatformUserSetting>().HasIndex(x => x.Language);
            modelBuilder.Entity<PlatformUserSetting>().HasIndex(x => x.TimeZone);
            modelBuilder.Entity<PlatformUserSetting>().HasIndex(x => x.Phone);

            //App
            modelBuilder.Entity<App>().HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<App>().HasIndex(x => x.AppDraftId);
            modelBuilder.Entity<App>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<App>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<App>().HasIndex(x => x.Deleted);

            //AppTemplate
            modelBuilder.Entity<AppTemplate>().HasIndex(x => x.Name);
            modelBuilder.Entity<AppTemplate>().HasIndex(x => x.Type);
            modelBuilder.Entity<AppTemplate>().HasIndex(x => x.Language);
            modelBuilder.Entity<AppTemplate>().HasIndex(x => x.SystemCode);
            modelBuilder.Entity<AppTemplate>().HasIndex(x => x.Active);
            modelBuilder.Entity<AppTemplate>().HasIndex(x => new { x.AppId, x.SystemCode, x.Language }).IsUnique();

            //PlatformWarehouse
            modelBuilder.Entity<PlatformWarehouse>().HasIndex(x => x.DatabaseName);
            modelBuilder.Entity<PlatformWarehouse>().HasIndex(x => x.Completed);

            //Tenant
            modelBuilder.Entity<Tenant>().HasIndex(x => x.GuidId);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.OwnerId);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.Deleted);

            //TenantSetting
            modelBuilder.Entity<TenantSetting>().HasIndex(x => x.CustomDomain);

            //TenantLicense
            modelBuilder.Entity<TenantLicense>().HasIndex(x => x.IsPaidCustomer);
            modelBuilder.Entity<TenantLicense>().HasIndex(x => x.IsDeactivated);
            modelBuilder.Entity<TenantLicense>().HasIndex(x => x.IsSuspended);
            modelBuilder.Entity<TenantLicense>().HasIndex(x => x.DeactivatedAt);
            modelBuilder.Entity<TenantLicense>().HasIndex(x => x.SuspendedAt);

            //UserTenants
            modelBuilder.Entity<UserTenant>().HasIndex(x => x.UserId);
            modelBuilder.Entity<UserTenant>().HasIndex(x => x.TenantId);

            //Release
            modelBuilder.Entity<Release>().HasIndex(x => x.AppId);
            modelBuilder.Entity<Release>().HasIndex(x => x.TenantId);
            modelBuilder.Entity<Release>().HasIndex(x => x.StartTime);
            modelBuilder.Entity<Release>().HasIndex(x => x.EndTime);
            modelBuilder.Entity<Release>().HasIndex(x => x.Status);
            
            modelBuilder.Entity<AppSetting>()
                .Property(x => x.PicklistLanguage)
                .HasDefaultValue("en");
        }

        public DbSet<PlatformUser> Users { get; set; }
        public DbSet<PlatformUserSetting> UserSettings { get; set; }
        public DbSet<Release> Releases { get; set; }
        public DbSet<App> Apps { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<AppTemplate> AppTemplates { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantSetting> TenantSettings { get; set; }
        public DbSet<TenantLicense> TenantLicenses { get; set; }
        public DbSet<PlatformWarehouse> Warehouses { get; set; }
        public DbSet<UserTenant> UserTenants { get; set; }
    }
}