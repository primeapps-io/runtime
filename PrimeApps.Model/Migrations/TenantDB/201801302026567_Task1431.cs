namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1431 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("public.dashlets", "user_id", "public.users");
            DropIndex("public.dashlets", "dashlets_IX_user_id");
            CreateTable(
                "public.dashboard",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false, maxLength: 50),
                        description = c.String(maxLength: 250),
                        user_id = c.Int(),
                        profile_id = c.Int(),
                        is_active = c.Boolean(nullable: false),
                        sharing_type = c.Int(nullable: false),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.profiles", t => t.profile_id)
                .ForeignKey("public.users", t => t.updated_by)
                .ForeignKey("public.users", t => t.user_id)
                .Index(t => t.user_id)
                .Index(t => t.profile_id)
                .Index(t => t.sharing_type)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
            AddColumn("public.dashlets", "dashboard_id", c => c.Int());
            CreateIndex("public.dashlets", "dashboard_id");
            AddForeignKey("public.dashlets", "dashboard_id", "public.dashboard", "id");
            DropColumn("public.dashlets", "user_id");
        }
        
        public override void Down()
        {
            AddColumn("public.dashlets", "user_id", c => c.Int());
            DropForeignKey("public.dashlets", "dashboard_id", "public.dashboard");
            DropForeignKey("public.dashboard", "user_id", "public.users");
            DropForeignKey("public.dashboard", "updated_by", "public.users");
            DropForeignKey("public.dashboard", "profile_id", "public.profiles");
            DropForeignKey("public.dashboard", "created_by", "public.users");
            DropIndex("public.dashlets", new[] { "dashboard_id" });
            DropIndex("public.dashboard", new[] { "deleted" });
            DropIndex("public.dashboard", new[] { "updated_at" });
            DropIndex("public.dashboard", new[] { "created_at" });
            DropIndex("public.dashboard", new[] { "updated_by" });
            DropIndex("public.dashboard", new[] { "created_by" });
            DropIndex("public.dashboard", new[] { "sharing_type" });
            DropIndex("public.dashboard", new[] { "profile_id" });
            DropIndex("public.dashboard", new[] { "user_id" });
            DropColumn("public.dashlets", "dashboard_id");
            DropTable("public.dashboard");
            CreateIndex("public.dashlets", "user_id", name: "dashlets_IX_user_id");
            AddForeignKey("public.dashlets", "user_id", "public.users", "id");
        }
    }
}
