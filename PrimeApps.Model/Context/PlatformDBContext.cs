﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.Model.Context
{
    public class PlatformDBContext : DbContext
    {
        public int? UserId { get; set; }

        public PlatformDBContext(DbContextOptions<PlatformDBContext> options) : base(options) { }

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

            modelBuilder.Entity<TeamApp>()
               .HasKey(t => new { t.AppId, t.TeamId });

            modelBuilder.Entity<TeamApp>()
                .HasOne(pt => pt.App)
                .WithMany(p => p.AppTeams)
                .HasForeignKey(pt => pt.AppId);

            modelBuilder.Entity<TeamApp>()
                .HasOne(pt => pt.Team)
                .WithMany(t => t.TeamApps)
                .HasForeignKey(pt => pt.TeamId);

            modelBuilder.Entity<TeamUser>()
               .HasKey(t => new { t.UserId, t.TeamId });

            modelBuilder.Entity<TeamUser>()
                .HasOne(pt => pt.PlatformUser)
                .WithMany(p => p.UserTeams)
                .HasForeignKey(pt => pt.TeamId);

            modelBuilder.Entity<TeamUser>()
                .HasOne(pt => pt.Team)
                .WithMany(t => t.TeamUsers)
                .HasForeignKey(pt => pt.UserId);

            modelBuilder.Entity<OrganizationUser>()
               .HasKey(t => new { t.UserId, t.OrganizationId });

            modelBuilder.Entity<OrganizationUser>()
                .HasOne(pt => pt.PlatformUser)
                .WithMany(p => p.UserOrganizations)
                .HasForeignKey(pt => pt.OrganizationId);

            modelBuilder.Entity<OrganizationUser>()
                .HasOne(pt => pt.Organization)
                .WithMany(t => t.OrganizationUsers)
                .HasForeignKey(pt => pt.UserId);

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

            modelBuilder.Entity<App>()
               .HasMany(p => p.Templates)
               .WithOne(i => i.App)
               .HasForeignKey(b => b.AppId);

            modelBuilder.Entity<Team>()
                .HasOne(p => p.Organization)
                .WithMany(b => b.Teams)
                .HasForeignKey(p => p.OrganizationId);

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
            modelBuilder.Entity<App>().HasIndex(x => x.Name);
            modelBuilder.Entity<App>().HasIndex(x => x.Label);
            modelBuilder.Entity<App>().HasIndex(x => x.Description);
            modelBuilder.Entity<App>().HasIndex(x => x.TemplateId);
            modelBuilder.Entity<App>().HasIndex(x => x.UseTenantSettings);
            modelBuilder.Entity<App>().HasIndex(x => x.CreatedById);
            modelBuilder.Entity<App>().HasIndex(x => x.UpdatedById);
            modelBuilder.Entity<App>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<App>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<App>().HasIndex(x => x.Deleted);

            //AppSetting
            modelBuilder.Entity<AppSetting>().HasIndex(x => x.AppId);
            modelBuilder.Entity<AppSetting>().HasIndex(x => x.Domain);
            modelBuilder.Entity<AppSetting>().HasIndex(x => x.MailSenderName);
            modelBuilder.Entity<AppSetting>().HasIndex(x => x.MailSenderEmail);
            modelBuilder.Entity<AppSetting>().HasIndex(x => x.Currency);
            modelBuilder.Entity<AppSetting>().HasIndex(x => x.Culture);
            modelBuilder.Entity<AppSetting>().HasIndex(x => x.Language);
            modelBuilder.Entity<AppSetting>().HasIndex(x => x.TimeZone);

            //AppTemplates
            modelBuilder.Entity<AppTemplate>().HasIndex(x => x.AppId);
            modelBuilder.Entity<AppTemplate>().HasIndex(x => x.Subject);
            modelBuilder.Entity<AppTemplate>().HasIndex(x => x.Type);
            modelBuilder.Entity<AppTemplate>().HasIndex(x => x.Language);
            modelBuilder.Entity<AppTemplate>().HasIndex(x => x.Name);
            modelBuilder.Entity<AppTemplate>().HasIndex(x => x.SystemCode);
            modelBuilder.Entity<AppTemplate>().HasIndex(x => x.MailSenderEmail);
            modelBuilder.Entity<AppTemplate>().HasIndex(x => x.MailSenderName);
            modelBuilder.Entity<AppTemplate>().HasIndex(x => x.Active);

            //ApiLog
            //App
            //ExchangeRate
            modelBuilder.Entity<ExchangeRate>().HasIndex(x => x.Date);
            modelBuilder.Entity<ExchangeRate>().HasIndex(x => x.Year);
            modelBuilder.Entity<ExchangeRate>().HasIndex(x => x.Month);
            modelBuilder.Entity<ExchangeRate>().HasIndex(x => x.Day);

            //PlatformWarehouse
            modelBuilder.Entity<PlatformWarehouse>().HasIndex(x => x.TenantId);
            modelBuilder.Entity<PlatformWarehouse>().HasIndex(x => x.DatabaseName);
            modelBuilder.Entity<PlatformWarehouse>().HasIndex(x => x.Completed);

            //Tenant
            modelBuilder.Entity<Tenant>().HasIndex(x => x.GuidId);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.OwnerId);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.AppId);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.UseUserSettings);

            modelBuilder.Entity<Tenant>().HasIndex(x => x.CreatedById);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.UpdatedById);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.Deleted);

            //TenantSetting
            modelBuilder.Entity<TenantSetting>().HasIndex(x => x.TenantId);
            modelBuilder.Entity<TenantSetting>().HasIndex(x => x.CustomDomain);
            modelBuilder.Entity<TenantSetting>().HasIndex(x => x.MailSenderName);
            modelBuilder.Entity<TenantSetting>().HasIndex(x => x.MailSenderEmail);
            modelBuilder.Entity<TenantSetting>().HasIndex(x => x.Culture);
            modelBuilder.Entity<TenantSetting>().HasIndex(x => x.Currency);
            modelBuilder.Entity<TenantSetting>().HasIndex(x => x.Language);
            modelBuilder.Entity<TenantSetting>().HasIndex(x => x.TimeZone);

            //TenantLicense
            modelBuilder.Entity<TenantLicense>().HasIndex(x => x.TenantId);
            modelBuilder.Entity<TenantLicense>().HasIndex(x => x.IsPaidCustomer);
            modelBuilder.Entity<TenantLicense>().HasIndex(x => x.IsDeactivated);
            modelBuilder.Entity<TenantLicense>().HasIndex(x => x.IsSuspended);
            modelBuilder.Entity<TenantLicense>().HasIndex(x => x.DeactivatedAt);
            modelBuilder.Entity<TenantLicense>().HasIndex(x => x.SuspendedAt);

            //Organization
            modelBuilder.Entity<Organization>().HasIndex(x => x.Id);
            modelBuilder.Entity<Organization>().HasIndex(x => x.Name);
            modelBuilder.Entity<Organization>().HasIndex(x => x.Label);
            /*modelBuilder.Entity<Organization>().HasIndex(x => x.Owner);*/

            modelBuilder.Entity<Organization>().HasIndex(x => x.CreatedById);
            modelBuilder.Entity<Organization>().HasIndex(x => x.UpdatedById);
            modelBuilder.Entity<Organization>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Organization>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Organization>().HasIndex(x => x.Deleted);

            //Team
            modelBuilder.Entity<Team>().HasIndex(x => x.Id);
            modelBuilder.Entity<Team>().HasIndex(x => x.Name);
            modelBuilder.Entity<Team>().HasIndex(x => x.OrganizationId);

            modelBuilder.Entity<Team>().HasIndex(x => x.CreatedById);
            modelBuilder.Entity<Team>().HasIndex(x => x.UpdatedById);
            modelBuilder.Entity<Team>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Team>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Team>().HasIndex(x => x.Deleted);

            //UserTenants
            modelBuilder.Entity<UserTenant>().HasIndex(x => x.UserId);
            modelBuilder.Entity<UserTenant>().HasIndex(x => x.TenantId);

            //TeamApps
            modelBuilder.Entity<TeamApp>().HasIndex(x => x.AppId);
            modelBuilder.Entity<TeamApp>().HasIndex(x => x.TeamId);

            //TeamUsers
            modelBuilder.Entity<TeamUser>().HasIndex(x => x.UserId);
            modelBuilder.Entity<TeamUser>().HasIndex(x => x.TeamId);

            //OrganizationUsers
            modelBuilder.Entity<OrganizationUser>().HasIndex(x => x.UserId);
            modelBuilder.Entity<OrganizationUser>().HasIndex(x => x.OrganizationId);

            //AppWorkflow
            modelBuilder.Entity<AppWorkflow>().HasIndex(x => x.Active);
            modelBuilder.Entity<AppWorkflow>().HasIndex(x => x.AppId);

            //AppWorkflowLog
            modelBuilder.Entity<AppWorkflowLog>().HasIndex(x => x.AppWorkflowId);
            modelBuilder.Entity<AppWorkflowLog>().HasIndex(x => x.AppId);
        }

        public DbSet<PlatformUser> Users { get; set; }
        public DbSet<PlatformUserSetting> UserSettings { get; set; }
        public DbSet<App> Apps { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<AppTemplate> AppTemplates { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<TenantSetting> TenantSettings { get; set; }
        public DbSet<TenantLicense> TenantLicenses { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamUser> TeamUsers { get; set; }
        public DbSet<TeamApp> TeamApps { get; set; }
        public DbSet<OrganizationUser> OrganizationUsers { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<PlatformWarehouse> Warehouses { get; set; }
        public DbSet<UserTenant> UserTenants { get; set; }
        public DbSet<AppWorkflow> AppWorkflows { get; set; }
        public DbSet<AppWorkflowLog> AppWorkflowLogs { get; set; }
        public DbSet<AppWorkflowWebhook> AppWorkflowWebhooks { get; set; }
    }
}