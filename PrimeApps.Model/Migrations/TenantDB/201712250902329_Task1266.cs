namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1266 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.workflows", "process_filter", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.workflows", "process_filter");
        }
    }
}
