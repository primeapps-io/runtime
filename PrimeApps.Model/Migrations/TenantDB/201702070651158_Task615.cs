namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task615 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.modules", "display_calendar", c => c.Boolean(nullable: false));
            AddColumn("public.modules", "calendar_color_dark", c => c.String());
            AddColumn("public.modules", "calendar_color_light", c => c.String());
            AddColumn("public.fields", "calendar_date_type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.fields", "calendar_date_type");
            DropColumn("public.modules", "calendar_color_light");
            DropColumn("public.modules", "calendar_color_dark");
            DropColumn("public.modules", "display_calendar");
        }
    }
}
