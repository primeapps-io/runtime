namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task565 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.changelogs",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        record_id = c.Int(nullable: false),
                        record = c.String(),
                        updated_by = c.Int(nullable: false),
                        updated_at = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.updated_by, cascadeDelete: true)
                .Index(t => t.updated_by);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.changelogs", "updated_by", "public.users");
            DropIndex("public.changelogs", new[] { "updated_by" });
            DropTable("public.changelogs");
        }
    }
}
