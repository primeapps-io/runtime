namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1426 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.module_profile_settings",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        module_id = c.Int(nullable: false),
                        profiles = c.String(),
                        label_en_singular = c.String(nullable: false, maxLength: 50),
                        label_en_plural = c.String(nullable: false, maxLength: 50),
                        label_tr_singular = c.String(nullable: false, maxLength: 50),
                        label_tr_plural = c.String(nullable: false, maxLength: 50),
                        menu_icon = c.String(maxLength: 100),
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
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.module_profile_settings", "updated_by", "public.users");
            DropForeignKey("public.module_profile_settings", "module_id", "public.modules");
            DropForeignKey("public.module_profile_settings", "created_by", "public.users");
            DropIndex("public.module_profile_settings", new[] { "deleted" });
            DropIndex("public.module_profile_settings", new[] { "updated_at" });
            DropIndex("public.module_profile_settings", new[] { "created_at" });
            DropIndex("public.module_profile_settings", new[] { "updated_by" });
            DropIndex("public.module_profile_settings", new[] { "created_by" });
            DropIndex("public.module_profile_settings", new[] { "module_id" });
            DropTable("public.module_profile_settings");
        }
    }
}
