namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1512 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.helps",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        template = c.String(nullable: false),
                        module_id = c.Int(),
                        modal_type = c.Int(nullable: false),
                        show_type = c.Int(nullable: false),
                        route_url = c.String(),
                        first_screen = c.Boolean(nullable: false),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id)
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
            DropForeignKey("public.helps", "updated_by", "public.users");
            DropForeignKey("public.helps", "module_id", "public.modules");
            DropForeignKey("public.helps", "created_by", "public.users");
            DropIndex("public.helps", new[] { "deleted" });
            DropIndex("public.helps", new[] { "updated_at" });
            DropIndex("public.helps", new[] { "created_at" });
            DropIndex("public.helps", new[] { "updated_by" });
            DropIndex("public.helps", new[] { "created_by" });
            DropIndex("public.helps", new[] { "module_id" });
            DropTable("public.helps");
        }
    }
}
