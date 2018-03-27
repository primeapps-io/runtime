namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task612 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.imports",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        module_id = c.Int(nullable: false),
                        total_count = c.Int(nullable: false),
                        excel_url = c.String(),
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
            DropForeignKey("public.imports", "updated_by", "public.users");
            DropForeignKey("public.imports", "module_id", "public.modules");
            DropForeignKey("public.imports", "created_by", "public.users");
            DropIndex("public.imports", new[] { "deleted" });
            DropIndex("public.imports", new[] { "updated_at" });
            DropIndex("public.imports", new[] { "created_at" });
            DropIndex("public.imports", new[] { "updated_by" });
            DropIndex("public.imports", new[] { "created_by" });
            DropIndex("public.imports", new[] { "module_id" });
            DropTable("public.imports");
        }
    }
}
