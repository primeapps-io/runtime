namespace OfisimCRM.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1731 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.menu_items",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        menu_id = c.Int(nullable: false),
                        module_id = c.Int(),
                        parent_id = c.Int(),
                        route = c.String(maxLength: 100),
                        label_en = c.String(nullable: false, maxLength: 50),
                        label_tr = c.String(nullable: false, maxLength: 50),
                        menu_icon = c.String(maxLength: 100),
                        order = c.Short(nullable: false),
                        is_dynamic = c.Boolean(nullable: false),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.menu", t => t.menu_id, cascadeDelete: true)
                .ForeignKey("public.menu_items", t => t.parent_id)
                .ForeignKey("public.modules", t => t.module_id)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.menu_id)
                .Index(t => t.module_id)
                .Index(t => t.parent_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
            CreateTable(
                "public.menu",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false),
                        profile_id = c.Int(nullable: false),
                        _default = c.Boolean(name: "default", nullable: false),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.profiles", t => t.profile_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.profile_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.menu_items", "updated_by", "public.users");
            DropForeignKey("public.menu_items", "module_id", "public.modules");
            DropForeignKey("public.menu_items", "parent_id", "public.menu_items");
            DropForeignKey("public.menu_items", "menu_id", "public.menu");
            DropForeignKey("public.menu", "updated_by", "public.users");
            DropForeignKey("public.menu", "profile_id", "public.profiles");
            DropForeignKey("public.menu", "created_by", "public.users");
            DropForeignKey("public.menu_items", "created_by", "public.users");
            DropIndex("public.menu", new[] { "deleted" });
            DropIndex("public.menu", new[] { "updated_at" });
            DropIndex("public.menu", new[] { "created_at" });
            DropIndex("public.menu", new[] { "updated_by" });
            DropIndex("public.menu", new[] { "created_by" });
            DropIndex("public.menu", new[] { "profile_id" });
            DropIndex("public.menu_items", new[] { "deleted" });
            DropIndex("public.menu_items", new[] { "updated_at" });
            DropIndex("public.menu_items", new[] { "created_at" });
            DropIndex("public.menu_items", new[] { "updated_by" });
            DropIndex("public.menu_items", new[] { "created_by" });
            DropIndex("public.menu_items", new[] { "parent_id" });
            DropIndex("public.menu_items", new[] { "module_id" });
            DropIndex("public.menu_items", new[] { "menu_id" });
            DropTable("public.menu");
            DropTable("public.menu_items");
        }
    }
}
