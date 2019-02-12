using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Entities.Console;

namespace PrimeApps.Model.Context
{
    public class ConsoleDBContext : DbContext
    {
        public int? UserId { get; set; }

        public ConsoleDBContext(DbContextOptions<ConsoleDBContext> options) : base(options) { }

        public ConsoleDBContext(IConfiguration configuration)
        {
            Database.GetDbConnection().ConnectionString = configuration.GetConnectionString("ConsoleDBConnection");
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
            modelBuilder.Entity<AppDraft>()
                .HasOne(x => x.Setting)
                .WithOne(i => i.App)
                .HasForeignKey<AppDraftSetting>(b => b.AppId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeamUser>()
               .HasKey(t => new { t.UserId, t.TeamId });

            modelBuilder.Entity<TeamUser>()
                .HasOne(pt => pt.ConsoleUser)
                .WithMany(p => p.UserTeams)
                .HasForeignKey(pt => pt.UserId);

            modelBuilder.Entity<TeamUser>()
                .HasOne(pt => pt.Team)
                .WithMany(t => t.TeamUsers)
                .HasForeignKey(pt => pt.TeamId);

            modelBuilder.Entity<OrganizationUser>()
               .HasKey(t => new { t.UserId, t.OrganizationId });

            modelBuilder.Entity<OrganizationUser>()
                .HasOne(pt => pt.ConsoleUser)
                .WithMany(p => p.UserOrganizations)
                .HasForeignKey(pt => pt.UserId);

            modelBuilder.Entity<OrganizationUser>()
                .HasOne(pt => pt.Organization)
                .WithMany(t => t.OrganizationUsers)
                .HasForeignKey(pt => pt.OrganizationId);

            BuildIndexes(modelBuilder);
        }

        public void BuildIndexes(ModelBuilder modelBuilder)
        {
            //AppDraft
            modelBuilder.Entity<AppDraft>().HasIndex(x => x.Name);
            modelBuilder.Entity<AppDraft>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<AppDraft>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<AppDraft>().HasIndex(x => x.Deleted);

            //Templet
            modelBuilder.Entity<Templet>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Templet>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Templet>().HasIndex(x => x.Deleted);

            //TempletCategory
            modelBuilder.Entity<TempletCategory>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<TempletCategory>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<TempletCategory>().HasIndex(x => x.Deleted);

            //Organization
            modelBuilder.Entity<Organization>().HasIndex(x => x.Name);
            modelBuilder.Entity<Organization>().HasIndex(x => x.Label);
            modelBuilder.Entity<Organization>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Organization>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Organization>().HasIndex(x => x.Deleted);

            //OrganizationUser
            modelBuilder.Entity<OrganizationUser>().HasIndex(x => x.OrganizationId);
            modelBuilder.Entity<OrganizationUser>().HasIndex(x => x.UserId);

            //Team
            modelBuilder.Entity<Team>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Team>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Team>().HasIndex(x => x.Deleted);

            //TeamUser
            modelBuilder.Entity<TeamUser>().HasIndex(x => x.UserId);
            modelBuilder.Entity<TeamUser>().HasIndex(x => x.TeamId);

            //Deployment
            modelBuilder.Entity<Deployment>().HasIndex(x => x.AppId);
            modelBuilder.Entity<Deployment>().HasIndex(x => x.StartTime);
            modelBuilder.Entity<Deployment>().HasIndex(x => x.EndTime);
            modelBuilder.Entity<Deployment>().HasIndex(x => x.Status);
            

        }
        public DbSet<ConsoleUser> Users { get; set; }
        public DbSet<AppDraft> Apps { get; set; }
        public DbSet<AppDraftSetting> AppSettings { get; set; }
        public DbSet<Deployment> Deployments { get; set; }
        public DbSet<AppCollaborator> AppCollaborators { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationUser> OrganizationUsers { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamUser> TeamUsers { get; set; }
        public DbSet<Templet> Templets { get; set; }
        public DbSet<TempletCategory> TempletCategories { get; set; }
    }
}