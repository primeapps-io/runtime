namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task1244 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.fields", "image_size_list", c => c.Int(nullable: false));
            AddColumn("public.fields", "image_size_detail", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.fields", "image_size_detail");
            DropColumn("public.fields", "image_size_list");
        }
    }
}
