namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task692 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.field_combinations", "combination_character", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("public.field_combinations", "combination_character");
        }
    }
}
