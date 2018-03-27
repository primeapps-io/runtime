namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1140 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.action_button_permissions",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        action_button_id = c.Int(nullable: false),
                        profile_id = c.Int(nullable: false),
                        type = c.Int(nullable: false),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.action_buttons", t => t.action_button_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.profiles", t => t.profile_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => new { t.action_button_id, t.profile_id }, unique: true, name: "action_button_permissions_IX_action_button_id_profile_id")
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.action_button_permissions", "updated_by", "public.users");
            DropForeignKey("public.action_button_permissions", "profile_id", "public.profiles");
            DropForeignKey("public.action_button_permissions", "created_by", "public.users");
            DropForeignKey("public.action_button_permissions", "action_button_id", "public.action_buttons");
            DropIndex("public.action_button_permissions", new[] { "deleted" });
            DropIndex("public.action_button_permissions", new[] { "updated_at" });
            DropIndex("public.action_button_permissions", new[] { "created_at" });
            DropIndex("public.action_button_permissions", new[] { "updated_by" });
            DropIndex("public.action_button_permissions", new[] { "created_by" });
            DropIndex("public.action_button_permissions", "action_button_permissions_IX_action_button_id_profile_id");
            DropTable("public.action_button_permissions");
        }
    }
}
