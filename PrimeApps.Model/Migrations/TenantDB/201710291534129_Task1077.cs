namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Task1077 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("public.reports", "module_id", "public.modules");
            DropIndex("public.reports", new[] { "module_id" });
            CreateTable(
                "public.report_shares",
                c => new
                {
                    report_id = c.Int(nullable: false),
                    user_id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.report_id, t.user_id })
                .ForeignKey("public.reports", t => t.report_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.user_id, cascadeDelete: true)
                .Index(t => t.report_id)
                .Index(t => t.user_id);

            AddColumn("public.reports", "sharing_type", c => c.Int(nullable: false));
            AddColumn("public.reports", "filter_logic", c => c.String(maxLength: 50));
            AlterColumn("public.charts", "caption", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("public.charts", "sub_caption", c => c.String(maxLength: 200));
            AlterColumn("public.reports", "module_id", c => c.Int(nullable: false, defaultValue: 1));
            CreateIndex("public.reports", "module_id");
            CreateIndex("public.reports", "sharing_type");
            AddForeignKey("public.reports", "module_id", "public.modules", "id", cascadeDelete: true);
            DropColumn("public.reports", "only_visual");
        }

        public override void Down()
        {
            AddColumn("public.reports", "only_visual", c => c.Boolean(nullable: false));
            DropForeignKey("public.reports", "module_id", "public.modules");
            DropForeignKey("public.report_shares", "user_id", "public.users");
            DropForeignKey("public.report_shares", "report_id", "public.reports");
            DropIndex("public.report_shares", new[] { "user_id" });
            DropIndex("public.report_shares", new[] { "report_id" });
            DropIndex("public.reports", new[] { "sharing_type" });
            DropIndex("public.reports", new[] { "module_id" });
            AlterColumn("public.reports", "module_id", c => c.Int());
            AlterColumn("public.charts", "sub_caption", c => c.String(maxLength: 280));
            AlterColumn("public.charts", "caption", c => c.String(nullable: false, maxLength: 80));
            DropColumn("public.reports", "filter_logic");
            DropColumn("public.reports", "sharing_type");
            DropTable("public.report_shares");
            CreateIndex("public.reports", "module_id");
            AddForeignKey("public.reports", "module_id", "public.modules", "id");
        }
    }
}
