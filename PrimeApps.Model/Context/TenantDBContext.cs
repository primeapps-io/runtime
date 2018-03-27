using Microsoft.EntityFrameworkCore;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Helpers;
using System;
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
            try
            {
                SetDefaultValues();
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(errorMessages);
            }
        }

        public override Task<int> SaveChangesAsync()
        {
            try
            {
                SetDefaultValues();
                return base.SaveChangesAsync();
            }
            catch (DbEntityValidationException ex)
            {
                string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(errorMessages);
            }
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

        private void CreateCustomModelMapping(DbModelBuilder modelBuilder)
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
                .HasOptional(x => x.Validation)
                .WithRequired(x => x.Field)
                .WillCascadeOnDelete(true);

            //Cascade delete for FieldCombination
            modelBuilder.Entity<Field>()
                .HasOptional(x => x.Combination)
                .WithRequired(x => x.Field)
                .WillCascadeOnDelete(true);

            //Junction table for view-user shares
            modelBuilder.Entity<View>()
                .HasMany(x => x.Shares)
                .WithMany(x => x.SharedViews)
                .Map(x => x.MapLeftKey("view_id")
                .MapRightKey("user_id")
                .ToTable("view_shares"));

            //Cascade delete for WorkflowNotification
            modelBuilder.Entity<Workflow>()
                .HasOptional(x => x.SendNotification)
                .WithRequired(x => x.Workflow)
                .WillCascadeOnDelete(true);

            //Cascade delete for WorkflowTask
            modelBuilder.Entity<Workflow>()
                .HasOptional(x => x.CreateTask)
                .WithRequired(x => x.Workflow)
                .WillCascadeOnDelete(true);

            //Cascade delete for WorkflowUpdate
            modelBuilder.Entity<Workflow>()
                .HasOptional(x => x.FieldUpdate)
                .WithRequired(x => x.Workflow)
                .WillCascadeOnDelete(true);

            //Cascade delete for WorkflowWebHook
            modelBuilder.Entity<Workflow>()
                .HasOptional(x => x.WebHook)
                .WithRequired(x => x.Workflow)
                .WillCascadeOnDelete(true);

            //Cascade delete profile permissions.
            modelBuilder.Entity<ProfilePermission>()
                .HasRequired(x => x.Profile)
                .WithMany(x => x.Permissions)
                .WillCascadeOnDelete(true);

            //Note self referecing
            modelBuilder.Entity<Note>()
                .HasOptional(x => x.Parent)
                .WithMany(x => x.Notes)
                .HasForeignKey(x => x.NoteId)
                .WillCascadeOnDelete(true);

            //Junction table for analytic-user shares
            modelBuilder.Entity<Analytic>()
                .HasMany(x => x.Shares)
                .WithMany(x => x.SharedAnalytics)
                .Map(x => x.MapLeftKey("analytic_id")
                .MapRightKey("user_id")
                .ToTable("analytic_shares"));

            //Junction table for template-user shares
            modelBuilder.Entity<Template>()
                .HasMany(x => x.Shares)
                .WithMany(x => x.SharedTemplates)
                .Map(x => x.MapLeftKey("template_id")
                .MapRightKey("user_id")
                .ToTable("template_shares"));

            //Junction table for liked note shares
            modelBuilder.Entity<Note>()
                .HasMany(x => x.Likes)
                .WithMany(x => x.LikedNotes)
                .Map(x => x.MapLeftKey("note_id")
                .MapRightKey("user_id")
                .ToTable("note_likes"));

            //Junction table for report-user shares
            modelBuilder.Entity<Report>()
                .HasMany(x => x.Shares)
                .WithMany(x => x.SharedReports)
                .Map(x => x.MapLeftKey("report_id")
                    .MapRightKey("user_id")
                    .ToTable("report_shares"));
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