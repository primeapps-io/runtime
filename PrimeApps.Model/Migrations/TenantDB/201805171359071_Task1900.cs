namespace OfisimCRM.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1900 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("public.reports", "filter_logic", c => c.String(maxLength: 200));
            AlterColumn("public.views", "filter_logic", c => c.String(maxLength: 200));
        }
        
        public override void Down()
        {
            AlterColumn("public.views", "filter_logic", c => c.String(maxLength: 50));
            AlterColumn("public.reports", "filter_logic", c => c.String(maxLength: 50));
        }
    }
}
