using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PrimeApps.Model.Context
{
    public class TenantDBContext : DbContext
    {
        public int? TenantId { get; set; }

        public int? UserId { get; set; }

        public TenantDBContext(DbContextOptions<TenantDBContext> options) : base(options) { }

        public TenantDBContext(int tenantId, IConfiguration configuration)
        {
            TenantId = tenantId;
            Database.GetDbConnection().ConnectionString = Postgres.GetConnectionString(configuration.GetConnectionString("TenantDBConnection"), tenantId);
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

            return base.SaveChangesAsync(cancellationToken);
        }

        public int GetCurrentUserId()
        {
            return UserId ?? (TenantId ?? 0);
        }

        public void SetCustomConnectionDatabaseName(string databaseName)
        {
            Database.GetDbConnection().ConnectionString = Postgres.GetConnectionString(Database.GetDbConnection().ConnectionString, databaseName);
        }

        private void SetDefaultValues()
        {
            int currentUserId = GetCurrentUserId();

            System.Collections.Generic.IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> entities = ChangeTracker.Entries().Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    if (((BaseEntity)entity.Entity).CreatedAt == DateTime.MinValue)
                    {
                        ((BaseEntity)entity.Entity).CreatedAt = DateTime.UtcNow;
                    }

                    if (((BaseEntity)entity.Entity).CreatedById < 1 && currentUserId > 0)
                    {
                        ((BaseEntity)entity.Entity).CreatedById = currentUserId;
                    }
                }
                else
                {
                    if (!((BaseEntity)entity.Entity).UpdatedAt.HasValue)
                    {
                        ((BaseEntity)entity.Entity).UpdatedAt = DateTime.UtcNow;
                    }

                    if (!((BaseEntity)entity.Entity).UpdatedById.HasValue && currentUserId > 0)
                    {
                        ((BaseEntity)entity.Entity).UpdatedById = currentUserId;
                    }
                }
            }
        }

        private void CreateModelMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UsersUserGroup>()
                .HasKey(ug => new { ug.UserId, ug.UserGroupId });

            modelBuilder.Entity<UsersUserGroup>()
                .HasOne(ug => ug.User)
                .WithMany(u => u.Groups)
                .HasForeignKey(bc => bc.UserId);

            modelBuilder.Entity<UsersUserGroup>()
                .HasOne(ug => ug.UserGroup)
                .WithMany(g => g.Users)
                .HasForeignKey(bc => bc.UserGroupId);

            modelBuilder.Entity<Field>()
                .HasOne(x => x.Validation)
                .WithOne(x => x.Field)
                .HasForeignKey<FieldValidation>(x => x.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Field>()
                .HasOne(x => x.Combination)
                .WithOne(x => x.Field)
                .HasForeignKey<FieldCombination>(b => b.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ViewShares>()
                .HasKey(vs => new { vs.UserId, vs.ViewId });

            modelBuilder.Entity<ViewShares>()
                .HasOne(vs => vs.User)
                .WithMany(u => u.SharedViews)
                .HasForeignKey(vs => vs.UserId);

            modelBuilder.Entity<ViewShares>()
                .HasOne(vs => vs.View)
                .WithMany(g => g.Shares)
                .HasForeignKey(vs => vs.ViewId);

            modelBuilder.Entity<Workflow>()
                .HasOne(x => x.SendNotification)
                .WithOne(x => x.Workflow)
                .HasForeignKey<WorkflowNotification>(x => x.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Workflow>()
                .HasOne(x => x.CreateTask)
                .WithOne(x => x.Workflow)
                .HasForeignKey<WorkflowTask>(x => x.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Workflow>()
                .HasOne(x => x.FieldUpdate)
                .WithOne(x => x.Workflow)
                .HasForeignKey<WorkflowUpdate>(x => x.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Workflow>()
                .HasOne(x => x.WebHook)
                .WithOne(x => x.Workflow)
                .HasForeignKey<WorkflowWebhook>(x => x.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Profile>()
                .HasMany(x => x.Users)
                .WithOne(x => x.Profile)
                .HasForeignKey(x => x.ProfileId);

            modelBuilder.Entity<Role>()
                .HasMany(x => x.Users)
                .WithOne(x => x.Role)
                .HasForeignKey(x => x.RoleId);

            modelBuilder.Entity<ProfilePermission>()
                .HasOne(x => x.Profile)
                .WithMany(x => x.Permissions)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Note>()
                .HasOne(x => x.Parent)
                .WithMany(x => x.Notes)
                .HasForeignKey(x => x.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AnalyticShares>()
                .HasKey(t => new { t.UserId, t.AnaltyicId });

            modelBuilder.Entity<AnalyticShares>()
                .HasOne(pt => pt.Analytic)
                .WithMany(p => p.Shares)
                .HasForeignKey(pt => pt.AnaltyicId);

            modelBuilder.Entity<AnalyticShares>()
                .HasOne(pt => pt.TenantUser)
                .WithMany(t => t.SharedAnalytics)
                .HasForeignKey(pt => pt.UserId);

            modelBuilder.Entity<TemplateShares>()
                .HasKey(t => new { t.UserId, t.TemplateId });/*We must ensure the primary key constraint names are matching*/

            modelBuilder.Entity<TemplateShares>()
                .HasOne(pt => pt.Template)
                .WithMany(p => p.Shares)
                .HasForeignKey(pt => pt.TemplateId);

            modelBuilder.Entity<TemplateShares>()
                .HasOne(pt => pt.TenantUser)
                .WithMany(t => t.SharedTemplates)
                .HasForeignKey(pt => pt.UserId);

            modelBuilder.Entity<NoteLikes>()
                .HasKey(t => new { t.UserId, t.NoteId });

            modelBuilder.Entity<NoteLikes>()
                .HasOne(pt => pt.Note)
                .WithMany(p => p.NoteLikes)
                .HasForeignKey(pt => pt.NoteId);

            modelBuilder.Entity<NoteLikes>()
                .HasOne(pt => pt.TenantUser)
                .WithMany(t => t.LikedNotes)
                .HasForeignKey(pt => pt.UserId);

            modelBuilder.Entity<ReportShares>()
                .HasKey(t => new { t.UserId, t.ReportId });

            modelBuilder.Entity<ReportShares>()
                .HasOne(pt => pt.Report)
                .WithMany(p => p.Shares)
                .HasForeignKey(pt => pt.ReportId);

            modelBuilder.Entity<ReportShares>()
                .HasOne(pt => pt.TenantUser)
                .WithMany(t => t.SharedReports)
                .HasForeignKey(pt => pt.UserId);

            modelBuilder.Entity<Profile>()
                .HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById);

            BuildIndexes(modelBuilder);
        }

        public void BuildIndexes(ModelBuilder modelBuilder)
        {
            //ActionButton
            modelBuilder.Entity<ActionButton>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<ActionButton>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<ActionButton>().HasIndex(x => x.Deleted);

            //ActionButtonPermission
            modelBuilder.Entity<ActionButtonPermission>().HasIndex(x => new { x.ActionButtonId, x.ProfileId }).IsUnique().HasName("action_button_permissions_IX_action_button_id_profile_id");
            modelBuilder.Entity<ActionButtonPermission>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<ActionButtonPermission>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<ActionButtonPermission>().HasIndex(x => x.Deleted);

            //Analytics
            modelBuilder.Entity<Analytic>().HasIndex(x => x.PowerBiReportId);
            modelBuilder.Entity<Analytic>().HasIndex(x => x.SharingType);
            modelBuilder.Entity<Analytic>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Analytic>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Analytic>().HasIndex(x => x.Deleted);

            //AuditLog
            modelBuilder.Entity<AuditLog>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<AuditLog>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<AuditLog>().HasIndex(x => x.Deleted);

            //Calculation
            modelBuilder.Entity<Calculation>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Calculation>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Calculation>().HasIndex(x => x.Deleted);

            //Chart
            modelBuilder.Entity<Chart>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Chart>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Chart>().HasIndex(x => x.Deleted);

            //Components
            modelBuilder.Entity<Components>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Components>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Components>().HasIndex(x => x.Deleted);

            //ConversionMapping
            modelBuilder.Entity<ConversionMapping>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<ConversionMapping>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<ConversionMapping>().HasIndex(x => x.Deleted);

            //ConversionSubModules
            modelBuilder.Entity<ConversionSubModule>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<ConversionSubModule>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<ConversionSubModule>().HasIndex(x => x.Deleted);

            //Dashboard
            modelBuilder.Entity<Dashboard>().HasIndex(x => x.SharingType);
            modelBuilder.Entity<Dashboard>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Dashboard>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Dashboard>().HasIndex(x => x.Deleted);

            //Dashlet
            modelBuilder.Entity<Dashlet>().HasIndex(x => x.ChartId);
            modelBuilder.Entity<Dashlet>().HasIndex(x => x.WidgetId);
            modelBuilder.Entity<Dashlet>().HasIndex(x => x.DashboardId);
            modelBuilder.Entity<Dashlet>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Dashlet>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Dashlet>().HasIndex(x => x.Deleted);

            //Dependency
            modelBuilder.Entity<Dependency>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Dependency>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Dependency>().HasIndex(x => x.Deleted);

            //Document
            modelBuilder.Entity<Document>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Document>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Document>().HasIndex(x => x.Deleted);

            //Field
            modelBuilder.Entity<Field>().HasIndex(x => new { x.ModuleId, x.Name }).HasName("fields_IX_module_id_name").IsUnique();
            modelBuilder.Entity<Field>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Field>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Field>().HasIndex(x => x.Deleted);

            //FieldCombination
            //FieldFilter
            //FieldPermission
            modelBuilder.Entity<FieldPermission>().HasIndex(x => new { x.FieldId, x.ProfileId }).HasName("field_permissions_IX_field_id_profile_id");
            modelBuilder.Entity<FieldPermission>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<FieldPermission>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<FieldPermission>().HasIndex(x => x.Deleted);

            //Help
            modelBuilder.Entity<Help>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Help>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Help>().HasIndex(x => x.Deleted);

            //Import
            modelBuilder.Entity<Import>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Import>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Import>().HasIndex(x => x.Deleted);

            //Module
            modelBuilder.Entity<Module>().HasIndex(x => x.Name).IsUnique();
            modelBuilder.Entity<Module>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Module>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Module>().HasIndex(x => x.Deleted);

            //ModuleProfileSetting
            modelBuilder.Entity<ModuleProfileSetting>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<ModuleProfileSetting>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<ModuleProfileSetting>().HasIndex(x => x.Deleted);

            //Note
            modelBuilder.Entity<Note>().HasIndex(x => x.RecordId);
            modelBuilder.Entity<Note>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Note>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Note>().HasIndex(x => x.Deleted);

            //Notification
            modelBuilder.Entity<Notification>().HasIndex(x => x.NotificationType);
            modelBuilder.Entity<Notification>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Notification>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Notification>().HasIndex(x => x.Deleted);

            //Picklist
            modelBuilder.Entity<Picklist>().HasIndex(x => x.LabelEn).IsUnique();
            modelBuilder.Entity<Picklist>().HasIndex(x => x.LabelTr).IsUnique();
            modelBuilder.Entity<Picklist>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Picklist>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Picklist>().HasIndex(x => x.Deleted);

            //PicklistItem
            modelBuilder.Entity<PicklistItem>().HasIndex(x => x.SystemCode).IsUnique();
            modelBuilder.Entity<PicklistItem>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<PicklistItem>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<PicklistItem>().HasIndex(x => x.Deleted);

            //Process
            modelBuilder.Entity<Process>().HasIndex(x => x.ModuleId);
            modelBuilder.Entity<Process>().HasIndex(x => x.Active);
            modelBuilder.Entity<Process>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Process>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Process>().HasIndex(x => x.Deleted);

            //ProcessApprover
            modelBuilder.Entity<ProcessApprover>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<ProcessApprover>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<ProcessApprover>().HasIndex(x => x.Deleted);

            //ProcessFilter
            modelBuilder.Entity<ProcessFilter>().HasIndex(x => x.ProcessId);
            modelBuilder.Entity<ProcessFilter>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<ProcessFilter>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<ProcessFilter>().HasIndex(x => x.Deleted);

            //ProcessLog
            modelBuilder.Entity<ProcessLog>().HasIndex(x => x.ModuleId);
            modelBuilder.Entity<ProcessLog>().HasIndex(x => x.RecordId);
            modelBuilder.Entity<ProcessLog>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<ProcessLog>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<ProcessLog>().HasIndex(x => x.Deleted);

            //ProcessRequest
            modelBuilder.Entity<ProcessRequest>().HasIndex(x => x.Module);
            modelBuilder.Entity<ProcessRequest>().HasIndex(x => x.RecordId);
            modelBuilder.Entity<ProcessRequest>().HasIndex(x => x.Status);
            modelBuilder.Entity<ProcessRequest>().HasIndex(x => x.OperationType);
            modelBuilder.Entity<ProcessRequest>().HasIndex(x => x.ProcessStatusOrder);
            modelBuilder.Entity<ProcessRequest>().HasIndex(x => x.Active);
            modelBuilder.Entity<ProcessRequest>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<ProcessRequest>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<ProcessRequest>().HasIndex(x => x.Deleted);

            //Relation
            modelBuilder.Entity<Relation>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Relation>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Relation>().HasIndex(x => x.Deleted);

            //Reminder
            modelBuilder.Entity<Reminder>().HasIndex(x => x.ReminderScope);
            modelBuilder.Entity<Reminder>().HasIndex(x => x.ReminderType);
            modelBuilder.Entity<Reminder>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Reminder>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Reminder>().HasIndex(x => x.Deleted);

            //Report
            modelBuilder.Entity<Report>().HasIndex(x => x.SharingType);
            modelBuilder.Entity<Report>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Report>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Report>().HasIndex(x => x.Deleted);

            //ReportCategory
            modelBuilder.Entity<ReportCategory>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<ReportCategory>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<ReportCategory>().HasIndex(x => x.Deleted);

            //Section
            modelBuilder.Entity<Section>().HasIndex(x => new { x.ModuleId, x.Name }).HasName("sections_IX_module_id_name").IsUnique();
            modelBuilder.Entity<Section>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Section>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Section>().HasIndex(x => x.Deleted);

            //SectionPermission
            modelBuilder.Entity<SectionPermission>().HasIndex(x => new { x.SectionId, x.ProfileId }).HasName("section_permissions_IX_section_id_profile_id").IsUnique();
            modelBuilder.Entity<SectionPermission>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<SectionPermission>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<SectionPermission>().HasIndex(x => x.Deleted);

            //Setting
            modelBuilder.Entity<Setting>().HasIndex(x => x.UserId).HasName("settings_IX_user_id").IsUnique();
            modelBuilder.Entity<Setting>().HasIndex(x => x.Key);
            modelBuilder.Entity<Setting>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Setting>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Setting>().HasIndex(x => x.Deleted);

            //Template
            modelBuilder.Entity<Template>().HasIndex(x => new { x.Id, x.Code }).IsUnique();
            modelBuilder.Entity<Template>().HasIndex(x => x.SharingType);
            modelBuilder.Entity<Template>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Template>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Template>().HasIndex(x => x.Deleted);

            //TemplatePermission
            modelBuilder.Entity<TemplatePermission>().HasIndex(x => new { x.TemplateId, x.ProfileId }).HasName("template_permissions_IX_template_id_profile_id").IsUnique();
            modelBuilder.Entity<TemplatePermission>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<TemplatePermission>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<TemplatePermission>().HasIndex(x => x.Deleted);

            //TenantUser
            modelBuilder.Entity<TenantUser>().HasIndex(x => x.Email);
            modelBuilder.Entity<TenantUser>().HasIndex(x => x.FullName);
            modelBuilder.Entity<TenantUser>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<TenantUser>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<TenantUser>().HasIndex(x => x.Deleted);

            //View
            modelBuilder.Entity<View>().HasIndex(x => x.ModuleId);
            modelBuilder.Entity<View>().HasIndex(x => new { x.ModuleId, x.SystemCode }).HasName("views_IX_module_id_system_code").IsUnique();
            modelBuilder.Entity<View>().HasIndex(x => x.SharingType);
            modelBuilder.Entity<View>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<View>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<View>().HasIndex(x => x.Deleted);

            //ViewState
            modelBuilder.Entity<ViewState>().HasIndex(x => new { x.ModuleId, x.UserId }).HasName("view_states_IX_module_id_user_id").IsUnique();
            modelBuilder.Entity<ViewState>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<ViewState>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<ViewState>().HasIndex(x => x.Deleted);

            //Widget
            modelBuilder.Entity<Widget>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Widget>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Widget>().HasIndex(x => x.Deleted);

            //Workflow
            modelBuilder.Entity<Workflow>().HasIndex(x => x.Active);
            modelBuilder.Entity<Workflow>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<Workflow>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<Workflow>().HasIndex(x => x.Deleted);

            //WorkflowFilter
            modelBuilder.Entity<WorkflowFilter>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<WorkflowFilter>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<WorkflowFilter>().HasIndex(x => x.Deleted);

            //WorkflowLog
            modelBuilder.Entity<WorkflowLog>().HasIndex(x => x.ModuleId);
            modelBuilder.Entity<WorkflowLog>().HasIndex(x => x.RecordId);
            modelBuilder.Entity<WorkflowLog>().HasIndex(x => x.CreatedAt);
            modelBuilder.Entity<WorkflowLog>().HasIndex(x => x.UpdatedAt);
            modelBuilder.Entity<WorkflowLog>().HasIndex(x => x.Deleted);
        }

        public DbSet<TenantUser> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ProfilePermission> ProfilePermissions { get; set; }
        public DbSet<Calculation> Calculations { get; set; }
        public DbSet<Dependency> Dependencies { get; set; }
        public DbSet<Components> Components { get; set; }
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
        public DbSet<ViewShares> ViewShares { get; set; }
        public DbSet<UsersUserGroup> UsersUserGroups { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
    }
}