namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task854 : DbMigration
    {
        public override void Up()
        {
            DropIndex("public.workflow_filters", new[] { "workflow_id" });
            CreateTable(
                "public.process_approvers",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        process_id = c.Int(nullable: false),
                        user_id = c.Int(nullable: false),
                        order = c.Short(nullable: false),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.processes", t => t.process_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .ForeignKey("public.users", t => t.user_id, cascadeDelete: true)
                .Index(t => t.process_id)
                .Index(t => t.user_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
            CreateTable(
                "public.processes",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        module_id = c.Int(nullable: false),
                        user_id = c.Int(nullable: false),
                        name = c.String(nullable: false, maxLength: 200),
                        frequency = c.Int(nullable: false),
                        active = c.Boolean(nullable: false),
                        operations = c.String(nullable: false, maxLength: 50),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.module_id)
                .Index(t => t.active)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
            CreateTable(
                "public.process_filters",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        process_id = c.Int(nullable: false),
                        field = c.String(nullable: false, maxLength: 120),
                        Operator = c.Int(nullable: false),
                        Value = c.String(nullable: false, maxLength: 100),
                        No = c.Int(nullable: false),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.processes", t => t.process_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.process_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
            CreateTable(
                "public.process_logs",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        process_id = c.Int(nullable: false),
                        module_id = c.Int(nullable: false),
                        record_id = c.Int(nullable: false),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.processes", t => t.process_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.process_id)
                .Index(t => t.module_id)
                .Index(t => t.record_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
            CreateTable(
                "public.process_requests",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        process_id = c.Int(nullable: false),
                        record_id = c.Int(nullable: false),
                        process_status = c.Int(nullable: false),
                        operation_type = c.Int(nullable: false),
                        process_status_order = c.Int(nullable: false),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.processes", t => t.process_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.process_id)
                .Index(t => t.record_id)
                .Index(t => t.process_status)
                .Index(t => t.operation_type)
                .Index(t => t.process_status_order)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
            CreateIndex("public.workflow_filters", "workflow_id");
        }
        
        public override void Down()
        {
            DropForeignKey("public.process_approvers", "user_id", "public.users");
            DropForeignKey("public.process_approvers", "updated_by", "public.users");
            DropForeignKey("public.process_approvers", "process_id", "public.processes");
            DropForeignKey("public.processes", "updated_by", "public.users");
            DropForeignKey("public.process_requests", "updated_by", "public.users");
            DropForeignKey("public.process_requests", "process_id", "public.processes");
            DropForeignKey("public.process_requests", "created_by", "public.users");
            DropForeignKey("public.processes", "module_id", "public.modules");
            DropForeignKey("public.process_logs", "updated_by", "public.users");
            DropForeignKey("public.process_logs", "process_id", "public.processes");
            DropForeignKey("public.process_logs", "created_by", "public.users");
            DropForeignKey("public.process_filters", "updated_by", "public.users");
            DropForeignKey("public.process_filters", "process_id", "public.processes");
            DropForeignKey("public.process_filters", "created_by", "public.users");
            DropForeignKey("public.processes", "created_by", "public.users");
            DropForeignKey("public.process_approvers", "created_by", "public.users");
            DropIndex("public.workflow_filters", new[] { "workflow_id" });
            DropIndex("public.process_requests", new[] { "deleted" });
            DropIndex("public.process_requests", new[] { "updated_at" });
            DropIndex("public.process_requests", new[] { "created_at" });
            DropIndex("public.process_requests", new[] { "updated_by" });
            DropIndex("public.process_requests", new[] { "created_by" });
            DropIndex("public.process_requests", new[] { "process_status_order" });
            DropIndex("public.process_requests", new[] { "operation_type" });
            DropIndex("public.process_requests", new[] { "process_status" });
            DropIndex("public.process_requests", new[] { "record_id" });
            DropIndex("public.process_requests", new[] { "process_id" });
            DropIndex("public.process_logs", new[] { "deleted" });
            DropIndex("public.process_logs", new[] { "updated_at" });
            DropIndex("public.process_logs", new[] { "created_at" });
            DropIndex("public.process_logs", new[] { "updated_by" });
            DropIndex("public.process_logs", new[] { "created_by" });
            DropIndex("public.process_logs", new[] { "record_id" });
            DropIndex("public.process_logs", new[] { "module_id" });
            DropIndex("public.process_logs", new[] { "process_id" });
            DropIndex("public.process_filters", new[] { "deleted" });
            DropIndex("public.process_filters", new[] { "updated_at" });
            DropIndex("public.process_filters", new[] { "created_at" });
            DropIndex("public.process_filters", new[] { "updated_by" });
            DropIndex("public.process_filters", new[] { "created_by" });
            DropIndex("public.process_filters", new[] { "process_id" });
            DropIndex("public.processes", new[] { "deleted" });
            DropIndex("public.processes", new[] { "updated_at" });
            DropIndex("public.processes", new[] { "created_at" });
            DropIndex("public.processes", new[] { "updated_by" });
            DropIndex("public.processes", new[] { "created_by" });
            DropIndex("public.processes", new[] { "active" });
            DropIndex("public.processes", new[] { "module_id" });
            DropIndex("public.process_approvers", new[] { "deleted" });
            DropIndex("public.process_approvers", new[] { "updated_at" });
            DropIndex("public.process_approvers", new[] { "created_at" });
            DropIndex("public.process_approvers", new[] { "updated_by" });
            DropIndex("public.process_approvers", new[] { "created_by" });
            DropIndex("public.process_approvers", new[] { "user_id" });
            DropIndex("public.process_approvers", new[] { "process_id" });
            DropTable("public.process_requests");
            DropTable("public.process_logs");
            DropTable("public.process_filters");
            DropTable("public.processes");
            DropTable("public.process_approvers");
            CreateIndex("public.workflow_filters", "workflow_id");
        }
    }
}
