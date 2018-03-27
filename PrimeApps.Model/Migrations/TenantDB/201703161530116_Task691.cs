namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task691 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.fields", "multiline_type_use_html", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.fields", "multiline_type_use_html");
        }
    }
}
