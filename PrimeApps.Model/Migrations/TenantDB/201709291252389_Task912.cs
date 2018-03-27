namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Task912 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.note_likes",
                c => new
                    {
                        note_id = c.Int(nullable: false),
                        user_id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.note_id, t.user_id })
                .ForeignKey("public.notes", t => t.note_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.user_id, cascadeDelete: true)
                .Index(t => t.note_id)
                .Index(t => t.user_id);
            
            AddColumn("public.profiles", "newsfeeed", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropForeignKey("public.note_likes", "user_id", "public.users");
            DropForeignKey("public.note_likes", "note_id", "public.notes");
            DropIndex("public.note_likes", new[] { "user_id" });
            DropIndex("public.note_likes", new[] { "note_id" });
            DropColumn("public.profiles", "newsfeeed");
            DropTable("public.note_likes");
        }
    }
}
