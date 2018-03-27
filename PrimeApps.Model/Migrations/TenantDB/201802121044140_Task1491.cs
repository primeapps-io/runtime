namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1491 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.profiles", "dashboard", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("public.profiles", "home", c => c.Boolean(nullable: false));
            AddColumn("public.profiles", "startpage", c => c.String(defaultValue: "dashboard"));
        }
        
        public override void Down()
        {
            DropColumn("public.profiles", "startpage");
            DropColumn("public.profiles", "home");
            DropColumn("public.profiles", "dashboard");
        }
    }
}
