namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task619 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.fields", "primary_lookup", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.fields", "primary_lookup");
        }
    }
}
