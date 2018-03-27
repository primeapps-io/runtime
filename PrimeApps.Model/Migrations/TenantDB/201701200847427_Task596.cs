namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task596 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.field_permissions",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        field_id = c.Int(nullable: false),
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
                .ForeignKey("public.fields", t => t.field_id, cascadeDelete: true)
                .ForeignKey("public.profiles", t => t.profile_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => new { t.field_id, t.profile_id }, unique: true, name: "field_permissions_IX_field_id_profile_id")
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.field_permissions", "updated_by", "public.users");
            DropForeignKey("public.field_permissions", "profile_id", "public.profiles");
            DropForeignKey("public.field_permissions", "field_id", "public.fields");
            DropForeignKey("public.field_permissions", "created_by", "public.users");
            DropIndex("public.field_permissions", new[] { "deleted" });
            DropIndex("public.field_permissions", new[] { "updated_at" });
            DropIndex("public.field_permissions", new[] { "created_at" });
            DropIndex("public.field_permissions", new[] { "updated_by" });
            DropIndex("public.field_permissions", new[] { "created_by" });
            DropIndex("public.field_permissions", "field_permissions_IX_field_id_profile_id");
            DropTable("public.field_permissions");
        }
    }
}
