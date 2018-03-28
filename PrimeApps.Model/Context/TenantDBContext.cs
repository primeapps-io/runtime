using Microsoft.EntityFrameworkCore;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PrimeApps.Model.Context
{
    public partial class TenantDBContext : DbContext
    {
        private int? _tenantId;

        public int? TenantId
        {
            get { return _tenantId; }
            set { _tenantId = value; }
        }

        /// <summary>
        /// This context is to be used only for tenant database related operations. It includes all tenant related models. It must not be used for SaaS database specific operations. 
        /// Instead use <see cref="PlatformDBContext"/> 
        /// </summary>
        public TenantDBContext() : base()
        {
            //Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
            //Configuration.LazyLoadingEnabled = false;
            //Configuration.ProxyCreationEnabled = false;
        }
        public TenantDBContext(DbContextOptions options) : base(options)
        {

        }
        /// <summary>
        /// This context is to be used only for tenant database related operations. It includes all tenant related models. It must not be used for SaaS database specific operations. 
        /// Instead use <see cref="PlatformDBContext"/> 
        /// </summary>
        /// <param name="tenantId"></param>
        public TenantDBContext(int tenantId) : this()
        {
            _tenantId = tenantId;
            base.Database.GetDbConnection().ConnectionString = Postgres.GetConnectionString(tenantId);
        }

        /// <summary>
        /// This context is to be used only for tenant database related operations. It includes all tenant related models. It must not be used for SaaS database specific operations. 
        /// Instead use <see cref="PlatformDBContext"/> 
        /// </summary>
        /// <param name="databaseName"></param>
        public TenantDBContext(string databaseName) : this()
        {
            base.Database.GetDbConnection().ConnectionString = Postgres.GetConnectionString(databaseName);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CreateCustomModelMapping(modelBuilder);

            // PostgreSQL uses the public schema by default - not dbo.
            modelBuilder.HasDefaultSchema("public");

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {

            var validationErrors = ChangeTracker
       .Entries<IValidatableObject>()
       .SelectMany(e => e.Entity.Validate(null))
       .Where(r => r != ValidationResult.Success);

            if (validationErrors.Any())
            {
                // Possibly throw an exception here
                string errorMessages = string.Join("; ", validationErrors.Select(x => x.ErrorMessage));
                throw new Exception(errorMessages);

            }

            return base.SaveChanges();
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var validationErrors = ChangeTracker
        .Entries<IValidatableObject>()
        .SelectMany(e => e.Entity.Validate(null))
        .Where(r => r != ValidationResult.Success);

            if (validationErrors.Any())
            {
                // Possibly throw an exception here
                string errorMessages = string.Join("; ", validationErrors.Select(x => x.ErrorMessage));
                throw new Exception(errorMessages);
            }

            SetDefaultValues();
            return base.SaveChangesAsync();

        }

        public int GetCurrentUserId()
        {
            var claimsPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;

            if (claimsPrincipal == null)
                return _tenantId ?? 0;

            var claimsIdentity = (ClaimsIdentity)claimsPrincipal.Identity;
            var userIdClaim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == "user_id");

            return userIdClaim == null ? _tenantId ?? 0 : int.Parse(userIdClaim.Value);
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

        private void CreateCustomModelMapping(ModelBuilder modelBuilder)
        {
            //Many to many relationship users <-> user_groups
            modelBuilder.Entity<TenantUser>()
                .HasMany(s => s.Groups)
                .WithMany(c => c.Users)
                .Map(cs =>
                {
                    cs.MapLeftKey("user_id");
                    cs.MapRightKey("group_id");
                    cs.ToTable("users_user_groups");
                });
            
            //Cascade delete for FieldValidation
            modelBuilder.Entity<Field>()
                .HasOne(x=>x.Validation)
                .WithOne()
                .HasForeignKey(typeof(FieldValidation),"Validation")
                .OnDelete(DeleteBehavior.Cascade);

            //Cascade delete for FieldCombination
            /*modelBuilder.Entity<Field>()
                .HasOptional(x => x.Combination)
                .WithRequired(x => x.Field)
                .WillCascadeOnDelete(true);*/

	        modelBuilder.Entity<Field>()
		        .HasOne(x => x.Combination)
		        .WithOne()
		        .HasForeignKey(typeof(FieldCombination), "Combination")
		        .OnDelete(DeleteBehavior.Cascade);

			//Junction table for view-user shares
			modelBuilder.Entity<View>()
                .HasMany(x => x.Shares)
                .WithMany(x => x.SharedViews)
                .Map(x => x.MapLeftKey("view_id")
                .MapRightKey("user_id")
                .ToTable("view_shares"));

            //Cascade delete for WorkflowNotification
            /*modelBuilder.Entity<Workflow>()
                .HasOptional(x => x.SendNotification)
                .WithRequired(x => x.Workflow)
                .WillCascadeOnDelete(true);*/

	        modelBuilder.Entity<Workflow>()
		        .HasOne(x => x.SendNotification)
		        .WithOne()
		        .HasForeignKey(typeof(WorkflowNotification), "SendNotification")
		        .OnDelete(DeleteBehavior.Cascade);

			//Cascade delete for WorkflowTask
			/*modelBuilder.Entity<Workflow>()
                .HasOptional(x => x.CreateTask)
                .WithRequired(x => x.Workflow)
                .WillCascadeOnDelete(true);*/

	        modelBuilder.Entity<Workflow>()
		        .HasOne(x => x.CreateTask)
		        .WithOne()
		        .HasForeignKey(typeof(WorkflowTask), "CreateTask")
		        .OnDelete(DeleteBehavior.Cascade);

			//Cascade delete for WorkflowUpdate
			/*modelBuilder.Entity<Workflow>()
                .HasOptional(x => x.FieldUpdate)
                .WithRequired(x => x.Workflow)
                .WillCascadeOnDelete(true);*/

	        modelBuilder.Entity<Workflow>()
		        .HasOne(x => x.FieldUpdate)
		        .WithOne()
		        .HasForeignKey(typeof(WorkflowUpdate), "FieldUpdate")
		        .OnDelete(DeleteBehavior.Cascade);

			//Cascade delete for WorkflowWebHook
			/*modelBuilder.Entity<Workflow>()
                .HasOptional(x => x.WebHook)
                .WithRequired(x => x.Workflow)
                .WillCascadeOnDelete(true);*/

	        modelBuilder.Entity<Workflow>()
		        .HasOne(x => x.WebHook)
		        .WithOne()
		        .HasForeignKey(typeof(WorkflowWebhook), "WebHook")
		        .OnDelete(DeleteBehavior.Cascade);

			//Cascade delete profile permissions.
			/*modelBuilder.Entity<ProfilePermission>()
                .WithMany(x => x.Permissions)
				.OnDelete(DeleteBehavior.Cascade);*/

	        modelBuilder.Entity<ProfilePermission>()
		        .HasOne(x => x.Profile)
		        .WithMany(x => x.Permissions)
		        .OnDelete(DeleteBehavior.Cascade);

			//Note self referecing
			/*modelBuilder.Entity<Note>()
                .HasOptional(x => x.Parent)
                .WithMany(x => x.Notes)
                .HasForeignKey(x => x.NoteId)
                .WillCascadeOnDelete(true);*/

	        modelBuilder.Entity<Note>()
		        .HasOne(x => x.Parent)
				.WithMany()
		        .HasForeignKey(x => x.NoteId)
		        .OnDelete(DeleteBehavior.Cascade);

			//Junction table for analytic-user shares
			/*modelBuilder.Entity<Analytic>()
                .HasMany(x => x.Shares)
                .WithMany(x => x.SharedAnalytics)
                .Map(x => x.MapLeftKey("analytic_id")
                .MapRightKey("user_id")
                .ToTable("analytic_shares"));*/

			modelBuilder.Entity<AnalyticTenantUser>()
		        .HasKey(t => new { t.AnaltyicId, t.TenantUserId });

			modelBuilder.Entity<AnalyticTenantUser>()
		        .HasOne(pt => pt.Analytic)
		        .WithMany(p => p.Shares)
		        .HasForeignKey(pt => pt.AnaltyicId);

	        modelBuilder.Entity<AnalyticTenantUser>()
		        .HasOne(pt => pt.TenantUser)
		        .WithMany(t => t.SharedAnalytics)
		        .HasForeignKey(pt => pt.TenantUserId);
			

			//Junction table for template-user shares
			/*modelBuilder.Entity<Template>()
                .HasMany(x => x.Shares)
                .WithMany(x => x.SharedTemplates)
                .Map(x => x.MapLeftKey("template_id")
                .MapRightKey("user_id")
                .ToTable("template_shares"));*/

	        modelBuilder.Entity<TemplateTenantUser>()
		        .HasKey(t => new { t.TemplateId, t.TenantUserId });

	        modelBuilder.Entity<TemplateTenantUser>()
		        .HasOne(pt => pt.Template)
		        .WithMany(p => p.Shares)
		        .HasForeignKey(pt => pt.TemplateId);

	        modelBuilder.Entity<TemplateTenantUser>()
		        .HasOne(pt => pt.TenantUser)
		        .WithMany(t => t.SharedTemplates)
		        .HasForeignKey(pt => pt.TenantUserId);



			//Junction table for liked note shares
			/*modelBuilder.Entity<Note>()
                .HasMany(x => x.Likes)
                .WithMany(x => x.LikedNotes)
                .Map(x => x.MapLeftKey("note_id")
                .MapRightKey("user_id")
                .ToTable("note_likes"));*/


	        modelBuilder.Entity<NoteTenantUser>()
		        .HasKey(t => new { t.NoteId, t.TenantUserId });

	        modelBuilder.Entity<NoteTenantUser>()
		        .HasOne(pt => pt.Note)
		        .WithMany(p => p.Likes)
		        .HasForeignKey(pt => pt.NoteId);

	        modelBuilder.Entity<NoteTenantUser>()
		        .HasOne(pt => pt.TenantUser)
		        .WithMany(t => t.LikedNotes)
		        .HasForeignKey(pt => pt.TenantUserId);


			//Junction table for report-user shares
			modelBuilder.Entity<Report>()
                .HasMany(x => x.Shares)
                .WithMany(x => x.SharedReports)
                .Map(x => x.MapLeftKey("report_id")
                    .MapRightKey("user_id")
                    .ToTable("report_shares"));

	        modelBuilder.Entity<ReportTenantUser>()
		        .HasKey(t => new { t.ReportId, t.TenantUserId });

	        modelBuilder.Entity<ReportTenantUser>()
		        .HasOne(pt => pt.Report)
		        .WithMany(p => p.Shares)
		        .HasForeignKey(pt => pt.ReportId);

	        modelBuilder.Entity<ReportTenantUser>()
		        .HasOne(pt => pt.TenantUser)
		        .WithMany(t => t.SharedReports)
		        .HasForeignKey(pt => pt.TenantUserId);
		}

        public DbSet<TenantUser> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ProfilePermission> ProfilePermissions { get; set; }
        public DbSet<Calculation> Calculations { get; set; }
        public DbSet<Dependency> Dependencies { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<FieldCombination> FieldCombinations { get; set; }
        public DbSet<FieldValidation> FieldValidations { get; set; }
        public DbSet<FieldPermission> FieldPermissions { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Relation> Relations { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<SectionPermission> SectionPermissions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Picklist> Picklists { get; set; }
        public DbSet<PicklistItem> PicklistItems { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<View> Views { get; set; }
        public DbSet<ViewField> ViewFields { get; set; }
        public DbSet<ViewFilter> ViewFilters { get; set; }
        public DbSet<ViewState> ViewStates { get; set; }
        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<WorkflowFilter> WorkflowFilters { get; set; }
        public DbSet<WorkflowNotification> WorkflowNotifications { get; set; }
        public DbSet<WorkflowTask> WorkflowTasks { get; set; }
        public DbSet<WorkflowUpdate> WorkflowUpdates { get; set; }
        public DbSet<WorkflowLog> WorkflowLogs { get; set; }
        public DbSet<WorkflowWebhook> WorkflowWebHooks { get; set; }
        public DbSet<ActionButton> ActionButtons { get; set; }
        public DbSet<ConversionMapping> ConversionMappings { get; set; }
        public DbSet<ConversionSubModule> ConversionSubModules { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<Analytic> Analytics { get; set; }
        public DbSet<Import> Imports { get; set; }
        public DbSet<Dashlet> Dashlets { get; set; }
        public DbSet<Chart> Charts { get; set; }
        public DbSet<Widget> Widgets { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportField> ReportFields { get; set; }
        public DbSet<ReportFilter> ReportFilters { get; set; }
        public DbSet<ReportAggregation> ReportAggregations { get; set; }
        public DbSet<ChangeLog> ChangeLogs { get; set; }
        public DbSet<Process> Processes { get; set; }
        public DbSet<ProcessApprover> ProcessApprovers { get; set; }
        public DbSet<ProcessFilter> ProcessFilters { get; set; }
        public DbSet<ProcessLog> ProcessLogs { get; set; }
        public DbSet<ProcessRequest> ProcessRequests { get; set; }
        public DbSet<ReportCategory> ReportCategories { get; set; }
        public DbSet<UserCustomShare> UserCustomShares { get; set; }
        public DbSet<ModuleProfileSetting> ModuleProfileSettings { get; set; }
        public DbSet<Dashboard> Dashboards { get; set; }
        public DbSet<Help> Helps { get; set; }
        public DbSet<FieldFilter> FieldFilters { get; set; }
    }
}