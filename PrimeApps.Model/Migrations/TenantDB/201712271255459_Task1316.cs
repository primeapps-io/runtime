namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1316 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.processes", "approver_type", c => c.Int(nullable: false));
            AddColumn("public.processes", "trigger_time", c => c.Int(nullable: false));
            AddColumn("public.processes", "approver_field", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("public.processes", "approver_field");
            DropColumn("public.processes", "trigger_time");
            DropColumn("public.processes", "approver_type");
        }
    }
}
