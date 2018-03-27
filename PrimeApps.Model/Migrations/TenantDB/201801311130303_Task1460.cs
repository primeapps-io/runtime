namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1460 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.template_permissions",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        template_id = c.Int(nullable: false),
                        profile_id = c.Int(nullable: false),
                        type = c.Int(nullable: false),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.profiles", t => t.profile_id, cascadeDelete: true)
                .ForeignKey("public.templates", t => t.template_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => new { t.template_id, t.profile_id }, unique: true, name: "template_permissions_IX_template_id_profile_id")
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.template_permissions", "updated_by", "public.users");
            DropForeignKey("public.template_permissions", "template_id", "public.templates");
            DropForeignKey("public.template_permissions", "profile_id", "public.profiles");
            DropForeignKey("public.template_permissions", "created_by", "public.users");
            DropIndex("public.template_permissions", new[] { "deleted" });
            DropIndex("public.template_permissions", new[] { "updated_at" });
            DropIndex("public.template_permissions", new[] { "created_at" });
            DropIndex("public.template_permissions", new[] { "updated_by" });
            DropIndex("public.template_permissions", new[] { "created_by" });
            DropIndex("public.template_permissions", "template_permissions_IX_template_id_profile_id");
            DropTable("public.template_permissions");
        }
    }
}
