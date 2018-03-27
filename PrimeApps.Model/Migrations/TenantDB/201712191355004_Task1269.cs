namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1269 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.fields", "view_type", c => c.Int(nullable: false));
            AddColumn("public.fields", "position", c => c.Int(nullable: false));
            AddColumn("public.sections", "custom_label", c => c.String(maxLength: 1000));
            AlterColumn("public.fields", "custom_label", c => c.String(maxLength: 1000));
        }
        
        public override void Down()
        {
            AlterColumn("public.fields", "custom_label", c => c.String(maxLength: 400));
            DropColumn("public.sections", "custom_label");
            DropColumn("public.fields", "position");
            DropColumn("public.fields", "view_type");
        }
    }
}
