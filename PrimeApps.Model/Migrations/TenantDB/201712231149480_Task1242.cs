namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1242 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.relations", "detail_view_type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.relations", "detail_view_type");
        }
    }
}
