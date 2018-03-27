namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task797 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.modules", "detail_view_type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.modules", "detail_view_type");
        }
    }
}
