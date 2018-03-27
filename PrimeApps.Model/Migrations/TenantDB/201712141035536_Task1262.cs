namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1262 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.fields", "editable", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("public.fields", "show_label", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("public.fields", "show_label");
            DropColumn("public.fields", "editable");
        }
    }
}
