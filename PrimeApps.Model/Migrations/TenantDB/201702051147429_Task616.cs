namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task616 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.templates", "module", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("public.templates", "module");
        }
    }
}
