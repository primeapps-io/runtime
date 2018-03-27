namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task986 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("public.notes", "text", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("public.notes", "text", c => c.String(nullable: false, maxLength: 500));
        }
    }
}
