namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task682 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.reminders", "timezone_offset", c => c.Int(nullable: false, defaultValue:180));
        }
        
        public override void Down()
        {
            DropColumn("public.reminders", "timezone_offset");
        }
    }
}
