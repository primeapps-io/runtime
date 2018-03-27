namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1502 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("public.fields", "placeholder", c => c.String(maxLength: 400));
        }
        
        public override void Down()
        {
            AlterColumn("public.fields", "placeholder", c => c.String(maxLength: 50));
        }
    }
}
