namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task620 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.workflow_webhooks",
                c => new
                    {
                        workflow_id = c.Int(nullable: false),
                        callback_url = c.String(nullable: false, maxLength: 500),
                        method_type = c.Int(nullable: false),
                        parameters = c.String(),
                    })
                .PrimaryKey(t => t.workflow_id)
                .ForeignKey("public.workflows", t => t.workflow_id, cascadeDelete: true)
                .Index(t => t.workflow_id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.workflow_webhooks", "workflow_id", "public.workflows");
            DropIndex("public.workflow_webhooks", new[] { "workflow_id" });
            DropTable("public.workflow_webhooks");
        }
    }
}
