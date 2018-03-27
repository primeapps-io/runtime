namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1374 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.user_custom_shares",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        user_id = c.Int(nullable: false),
                        users = c.String(),
                        user_groups = c.String(),
                        modules = c.String(),
                        created_by = c.Int(nullable: false),
                        updated_by = c.Int(),
                        created_at = c.DateTime(nullable: false),
                        updated_at = c.DateTime(),
                        deleted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .ForeignKey("public.users", t => t.user_id, cascadeDelete: true)
                .Index(t => t.user_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.user_custom_shares", "user_id", "public.users");
            DropForeignKey("public.user_custom_shares", "updated_by", "public.users");
            DropForeignKey("public.user_custom_shares", "created_by", "public.users");
            DropIndex("public.user_custom_shares", new[] { "deleted" });
            DropIndex("public.user_custom_shares", new[] { "updated_at" });
            DropIndex("public.user_custom_shares", new[] { "created_at" });
            DropIndex("public.user_custom_shares", new[] { "updated_by" });
            DropIndex("public.user_custom_shares", new[] { "created_by" });
            DropIndex("public.user_custom_shares", new[] { "user_id" });
            DropTable("public.user_custom_shares");
        }
    }
}
