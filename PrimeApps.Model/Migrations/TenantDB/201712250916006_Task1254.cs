namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Task1254 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.conversion_sub_modules",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    module_id = c.Int(nullable: false),
                    sub_module_id = c.Int(nullable: false),
                    submodule_source_field = c.String(),
                    submodule_destination_field = c.String(),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.sub_module_id, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.module_id)
                .Index(t => t.sub_module_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

        }

        public override void Down()
        {
            DropForeignKey("public.conversion_sub_modules", "updated_by", "public.users");
            DropForeignKey("public.conversion_sub_modules", "module_id", "public.modules");
            DropForeignKey("public.conversion_sub_modules", "sub_module_id", "public.modules");
            DropForeignKey("public.conversion_sub_modules", "created_by", "public.users");
            DropIndex("public.conversion_sub_modules", new[] { "deleted" });
            DropIndex("public.conversion_sub_modules", new[] { "updated_at" });
            DropIndex("public.conversion_sub_modules", new[] { "created_at" });
            DropIndex("public.conversion_sub_modules", new[] { "updated_by" });
            DropIndex("public.conversion_sub_modules", new[] { "created_by" });
            DropIndex("public.conversion_sub_modules", new[] { "sub_module_id" });
            DropIndex("public.conversion_sub_modules", new[] { "module_id" });
            DropTable("public.conversion_sub_modules");
        }
    }
}
