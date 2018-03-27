namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1078 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("public.workflow_notifications", "message", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("public.workflow_notifications", "message", c => c.String(nullable: false, maxLength: 500));
        }
    }
}
