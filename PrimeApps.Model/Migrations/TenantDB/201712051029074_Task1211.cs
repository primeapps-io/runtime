namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1211 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.workflow_notifications", "cc", c => c.String(maxLength: 4000));
            AddColumn("public.workflow_notifications", "bcc", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("public.workflow_notifications", "bcc");
            DropColumn("public.workflow_notifications", "cc");
        }
    }
}
