namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1477 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.processes", "profiles", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("public.processes", "profiles");
        }
    }
}
