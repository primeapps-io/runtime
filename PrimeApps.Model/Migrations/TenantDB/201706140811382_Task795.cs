namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task795 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.action_buttons", "method_type", c => c.Int(nullable: false));
            AddColumn("public.action_buttons", "parameters", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("public.action_buttons", "parameters");
            DropColumn("public.action_buttons", "method_type");
        }
    }
}
