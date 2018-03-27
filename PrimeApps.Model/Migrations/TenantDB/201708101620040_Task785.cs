namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task785 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.notifications", "attachment_link", c => c.String(maxLength: 500));
            AddColumn("public.notifications", "attachment_name", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("public.notifications", "attachment_name");
            DropColumn("public.notifications", "attachment_link");
        }
    }
}
