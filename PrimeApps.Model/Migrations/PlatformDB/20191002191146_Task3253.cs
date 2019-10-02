using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    public partial class Task3253 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exchange_rates",
                schema: "public");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exchange_rates",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    date = table.Column<DateTime>(nullable: false),
                    day = table.Column<int>(nullable: false),
                    eur = table.Column<decimal>(nullable: false),
                    month = table.Column<int>(nullable: false),
                    usd = table.Column<decimal>(nullable: false),
                    year = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exchange_rates", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_date",
                schema: "public",
                table: "exchange_rates",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_day",
                schema: "public",
                table: "exchange_rates",
                column: "day");

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_month",
                schema: "public",
                table: "exchange_rates",
                column: "month");

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_year",
                schema: "public",
                table: "exchange_rates",
                column: "year");
        }
    }
}
