namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task850 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.fields", "address_type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.fields", "address_type");
        }
    }
}
