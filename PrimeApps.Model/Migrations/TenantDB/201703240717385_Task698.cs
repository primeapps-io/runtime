namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task698 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.section_permissions",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        section_id = c.Int(nullable: false),
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
                .ForeignKey("public.sections", t => t.section_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => new { t.section_id, t.profile_id }, unique: true, name: "section_permissions_IX_section_id_profile_id")
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.section_permissions", "updated_by", "public.users");
            DropForeignKey("public.section_permissions", "section_id", "public.sections");
            DropForeignKey("public.section_permissions", "profile_id", "public.profiles");
            DropForeignKey("public.section_permissions", "created_by", "public.users");
            DropIndex("public.section_permissions", new[] { "deleted" });
            DropIndex("public.section_permissions", new[] { "updated_at" });
            DropIndex("public.section_permissions", new[] { "created_at" });
            DropIndex("public.section_permissions", new[] { "updated_by" });
            DropIndex("public.section_permissions", new[] { "created_by" });
            DropIndex("public.section_permissions", "section_permissions_IX_section_id_profile_id");
            DropTable("public.section_permissions");
        }
    }
}
