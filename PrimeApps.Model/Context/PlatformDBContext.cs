using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Entities.Platform.Identity;
using PrimeApps.Model.Helpers;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Context
{
    public class PlatformDBContext : IdentityDbContext<PlatformUser, ApplicationRole, int>
    {
        /// <summary>
        /// This context is designed to be used by Ofisim SaaS DB related operations. It uses by default "ofisim" database. For tenant operations do not use this context!
        /// instead use <see cref="TenantDBContext"/>
        /// </summary>
        public PlatformDBContext() : base()
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        public PlatformDBContext(DbContextOptions<PlatformDBContext> options) : base(options)
        {
        }

        public static PlatformDBContext Create()
        {
            return new PlatformDBContext();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PlatformUser>().ToTable("users");
            modelBuilder.Entity<PlatformUser>().Property(p => p.Email).HasMaxLength(510);
            modelBuilder.Entity<PlatformUser>().Property(p => p.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<Entities.Platform.PlatformWarehouse>().ToTable("warehouse");

            modelBuilder.Entity<ApplicationRole>().ToTable("roles");
            modelBuilder.Entity<ApplicationRole>().Property(x => x.Id).HasColumnName("id");
            modelBuilder.Entity<ApplicationRole>().Property(x => x.Name).HasColumnName("name");

            modelBuilder.Entity<ApplicationUserRole>().ToTable("user_roles");
            modelBuilder.Entity<ApplicationUserRole>().Property(x => x.RoleId).HasColumnName("role_id");
            modelBuilder.Entity<ApplicationUserRole>().Property(x => x.UserId).HasColumnName("user_id");

            modelBuilder.Entity<ApplicationRoleClaim>().ToTable("user_claims");
            modelBuilder.Entity<ApplicationRoleClaim>().Property(x => x.Id).HasColumnName("id");
            modelBuilder.Entity<ApplicationRoleClaim>().Property(x => x.UserId).HasColumnName("user_id");
            modelBuilder.Entity<ApplicationRoleClaim>().Property(x => x.ClaimValue).HasColumnName("claim_value");
            modelBuilder.Entity<ApplicationRoleClaim>().Property(x => x.ClaimType).HasColumnName("claim_type");

            modelBuilder.Entity<ApplicationUserLogin>().ToTable("user_logins");
            modelBuilder.Entity<ApplicationUserLogin>().Property(x => x.LoginProvider).HasColumnName("login_provider");
            modelBuilder.Entity<ApplicationUserLogin>().Property(x => x.ProviderKey).HasColumnName("provider_key");
            modelBuilder.Entity<ApplicationUserLogin>().Property(x => x.UserId).HasColumnName("user_id");

            BuildIndexes(modelBuilder);
        }

        public void BuildIndexes(ModelBuilder modelBuilder)
        {
            //PlatformUser
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.Id);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.AppId);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.TenantId);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.Email);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.FirstName);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.LastName);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.EmailConfirmed);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.PasswordHash);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.PhoneNumberConfirmed);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.TwoFactorEnabled);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.LockoutEnd);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.LockoutEnabled);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.AccessFailedCount);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.Currency);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.ActiveDirectoryEmail);


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
            modelBuilder.Entity<Tenant>().HasIndex(x => x.Language);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.HasSampleData);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.HasAnalytics);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.HasPhone);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.CustomDomain);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.MailSenderEmail);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.CustomTitle);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.UserLicenseCount);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.ModuleLicenseCount);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.HasAnalyticsLicense);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.IsPaidCustomer);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.IsDeactivated);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.IsSuspended);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.DeactivatedAt);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.SuspendedAt);
            modelBuilder.Entity<Tenant>().HasIndex(x => x.OwnerId);

            //UserApp
            modelBuilder.Entity<UserApp>().HasIndex(x => x.UserId);
            modelBuilder.Entity<UserApp>().HasIndex(x => x.TenantId);
            modelBuilder.Entity<UserApp>().HasIndex(x => x.MainTenantId);
            modelBuilder.Entity<UserApp>().HasIndex(x => x.Email);
            modelBuilder.Entity<UserApp>().HasIndex(x => x.Active);
            modelBuilder.Entity<UserApp>().HasIndex(x => x.AppId);

        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<ActiveDirectoryTenant> ActiveDirectoryTenants { get; set; }
        public DbSet<ActiveDirectoryCache> ActiveDirectoryCache { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        //public DbSet<ApiLog> ApiLogs { get; set; } // TODO: Refactor with .net core and postgresql json
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<App> Apps { get; set; }
        public DbSet<PlatformWarehouse> Warehouses { get; set; }
        public DbSet<UserApp> UserApps { get; set; }
    }
}