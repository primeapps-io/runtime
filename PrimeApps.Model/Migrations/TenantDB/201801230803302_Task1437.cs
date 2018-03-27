namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1437 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.fields", "show_as_dropdown", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.fields", "show_as_dropdown");
        }
    }
}
