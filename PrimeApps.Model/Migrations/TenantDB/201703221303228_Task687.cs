namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task687 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.fields", "document_search", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.fields", "document_search");
        }
    }
}
