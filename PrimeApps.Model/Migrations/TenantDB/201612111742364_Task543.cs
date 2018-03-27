namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task543 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.analytics",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        label = c.String(nullable: false, maxLength: 50),
                        powerbi_report_id = c.String(),
                        pbix_url = c.String(nullable: false),
                        sharing_type = c.Int(nullable: false),
                        menu_icon = c.String(maxLength: 100),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.powerbi_report_id)
                .Index(t => t.sharing_type)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
            CreateTable(
                "public.analytic_shares",
                c => new
                    {
                        analytic_id = c.Int(nullable: false),
                        user_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.analytic_id, t.user_id })
                .ForeignKey("public.analytics", t => t.analytic_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.user_id, cascadeDelete: true)
                .Index(t => t.analytic_id)
                .Index(t => t.user_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.analytics", "updated_by", "public.users");
            DropForeignKey("public.analytic_shares", "user_id", "public.users");
            DropForeignKey("public.analytic_shares", "analytic_id", "public.analytics");
            DropForeignKey("public.analytics", "created_by", "public.users");
            DropIndex("public.analytic_shares", new[] { "user_id" });
            DropIndex("public.analytic_shares", new[] { "analytic_id" });
            DropIndex("public.analytics", new[] { "deleted" });
            DropIndex("public.analytics", new[] { "updated_at" });
            DropIndex("public.analytics", new[] { "created_at" });
            DropIndex("public.analytics", new[] { "updated_by" });
            DropIndex("public.analytics", new[] { "created_by" });
            DropIndex("public.analytics", new[] { "sharing_type" });
            DropIndex("public.analytics", new[] { "powerbi_report_id" });
            DropTable("public.analytic_shares");
            DropTable("public.analytics");
        }
    }
}
