using Microsoft.EntityFrameworkCore;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.Model.Context
{
    public class PlatformDBContext : DbContext
    {
        /// <summary>
        /// This context is designed to be used by Ofisim SaaS DB related operations. It uses by default "platform" database. For tenant operations do not use this context!
        /// instead use <see cref="TenantDBContext"/>
        /// </summary>
        public PlatformDBContext() { }

        public PlatformDBContext(DbContextOptions<PlatformDBContext> options) : base(options) { }

        public static PlatformDBContext Create()
        {
            return new PlatformDBContext();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CreateCustomModelMapping(modelBuilder);

            base.OnModelCreating(modelBuilder);
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
        }

        private void CreateCustomModelMapping(ModelBuilder modelBuilder)
        {
            //PlatformUser One to One and Cascade delete for TenantInfo
            modelBuilder.Entity<PlatformUser>()
                .HasOne(x => x.Setting)
                .WithOne(i => i.User)
                .HasForeignKey<PlatformUserSetting>(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //Tenant One to One and Cascade delete for TenantSetting
            modelBuilder.Entity<Tenant>()
                .HasOne(x => x.Setting)
                .WithOne(i => i.Tenant)
                .HasForeignKey<TenantSetting>(b => b.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            //Tenant One to One and Cascade delete for TenantLicense
            modelBuilder.Entity<Tenant>()
                .HasOne(x => x.License)
                .WithOne(i => i.Tenant)
                .HasForeignKey<TenantLicense>(b => b.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            //App One to One and Cascade delete for AppSetting
            modelBuilder.Entity<App>()
                .HasOne(x => x.Setting)
                .WithOne(i => i.App)
                .HasForeignKey<AppSetting>(b => b.AppId)
                .OnDelete(DeleteBehavior.Cascade);

            //TeamApps Many to Many
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

            //TeamUsers Many to Many
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

            //OrganizationUsers Many to Many
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

            //UserTenant Many to Many
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

            //Apps and Tenants One to Many 
            modelBuilder.Entity<App>()
               .HasMany(p => p.Tenants)
               .WithOne(i => i.App)
               .HasForeignKey(b => b.AppId);

            //Apps and EmailTemplates One to Many 
            modelBuilder.Entity<App>()
               .HasMany(p => p.Templates)
               .WithOne(i => i.App)
               .HasForeignKey(b => b.AppId);

            //Organization and Team One to Many
            modelBuilder.Entity<Team>()
                .HasOne(p => p.Organization)
                .WithMany(b => b.Teams)
                .HasForeignKey(p => p.OrganizationId);

            //Organization and Team One to Many
            modelBuilder.Entity<Tenant>()
                .HasOne(p => p.Owner)
                .WithMany(b => b.TenantsAsOwner)
                .HasForeignKey(p => p.OwnerId);

            //BaseEntity Tenant CreatedBy Relation. For Solving Error: Unable to determine the relationship represented by navigation property 'Tenant.CreatedBy' of type 'PlatformUser'.
            modelBuilder.Entity<Tenant>()
                .HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById);

            BuildIndexes(modelBuilder);
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
    }
}