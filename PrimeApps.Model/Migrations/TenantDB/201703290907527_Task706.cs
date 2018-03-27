namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task706 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.fields", "custom_label", c => c.String(maxLength: 400));
        }
        
        public override void Down()
        {
            DropColumn("public.fields", "custom_label");
        }
    }
}
