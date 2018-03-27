namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task621 : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.profiles", "export_data", c => c.Boolean(nullable: false));
            AddColumn("public.profiles", "import_data", c => c.Boolean(nullable: false));
            AddColumn("public.profiles", "word_pdf_download", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("public.profiles", "word_pdf_download");
            DropColumn("public.profiles", "import_data");
            DropColumn("public.profiles", "export_data");
        }
    }
}
