namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task675 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.profiles", "document_search", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.profiles", "document_search");
        }
    }
}
