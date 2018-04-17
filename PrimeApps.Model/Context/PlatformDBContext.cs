using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Context
{
    public class PlatformDBContext : DbContext
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
			CreateCustomModelMapping(modelBuilder);

			base.OnModelCreating(modelBuilder);


			//TODO Removed
			/*modelBuilder.Entity<PlatformUser>().ToTable("users");
            modelBuilder.Entity<PlatformUser>().Property(p => p.Email).HasMaxLength(510);
            modelBuilder.Entity<PlatformUser>().Property(p => p.Id).ValueGeneratedOnAdd();*/

			/* modelBuilder.Entity<ApplicationRole>().ToTable("roles");
			 modelBuilder.Entity<ApplicationRole>().Property(x => x.Id).HasColumnName("id");
			 modelBuilder.Entity<ApplicationRole>().Property(x => x.Name).HasColumnName("name");*/

			/*modelBuilder.Entity<ApplicationUserRole>().ToTable("user_roles");
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
            modelBuilder.Entity<ApplicationUserLogin>().Property(x => x.UserId).HasColumnName("user_id");*/

			//BuildIndexes(modelBuilder);
		}

		public void BuildIndexes(ModelBuilder modelBuilder)
        {
			//modelBuilder.Entity<Entities.Platform.PlatformWarehouse>().ToTable("warehouse");
			//modelBuilder.Entity<PlatformUser>().ToTable("users");

			//PlatformUser
			modelBuilder.Entity<PlatformUser>().HasIndex(x => x.Id);
			//TODO Removed
			/*modelBuilder.Entity<PlatformUser>().HasIndex(x => x.AppId);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.TenantId);*/
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.Email);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.FirstName);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.LastName);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.Currency);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.Culture);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.ActiveDirectoryEmail);
            modelBuilder.Entity<PlatformUser>().HasIndex(x => x.ActiveDirectoryTenantId);


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

			//TenantInfo
			modelBuilder.Entity<TenantInfo>().HasIndex(x => x.Language);
			modelBuilder.Entity<TenantInfo>().HasIndex(x => x.CustomDomain);
			modelBuilder.Entity<TenantInfo>().HasIndex(x => x.MailSenderEmail);
			modelBuilder.Entity<TenantInfo>().HasIndex(x => x.CustomTitle);

			//LicenceInfo
			modelBuilder.Entity<TenantSettings>().HasIndex(x => x.UserLicenseCount);
			modelBuilder.Entity<TenantSettings>().HasIndex(x => x.ModuleLicenseCount);
			modelBuilder.Entity<TenantSettings>().HasIndex(x => x.AnalyticsLicenseCount);
			modelBuilder.Entity<TenantSettings>().HasIndex(x => x.IsPaidCustomer);
			modelBuilder.Entity<TenantSettings>().HasIndex(x => x.IsDeactivated);
			modelBuilder.Entity<TenantSettings>().HasIndex(x => x.IsSuspended);
			modelBuilder.Entity<TenantSettings>().HasIndex(x => x.DeactivatedAt);
			modelBuilder.Entity<TenantSettings>().HasIndex(x => x.SuspendedAt);


			//UserApp
			//TODO Removed
			/*modelBuilder.Entity<UserApp>().HasIndex(x => x.UserId);
            modelBuilder.Entity<UserApp>().HasIndex(x => x.TenantId);
            modelBuilder.Entity<UserApp>().HasIndex(x => x.MainTenantId);
            modelBuilder.Entity<UserApp>().HasIndex(x => x.Email);
            modelBuilder.Entity<UserApp>().HasIndex(x => x.Active);
            modelBuilder.Entity<UserApp>().HasIndex(x => x.AppId);*/

			//Organization
			modelBuilder.Entity<Organization>().HasIndex(x => x.Id);
			modelBuilder.Entity<Organization>().HasIndex(x => x.Name);
			modelBuilder.Entity<Organization>().HasIndex(x => x.Owner);


			//Team
			modelBuilder.Entity<Team>().HasIndex(x => x.Id);
			modelBuilder.Entity<Team>().HasIndex(x => x.Name);
			modelBuilder.Entity<Team>().HasIndex(x => x.OrganizationId);
			modelBuilder.Entity<Team>().HasIndex(x => x.Owner);

			//UserTenants
			modelBuilder.Entity<UserTenants>().HasIndex(x => x.UserId);
			modelBuilder.Entity<UserTenants>().HasIndex(x => x.TenantId);

			//TeamApps
			modelBuilder.Entity<TeamApps>().HasIndex(x => x.AppId);
			modelBuilder.Entity<TeamApps>().HasIndex(x => x.TeamId);

			//TeamUsers
			modelBuilder.Entity<TeamUsers>().HasIndex(x => x.UserId);
			modelBuilder.Entity<TeamUsers>().HasIndex(x => x.TeamId);

			//OrganizationUsers
			modelBuilder.Entity<OrganizationUsers>().HasIndex(x => x.UserId);
			modelBuilder.Entity<OrganizationUsers>().HasIndex(x => x.OrganizationId);
		}

		private void CreateCustomModelMapping(ModelBuilder modelBuilder)
		{
			//TenantInfo One to One and Cascade delete for TenantInfo
			modelBuilder.Entity<Tenant>()
				.HasOne(x => x.Info)
				.WithOne(i => i.Tenant)
				.HasForeignKey<TenantInfo>(b => b.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			//TenantSetting One to One and Cascade delete for TenantSettings
			modelBuilder.Entity<Tenant>()
				.HasOne(x => x.Settings)
				.WithOne(i => i.Tenant)
				.HasForeignKey<TenantSettings>(b => b.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			//TenantInfo One to One and Cascade delete for TenantInfo
			modelBuilder.Entity<App>()
				.HasOne(x => x.Info)
				.WithOne(i => i.App)
				.HasForeignKey<AppInfo>(b => b.AppId)
				.OnDelete(DeleteBehavior.Cascade);
			
			//TeamApps Many to Many
			modelBuilder.Entity<TeamApps>()
			   .HasKey(t => new { t.AppId, t.TeamId});

			modelBuilder.Entity<TeamApps>()
				.HasOne(pt => pt.App)
				.WithMany(p => p.Teams)
				.HasForeignKey(pt => pt.AppId);

			modelBuilder.Entity<TeamApps>()
				.HasOne(pt => pt.Team)
				.WithMany(t => t.Apps)
				.HasForeignKey(pt => pt.TeamId);

			//TeamUsers Many to Many
			modelBuilder.Entity<TeamUsers>()
			   .HasKey(t => new { t.UserId, t.TeamId });

			modelBuilder.Entity<TeamUsers>()
				.HasOne(pt => pt.PlatformUser)
				.WithMany(p => p.Teams)
				.HasForeignKey(pt => pt.TeamId);

			modelBuilder.Entity<TeamUsers>()
				.HasOne(pt => pt.Team)
				.WithMany(t => t.Users)
				.HasForeignKey(pt => pt.UserId);

			//OrganizationUsers Many to Many
			modelBuilder.Entity<OrganizationUsers>()
			   .HasKey(t => new { t.UserId, t.OrganizationId });

			modelBuilder.Entity<OrganizationUsers>()
				.HasOne(pt => pt.PlatformUser)
				.WithMany(p => p.Organizations)
				.HasForeignKey(pt => pt.OrganizationId);

			modelBuilder.Entity<OrganizationUsers>()
				.HasOne(pt => pt.Organization)
				.WithMany(t => t.Users)
				.HasForeignKey(pt => pt.UserId);

			//UserTenants Many to Many
			modelBuilder.Entity<UserTenants>()
			   .HasKey(t => new { t.UserId, t.TenantId });

			modelBuilder.Entity<UserTenants>()
				.HasOne(pt => pt.PlatformUser)
				.WithMany(p => p.Tenants)
				.HasForeignKey(pt => pt.TenantId);

			modelBuilder.Entity<UserTenants>()
				.HasOne(pt => pt.Tenant)
				.WithMany(t => t.Users)
				.HasForeignKey(pt => pt.UserId);

			//Apps and Tenants One to Many 
			modelBuilder.Entity<App>()
			   .HasMany(p => p.Tenants)
			   .WithOne(i => i.App)
			   .HasForeignKey(b => b.AppId);

			//Organization and Team One to Many 
			modelBuilder.Entity<Organization>()
			   .HasMany(p => p.Teams)
			   .WithOne(i => i.Organization)
			   .HasForeignKey(b => b.OrganizationId);


			BuildIndexes(modelBuilder);
		}

		//TODO Removed
		//public DbSet<Client> Clients { get; set; }

		//TODO Removed
		//public DbSet<RefreshToken> RefreshTokens { get; set; }
		public DbSet<PlatformUser> Users { get; set; }
		public DbSet<ActiveDirectoryTenant> ActiveDirectoryTenants { get; set; }
        public DbSet<ActiveDirectoryCache> ActiveDirectoryCache { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Organization> Organization { get; set; }
		public DbSet<TenantInfo> TenantInfo { get; set; }
		public DbSet<TenantSettings> TenantSettings { get; set; }
		public DbSet<Team> Team { get; set; }
		public DbSet<TeamUsers> TeamUsers { get; set; }
		public DbSet<TeamApps> TeamApps { get; set; }
		public DbSet<OrganizationUsers> OrganizationUsers { get; set; }	
		//public DbSet<ApiLog> ApiLogs { get; set; } // TODO: Refactor with .net core and postgresql json
		public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<App> Apps { get; set; }
        public DbSet<PlatformWarehouse> Warehouses { get; set; }
		//TODO Removed
		//public DbSet<UserApp> UserApps { get; set; }
		public DbSet<UserTenants> UserTenants { get; set; }

    }
}