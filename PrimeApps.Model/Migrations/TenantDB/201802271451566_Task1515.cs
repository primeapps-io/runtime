namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1515 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.components",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        name = c.String(nullable: false, maxLength: 15),
                        content = c.String(),
                        type = c.Int(nullable: false),
                        place = c.Int(nullable: false),
                        module_id = c.Int(nullable: false),
                        order = c.Int(nullable: false),
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
            DropForeignKey("public.components", "updated_by", "public.users");
            DropForeignKey("public.components", "module_id", "public.modules");
            DropForeignKey("public.components", "created_by", "public.users");
            DropIndex("public.components", new[] { "deleted" });
            DropIndex("public.components", new[] { "updated_at" });
            DropIndex("public.components", new[] { "created_at" });
            DropIndex("public.components", new[] { "updated_by" });
            DropIndex("public.components", new[] { "created_by" });
            DropIndex("public.components", new[] { "module_id" });
            DropTable("public.components");
        }
    }
}
