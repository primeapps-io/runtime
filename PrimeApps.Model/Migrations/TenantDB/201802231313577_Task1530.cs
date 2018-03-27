namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1530 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.user_custom_shares", "shared_user_id", c => c.Int(nullable: false));
            CreateIndex("public.user_custom_shares", "shared_user_id");
            AddForeignKey("public.user_custom_shares", "shared_user_id", "public.users", "id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("public.user_custom_shares", "shared_user_id", "public.users");
            DropIndex("public.user_custom_shares", new[] { "shared_user_id" });
            DropColumn("public.user_custom_shares", "shared_user_id");
        }
    }
}
