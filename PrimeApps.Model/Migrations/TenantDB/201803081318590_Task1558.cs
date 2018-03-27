namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1558 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.helps", "name", c => c.String(nullable: false));
            AddColumn("public.helps", "module_type", c => c.Int(nullable: false));
            AddColumn("public.helps", "custom_help", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.helps", "custom_help");
            DropColumn("public.helps", "module_type");
            DropColumn("public.helps", "name");
        }
    }
}
