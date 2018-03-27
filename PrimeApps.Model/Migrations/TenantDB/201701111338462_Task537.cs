namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task537 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.roles", "share_data", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.roles", "share_data");
        }
    }
}
