namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task780 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.profiles", "lead_convert", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("public.profiles", "lead_convert");
        }
    }
}
