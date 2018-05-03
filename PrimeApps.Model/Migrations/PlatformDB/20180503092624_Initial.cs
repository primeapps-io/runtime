using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exchange_rates",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    usd = table.Column<decimal>(nullable: false),
                    eur = table.Column<decimal>(nullable: false),
                    date = table.Column<DateTime>(nullable: false),
                    year = table.Column<int>(nullable: false),
                    month = table.Column<int>(nullable: false),
                    day = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exchange_rates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    email = table.Column<string>(nullable: false),
                    first_name = table.Column<string>(nullable: false),
                    last_name = table.Column<string>(nullable: false),
                    culture = table.Column<string>(nullable: true),
                    currency = table.Column<string>(nullable: true),
                    phone = table.Column<string>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "apps",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_by = table.Column<int>(nullable: false),
                    updated_by = table.Column<int>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: true),
                    deleted = table.Column<bool>(nullable: false),
                    name = table.Column<string>(maxLength: 400, nullable: true),
                    description = table.Column<string>(maxLength: 4000, nullable: true),
                    logo = table.Column<string>(nullable: true),
                    template_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apps", x => x.id);
                    table.ForeignKey(
                        name: "FK_apps_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_apps_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_by = table.Column<int>(nullable: false),
                    updated_by = table.Column<int>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: true),
                    deleted = table.Column<bool>(nullable: false),
                    name = table.Column<string>(maxLength: 700, nullable: true),
                    owner = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.id);
                    table.ForeignKey(
                        name: "FK_organizations_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organizations_users_owner",
                        column: x => x.owner,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organizations_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "warehouse",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_by = table.Column<int>(nullable: false),
                    updated_by = table.Column<int>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: true),
                    deleted = table.Column<bool>(nullable: false),
                    tenant_id = table.Column<int>(nullable: false),
                    database_name = table.Column<string>(nullable: true),
                    database_user = table.Column<string>(nullable: true),
                    powerbi_workspace_id = table.Column<string>(nullable: true),
                    completed = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_warehouse", x => x.id);
                    table.ForeignKey(
                        name: "FK_warehouse_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_warehouse_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "app_settings",
                columns: table => new
                {
                    app_id = table.Column<int>(nullable: false),
                    title = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    favicon = table.Column<string>(nullable: true),
                    color = table.Column<string>(nullable: true),
                    image = table.Column<string>(nullable: true),
                    domain = table.Column<string>(nullable: true),
                    mail_sender_name = table.Column<string>(nullable: true),
                    mail_sender_email = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_settings", x => x.app_id);
                    table.ForeignKey(
                        name: "FK_app_settings_apps_app_id",
                        column: x => x.app_id,
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_by = table.Column<int>(nullable: false),
                    updated_by = table.Column<int>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: true),
                    deleted = table.Column<bool>(nullable: false),
                    app_id = table.Column<int>(nullable: false),
                    guid_id = table.Column<Guid>(nullable: false),
                    title = table.Column<string>(nullable: true),
                    owner_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenants_apps_app_id",
                        column: x => x.app_id,
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenants_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenants_users_owner_id",
                        column: x => x.owner_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenants_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_users",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    organization_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_users", x => new { x.user_id, x.organization_id });
                    table.ForeignKey(
                        name: "FK_organization_users_users_organization_id",
                        column: x => x.organization_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_users_organizations_user_id",
                        column: x => x.user_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_by = table.Column<int>(nullable: false),
                    updated_by = table.Column<int>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: true),
                    deleted = table.Column<bool>(nullable: false),
                    organization_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 700, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.id);
                    table.ForeignKey(
                        name: "FK_teams_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_teams_organizations_organization_id",
                        column: x => x.organization_id,
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_teams_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tenant_licenses",
                columns: table => new
                {
                    tenant_id = table.Column<int>(nullable: false),
                    user_license_count = table.Column<int>(nullable: false),
                    module_license_count = table.Column<int>(nullable: false),
                    analytics_license_count = table.Column<int>(nullable: false),
                    is_paid_customer = table.Column<bool>(nullable: false),
                    is_deactivated = table.Column<bool>(nullable: false),
                    is_suspended = table.Column<bool>(nullable: false),
                    deactivated_at = table.Column<DateTime>(nullable: true),
                    suspended_at = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_licenses", x => x.tenant_id);
                    table.ForeignKey(
                        name: "FK_tenant_licenses_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_settings",
                columns: table => new
                {
                    tenant_id = table.Column<int>(nullable: false),
                    currency = table.Column<string>(nullable: true),
                    language = table.Column<string>(nullable: true),
                    logo = table.Column<string>(nullable: true),
                    mail_sender_name = table.Column<string>(nullable: true),
                    mail_sender_email = table.Column<string>(nullable: true),
                    custom_domain = table.Column<string>(nullable: true),
                    custom_title = table.Column<string>(nullable: true),
                    custom_description = table.Column<string>(nullable: true),
                    custom_favicon = table.Column<string>(nullable: true),
                    custom_color = table.Column<string>(nullable: true),
                    custom_image = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_settings", x => x.tenant_id);
                    table.ForeignKey(
                        name: "FK_tenant_settings_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tenants",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    tenant_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tenants", x => new { x.user_id, x.tenant_id });
                    table.ForeignKey(
                        name: "FK_user_tenants_users_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_tenants_tenants_user_id",
                        column: x => x.user_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "team_apps",
                columns: table => new
                {
                    app_id = table.Column<int>(nullable: false),
                    team_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_apps", x => new { x.app_id, x.team_id });
                    table.ForeignKey(
                        name: "FK_team_apps_apps_app_id",
                        column: x => x.app_id,
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_team_apps_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "team_users",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    team_id = table.Column<int>(nullable: false),
                    role = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_users", x => new { x.user_id, x.team_id });
                    table.ForeignKey(
                        name: "FK_team_users_users_team_id",
                        column: x => x.team_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_team_users_teams_user_id",
                        column: x => x.user_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_settings_app_id",
                table: "app_settings",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_app_settings_domain",
                table: "app_settings",
                column: "domain");

            migrationBuilder.CreateIndex(
                name: "IX_app_settings_mail_sender_email",
                table: "app_settings",
                column: "mail_sender_email");

            migrationBuilder.CreateIndex(
                name: "IX_app_settings_mail_sender_name",
                table: "app_settings",
                column: "mail_sender_name");

            migrationBuilder.CreateIndex(
                name: "IX_apps_created_by",
                table: "apps",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_apps_description",
                table: "apps",
                column: "description");

            migrationBuilder.CreateIndex(
                name: "IX_apps_name",
                table: "apps",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_apps_template_id",
                table: "apps",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_apps_updated_by",
                table: "apps",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_date",
                table: "exchange_rates",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_day",
                table: "exchange_rates",
                column: "day");

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_month",
                table: "exchange_rates",
                column: "month");

            migrationBuilder.CreateIndex(
                name: "IX_exchange_rates_year",
                table: "exchange_rates",
                column: "year");

            migrationBuilder.CreateIndex(
                name: "IX_organization_users_organization_id",
                table: "organization_users",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_users_user_id",
                table: "organization_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_created_at",
                table: "organizations",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_created_by",
                table: "organizations",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_deleted",
                table: "organizations",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_id",
                table: "organizations",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_name",
                table: "organizations",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_owner",
                table: "organizations",
                column: "owner");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_updated_at",
                table: "organizations",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_updated_by",
                table: "organizations",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_team_apps_app_id",
                table: "team_apps",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_apps_team_id",
                table: "team_apps",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_users_team_id",
                table: "team_users",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_users_user_id",
                table: "team_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_created_at",
                table: "teams",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_teams_created_by",
                table: "teams",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_teams_deleted",
                table: "teams",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_teams_id",
                table: "teams",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_name",
                table: "teams",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_teams_organization_id",
                table: "teams",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_updated_at",
                table: "teams",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_teams_updated_by",
                table: "teams",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_licenses_deactivated_at",
                table: "tenant_licenses",
                column: "deactivated_at");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_licenses_is_deactivated",
                table: "tenant_licenses",
                column: "is_deactivated");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_licenses_is_paid_customer",
                table: "tenant_licenses",
                column: "is_paid_customer");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_licenses_is_suspended",
                table: "tenant_licenses",
                column: "is_suspended");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_licenses_suspended_at",
                table: "tenant_licenses",
                column: "suspended_at");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_licenses_tenant_id",
                table: "tenant_licenses",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_settings_custom_domain",
                table: "tenant_settings",
                column: "custom_domain");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_settings_mail_sender_email",
                table: "tenant_settings",
                column: "mail_sender_email");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_settings_mail_sender_name",
                table: "tenant_settings",
                column: "mail_sender_name");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_settings_tenant_id",
                table: "tenant_settings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_app_id",
                table: "tenants",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_created_at",
                table: "tenants",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_created_by",
                table: "tenants",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_deleted",
                table: "tenants",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_guid_id",
                table: "tenants",
                column: "guid_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_owner_id",
                table: "tenants",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_updated_at",
                table: "tenants",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_updated_by",
                table: "tenants",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_user_tenants_tenant_id",
                table: "user_tenants",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_tenants_user_id",
                table: "user_tenants",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_created_at",
                table: "users",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_users_culture",
                table: "users",
                column: "culture");

            migrationBuilder.CreateIndex(
                name: "IX_users_currency",
                table: "users",
                column: "currency");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_users_first_name",
                table: "users",
                column: "first_name");

            migrationBuilder.CreateIndex(
                name: "IX_users_id",
                table: "users",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_users_last_name",
                table: "users",
                column: "last_name");

            migrationBuilder.CreateIndex(
                name: "IX_users_updated_at",
                table: "users",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_completed",
                table: "warehouse",
                column: "completed");

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_created_by",
                table: "warehouse",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_database_name",
                table: "warehouse",
                column: "database_name");

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_tenant_id",
                table: "warehouse",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_updated_by",
                table: "warehouse",
                column: "updated_by");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_settings");

            migrationBuilder.DropTable(
                name: "exchange_rates");

            migrationBuilder.DropTable(
                name: "organization_users");

            migrationBuilder.DropTable(
                name: "team_apps");

            migrationBuilder.DropTable(
                name: "team_users");

            migrationBuilder.DropTable(
                name: "tenant_licenses");

            migrationBuilder.DropTable(
                name: "tenant_settings");

            migrationBuilder.DropTable(
                name: "user_tenants");

            migrationBuilder.DropTable(
                name: "warehouse");

            migrationBuilder.DropTable(
                name: "teams");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropTable(
                name: "organizations");

            migrationBuilder.DropTable(
                name: "apps");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
