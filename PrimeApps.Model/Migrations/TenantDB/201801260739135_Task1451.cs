namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1451 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.module_profile_settings", "display", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.module_profile_settings", "display");
        }
    }
}
