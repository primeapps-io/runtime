namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task266 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.charts",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        chart_type = c.Int(nullable: false),
                        caption = c.String(nullable: false, maxLength: 80),
                        sub_caption = c.String(nullable: false, maxLength: 280),
                        theme = c.Int(nullable: false),
                        x_axis_name = c.String(nullable: false, maxLength: 80),
                        y_axis_name = c.String(nullable: false, maxLength: 80),
                        report_id = c.Int(),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.reports", t => t.report_id)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.report_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
            CreateTable(
                "public.reports",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false, maxLength: 100),
                        report_type = c.Int(nullable: false),
                        report_feed = c.Int(nullable: false),
                        only_visual = c.Boolean(nullable: false),
                        sql_function = c.String(),
                        module_id = c.Int(),
                        user_id = c.Int(),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id)
                .ForeignKey("public.users", t => t.updated_by)
                .ForeignKey("public.users", t => t.user_id)
                .Index(t => t.module_id)
                .Index(t => t.user_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
            CreateTable(
                "public.dashlets",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false, maxLength: 50),
                        dashlet_area = c.Int(nullable: false),
                        dashlet_type = c.Int(nullable: false),
                        chart_id = c.Int(),
                        widget_id = c.Int(),
                        order = c.Int(nullable: false),
                        x_tile_height = c.Int(nullable: false),
                        y_tile_length = c.Int(nullable: false),
                        user_id = c.Int(),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.charts", t => t.chart_id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .ForeignKey("public.users", t => t.user_id)
                .ForeignKey("public.widgets", t => t.widget_id)
                .Index(t => t.chart_id)
                .Index(t => t.widget_id)
                .Index(t => t.user_id, name: "dashlets_IX_user_id")
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
            CreateTable(
                "public.widgets",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        widget_type = c.Int(nullable: false),
                        name = c.String(nullable: false, maxLength: 200),
                        load_url = c.String(maxLength: 100),
                        color = c.String(maxLength: 30),
                        icon = c.String(maxLength: 30),
                        report_id = c.Int(),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.reports", t => t.report_id)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.report_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.dashlets", "widget_id", "public.widgets");
            DropForeignKey("public.widgets", "updated_by", "public.users");
            DropForeignKey("public.widgets", "report_id", "public.reports");
            DropForeignKey("public.widgets", "created_by", "public.users");
            DropForeignKey("public.dashlets", "user_id", "public.users");
            DropForeignKey("public.dashlets", "updated_by", "public.users");
            DropForeignKey("public.dashlets", "created_by", "public.users");
            DropForeignKey("public.dashlets", "chart_id", "public.charts");
            DropForeignKey("public.charts", "updated_by", "public.users");
            DropForeignKey("public.charts", "report_id", "public.reports");
            DropForeignKey("public.reports", "user_id", "public.users");
            DropForeignKey("public.reports", "updated_by", "public.users");
            DropForeignKey("public.reports", "module_id", "public.modules");
            DropForeignKey("public.reports", "created_by", "public.users");
            DropForeignKey("public.charts", "created_by", "public.users");
            DropIndex("public.widgets", new[] { "deleted" });
            DropIndex("public.widgets", new[] { "updated_at" });
            DropIndex("public.widgets", new[] { "created_at" });
            DropIndex("public.widgets", new[] { "updated_by" });
            DropIndex("public.widgets", new[] { "created_by" });
            DropIndex("public.widgets", new[] { "report_id" });
            DropIndex("public.dashlets", new[] { "deleted" });
            DropIndex("public.dashlets", new[] { "updated_at" });
            DropIndex("public.dashlets", new[] { "created_at" });
            DropIndex("public.dashlets", new[] { "updated_by" });
            DropIndex("public.dashlets", new[] { "created_by" });
            DropIndex("public.dashlets", "dashlets_IX_user_id");
            DropIndex("public.dashlets", new[] { "widget_id" });
            DropIndex("public.dashlets", new[] { "chart_id" });
            DropIndex("public.reports", new[] { "deleted" });
            DropIndex("public.reports", new[] { "updated_at" });
            DropIndex("public.reports", new[] { "created_at" });
            DropIndex("public.reports", new[] { "updated_by" });
            DropIndex("public.reports", new[] { "created_by" });
            DropIndex("public.reports", new[] { "user_id" });
            DropIndex("public.reports", new[] { "module_id" });
            DropIndex("public.charts", new[] { "deleted" });
            DropIndex("public.charts", new[] { "updated_at" });
            DropIndex("public.charts", new[] { "created_at" });
            DropIndex("public.charts", new[] { "updated_by" });
            DropIndex("public.charts", new[] { "created_by" });
            DropIndex("public.charts", new[] { "report_id" });
            DropTable("public.widgets");
            DropTable("public.dashlets");
            DropTable("public.reports");
            DropTable("public.charts");
        }
    }
}
