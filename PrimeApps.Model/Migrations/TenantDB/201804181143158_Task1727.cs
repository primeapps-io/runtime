namespace OfisimCRM.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1727 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.tags",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        text = c.String(maxLength: 400),
                        field_id = c.Int(nullable: false),
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
                .Index(t => t.text)
                .Index(t => t.field_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.tags", "updated_by", "public.users");
            DropForeignKey("public.tags", "field_id", "public.fields");
            DropForeignKey("public.tags", "created_by", "public.users");
            DropIndex("public.tags", new[] { "deleted" });
            DropIndex("public.tags", new[] { "updated_at" });
            DropIndex("public.tags", new[] { "created_at" });
            DropIndex("public.tags", new[] { "updated_by" });
            DropIndex("public.tags", new[] { "created_by" });
            DropIndex("public.tags", new[] { "field_id" });
            DropIndex("public.tags", new[] { "text" });
            DropTable("public.tags");
        }
    }
}
