namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task936 : DbMigration
    {
        public override void Up()
        {
            DropIndex("public.reports", new[] { "module_id" });
            DropIndex("public.reports", new[] { "user_id" });
            CreateTable(
                "public.report_aggregations",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        report_id = c.Int(nullable: false),
                        type = c.Int(nullable: false),
                        field = c.String(nullable: false, maxLength: 120),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.reports", t => t.report_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.report_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
            CreateTable(
                "public.report_categories",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false, maxLength: 100),
                        order = c.Int(nullable: false),
                        user_id = c.Int(),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .ForeignKey("public.users", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
            CreateTable(
                "public.report_fields",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        report_id = c.Int(nullable: false),
                        field = c.String(nullable: false, maxLength: 120),
                        order = c.Int(nullable: false),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.reports", t => t.report_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.report_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
            CreateTable(
                "public.report_filters",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        report_id = c.Int(nullable: false),
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
                .ForeignKey("public.reports", t => t.report_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.report_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
            AddColumn("public.profiles", "report", c => c.Boolean(nullable: false,defaultValue:true));
            AddColumn("public.reports", "category_id", c => c.Int());
            AddColumn("public.reports", "group_field", c => c.String());
            AddColumn("public.reports", "sort_field", c => c.String());
            AddColumn("public.reports", "sort_direction", c => c.Int(nullable: false));
            AddColumn("public.widgets", "view_id", c => c.Int());
            AlterColumn("public.charts", "sub_caption", c => c.String(maxLength: 280));
            CreateIndex("public.reports", "module_id");
            CreateIndex("public.reports", "user_id");
            CreateIndex("public.reports", "category_id");
            CreateIndex("public.widgets", "view_id");
            AddForeignKey("public.reports", "category_id", "public.report_categories", "id");
            AddForeignKey("public.widgets", "view_id", "public.views", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("public.widgets", "view_id", "public.views");
            DropForeignKey("public.report_filters", "updated_by", "public.users");
            DropForeignKey("public.report_filters", "report_id", "public.reports");
            DropForeignKey("public.report_filters", "created_by", "public.users");
            DropForeignKey("public.report_fields", "updated_by", "public.users");
            DropForeignKey("public.report_fields", "report_id", "public.reports");
            DropForeignKey("public.report_fields", "created_by", "public.users");
            DropForeignKey("public.reports", "category_id", "public.report_categories");
            DropForeignKey("public.report_categories", "user_id", "public.users");
            DropForeignKey("public.report_categories", "updated_by", "public.users");
            DropForeignKey("public.report_categories", "created_by", "public.users");
            DropForeignKey("public.report_aggregations", "updated_by", "public.users");
            DropForeignKey("public.report_aggregations", "report_id", "public.reports");
            DropForeignKey("public.report_aggregations", "created_by", "public.users");
            DropIndex("public.widgets", new[] { "view_id" });
            DropIndex("public.report_filters", new[] { "deleted" });
            DropIndex("public.report_filters", new[] { "updated_at" });
            DropIndex("public.report_filters", new[] { "created_at" });
            DropIndex("public.report_filters", new[] { "updated_by" });
            DropIndex("public.report_filters", new[] { "created_by" });
            DropIndex("public.report_filters", new[] { "report_id" });
            DropIndex("public.report_fields", new[] { "deleted" });
            DropIndex("public.report_fields", new[] { "updated_at" });
            DropIndex("public.report_fields", new[] { "created_at" });
            DropIndex("public.report_fields", new[] { "updated_by" });
            DropIndex("public.report_fields", new[] { "created_by" });
            DropIndex("public.report_fields", new[] { "report_id" });
            DropIndex("public.report_categories", new[] { "deleted" });
            DropIndex("public.report_categories", new[] { "updated_at" });
            DropIndex("public.report_categories", new[] { "created_at" });
            DropIndex("public.report_categories", new[] { "updated_by" });
            DropIndex("public.report_categories", new[] { "created_by" });
            DropIndex("public.report_categories", new[] { "user_id" });
            DropIndex("public.report_aggregations", new[] { "deleted" });
            DropIndex("public.report_aggregations", new[] { "updated_at" });
            DropIndex("public.report_aggregations", new[] { "created_at" });
            DropIndex("public.report_aggregations", new[] { "updated_by" });
            DropIndex("public.report_aggregations", new[] { "created_by" });
            DropIndex("public.report_aggregations", new[] { "report_id" });
            DropIndex("public.reports", new[] { "category_id" });
            DropIndex("public.reports", new[] { "user_id" });
            DropIndex("public.reports", new[] { "module_id" });
            AlterColumn("public.charts", "sub_caption", c => c.String(nullable: false, maxLength: 280));
            DropColumn("public.widgets", "view_id");
            DropColumn("public.reports", "sort_direction");
            DropColumn("public.reports", "sort_field");
            DropColumn("public.reports", "group_field");
            DropColumn("public.reports", "category_id");
            DropColumn("public.profiles", "report");
            DropTable("public.report_filters");
            DropTable("public.report_fields");
            DropTable("public.report_categories");
            DropTable("public.report_aggregations");
            CreateIndex("public.reports", "user_id");
            CreateIndex("public.reports", "module_id");
        }
    }
}
