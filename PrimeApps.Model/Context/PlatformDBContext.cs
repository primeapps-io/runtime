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
        public PlatformDBContext(DbContextOptions options) : base(options)
        {
        }

        public static PlatformDBContext Create()
        {
            return new PlatformDBContext();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // PostgreSQL uses the public schema by default - not dbo.
            modelBuilder.HasDefaultSchema("public");

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