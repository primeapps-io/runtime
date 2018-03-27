namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Task819 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.profiles", "tasks", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("public.profiles", "calendar", c => c.Boolean(nullable: false, defaultValue: true));
        }

        public override void Down()
        {
            DropColumn("public.profiles", "calendar");
            DropColumn("public.profiles", "tasks");
        }
    }
}
