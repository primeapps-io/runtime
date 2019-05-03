using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Task3231 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "history_database",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    command_text = table.Column<string>(nullable: true),
                    table_name = table.Column<string>(nullable: true),
                    command_id = table.Column<Guid>(nullable: false),
                    executed_at = table.Column<DateTime>(nullable: true),
                    created_by = table.Column<string>(nullable: false),
                    deleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_history_database", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "history_storage",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    mime_type = table.Column<string>(nullable: true),
                    operation = table.Column<string>(nullable: true),
                    file_name = table.Column<string>(nullable: true),
                    unique_name = table.Column<string>(nullable: true),
                    path = table.Column<string>(nullable: true),
                    executed_at = table.Column<DateTime>(nullable: true),
                    created_by = table.Column<string>(nullable: false),
                    deleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_history_storage", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "history_database",
                schema: "public");

            migrationBuilder.DropTable(
                name: "history_storage",
                schema: "public");
        }
    }
}
