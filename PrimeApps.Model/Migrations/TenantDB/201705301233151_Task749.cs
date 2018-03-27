namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task749 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.action_buttons", "dependent_field", c => c.String());
            AddColumn("public.action_buttons", "dependent", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("public.action_buttons", "dependent");
            DropColumn("public.action_buttons", "dependent_field");
        }
    }
}
