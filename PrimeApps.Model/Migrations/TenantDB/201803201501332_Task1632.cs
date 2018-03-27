namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1632 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.field_filters",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        field_id = c.Int(nullable: false),
                        filter_field = c.String(nullable: false, maxLength: 120),
                        Operator = c.Int(nullable: false),
                        Value = c.String(nullable: false, maxLength: 100),
                        No = c.Int(nullable: false),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.fields", t => t.field_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.field_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.field_filters", "updated_by", "public.users");
            DropForeignKey("public.field_filters", "field_id", "public.fields");
            DropForeignKey("public.field_filters", "created_by", "public.users");
            DropIndex("public.field_filters", new[] { "deleted" });
            DropIndex("public.field_filters", new[] { "updated_at" });
            DropIndex("public.field_filters", new[] { "created_at" });
            DropIndex("public.field_filters", new[] { "updated_by" });
            DropIndex("public.field_filters", new[] { "created_by" });
            DropIndex("public.field_filters", new[] { "field_id" });
            DropTable("public.field_filters");
        }
    }
}
