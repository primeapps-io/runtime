namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task8542 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.process_requests", "module", c => c.String());
            AddColumn("public.process_requests", "active", c => c.Boolean(nullable: false));
            CreateIndex("public.process_requests", "module");
            CreateIndex("public.process_requests", "active");
        }
        
        public override void Down()
        {
            DropIndex("public.process_requests", new[] { "active" });
            DropIndex("public.process_requests", new[] { "module" });
            DropColumn("public.process_requests", "active");
            DropColumn("public.process_requests", "module");
        }
    }
}
