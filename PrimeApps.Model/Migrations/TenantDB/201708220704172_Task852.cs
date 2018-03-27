namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task852 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.template_shares",
                c => new
                    {
                        template_id = c.Int(nullable: false),
                        user_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.template_id, t.user_id })
                .ForeignKey("public.templates", t => t.template_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.user_id, cascadeDelete: true)
                .Index(t => t.template_id)
                .Index(t => t.user_id);
            
            AddColumn("public.templates", "subject", c => c.String(maxLength: 200));
            AddColumn("public.templates", "sharing_type", c => c.Int(nullable: false));
            CreateIndex("public.templates", "sharing_type");
        }
        
        public override void Down()
        {
            DropForeignKey("public.template_shares", "user_id", "public.users");
            DropForeignKey("public.template_shares", "template_id", "public.templates");
            DropIndex("public.template_shares", new[] { "user_id" });
            DropIndex("public.template_shares", new[] { "template_id" });
            DropIndex("public.templates", new[] { "sharing_type" });
            DropColumn("public.templates", "sharing_type");
            DropColumn("public.templates", "subject");
            DropTable("public.template_shares");
        }
    }
}
