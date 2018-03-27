namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1505 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.profiles", "collective_annual_leave", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.profiles", "collective_annual_leave");
        }
    }
}
