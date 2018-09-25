using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.PlatformDB
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "cache",
                schema: "public",
                columns: table => new
                {
                    key = table.Column<string>(maxLength: 100, nullable: false),
                    value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cache", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "exchange_rates",
                schema: "public",
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
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    email = table.Column<string>(nullable: false),
                    first_name = table.Column<string>(nullable: false),
                    last_name = table.Column<string>(nullable: false),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "apps",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_by = table.Column<int>(nullable: false),
                    updated_by = table.Column<int>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: true),
                    deleted = table.Column<bool>(nullable: false),
                    name = table.Column<string>(maxLength: 50, nullable: true),
                    label = table.Column<string>(maxLength: 400, nullable: true),
                    description = table.Column<string>(maxLength: 4000, nullable: true),
                    logo = table.Column<string>(nullable: true),
                    template_id = table.Column<int>(nullable: true),
                    use_tenant_settings = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apps", x => x.id);
                    table.ForeignKey(
                        name: "FK_apps_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_apps_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organizations",
                schema: "public",
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
                    label = table.Column<string>(maxLength: 50, nullable: true),
                    owner_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organizations", x => x.id);
                    table.ForeignKey(
                        name: "FK_organizations_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organizations_users_owner_id",
                        column: x => x.owner_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organizations_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_settings",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    phone = table.Column<string>(nullable: true),
                    culture = table.Column<string>(nullable: true),
                    currency = table.Column<string>(nullable: true),
                    time_zone = table.Column<string>(nullable: true),
                    language = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_settings", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_user_settings_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "warehouses",
                schema: "public",
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
                    table.PrimaryKey("PK_warehouses", x => x.id);
                    table.ForeignKey(
                        name: "FK_warehouses_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_warehouses_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "app_settings",
                schema: "public",
                columns: table => new
                {
                    app_id = table.Column<int>(nullable: false),
                    title = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    favicon = table.Column<string>(nullable: true),
                    color = table.Column<string>(nullable: true),
                    image = table.Column<string>(nullable: true),
                    domain = table.Column<string>(nullable: true),
                    auth_domain = table.Column<string>(nullable: true),
                    mail_sender_name = table.Column<string>(nullable: true),
                    mail_sender_email = table.Column<string>(nullable: true),
                    currency = table.Column<string>(nullable: true),
                    culture = table.Column<string>(nullable: true),
                    time_zone = table.Column<string>(nullable: true),
                    language = table.Column<string>(nullable: true),
                    banner = table.Column<string>(type: "jsonb", nullable: true),
                    google_analytics_code = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_settings", x => x.app_id);
                    table.ForeignKey(
                        name: "FK_app_settings_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "public",
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "app_templates",
                schema: "public",
                columns: table => new
                {
                    app_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 200, nullable: true),
                    subject = table.Column<string>(maxLength: 200, nullable: true),
                    content = table.Column<string>(nullable: true),
                    language = table.Column<string>(nullable: true),
                    type = table.Column<int>(nullable: false),
                    system_code = table.Column<string>(nullable: true),
                    active = table.Column<bool>(nullable: false),
                    mail_sender_name = table.Column<string>(nullable: true),
                    mail_sender_email = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_templates", x => x.app_id);
                    table.ForeignKey(
                        name: "FK_app_templates_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "public",
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "public",
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
                    owner_id = table.Column<int>(nullable: false),
                    use_user_settings = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                    table.ForeignKey(
                        name: "FK_tenants_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "public",
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenants_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenants_users_owner_id",
                        column: x => x.owner_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tenants_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflows",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_by = table.Column<int>(nullable: false),
                    updated_by = table.Column<int>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: true),
                    deleted = table.Column<bool>(nullable: false),
                    name = table.Column<string>(maxLength: 200, nullable: false),
                    active = table.Column<bool>(nullable: false),
                    frequency = table.Column<int>(nullable: false),
                    operations = table.Column<string>(maxLength: 50, nullable: false),
                    app_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflows", x => x.id);
                    table.ForeignKey(
                        name: "FK_workflows_apps_app_id",
                        column: x => x.app_id,
                        principalSchema: "public",
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflows_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflows_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_users",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_users_organizations_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_teams_organizations_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "public",
                        principalTable: "organizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_teams_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tenant_licenses",
                schema: "public",
                columns: table => new
                {
                    tenant_id = table.Column<int>(nullable: false),
                    user_license_count = table.Column<int>(nullable: false),
                    module_license_count = table.Column<int>(nullable: false),
                    analytics_license_count = table.Column<int>(nullable: false),
                    sip_license_count = table.Column<int>(nullable: false),
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
                        principalSchema: "public",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_settings",
                schema: "public",
                columns: table => new
                {
                    tenant_id = table.Column<int>(nullable: false),
                    currency = table.Column<string>(nullable: true),
                    culture = table.Column<string>(nullable: true),
                    time_zone = table.Column<string>(nullable: true),
                    language = table.Column<string>(nullable: true),
                    logo = table.Column<string>(nullable: true),
                    mail_sender_name = table.Column<string>(nullable: true),
                    mail_sender_email = table.Column<string>(nullable: true),
                    custom_domain = table.Column<string>(nullable: true),
                    custom_title = table.Column<string>(nullable: true),
                    custom_description = table.Column<string>(nullable: true),
                    custom_favicon = table.Column<string>(nullable: true),
                    custom_color = table.Column<string>(nullable: true),
                    custom_image = table.Column<string>(nullable: true),
                    has_sample_data = table.Column<bool>(nullable: false),
                    integration_email = table.Column<string>(nullable: true),
                    integration_password = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_settings", x => x.tenant_id);
                    table.ForeignKey(
                        name: "FK_tenant_settings_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "public",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tenants",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    tenant_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tenants", x => new { x.user_id, x.tenant_id });
                    table.ForeignKey(
                        name: "FK_user_tenants_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "public",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_tenants_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_logs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_by = table.Column<int>(nullable: false),
                    updated_by = table.Column<int>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: true),
                    deleted = table.Column<bool>(nullable: false),
                    workflow_id = table.Column<int>(nullable: false),
                    app_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_workflow_logs_workflows_workflow_id",
                        column: x => x.workflow_id,
                        principalSchema: "public",
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_logs_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_logs_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_webhooks",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    created_by = table.Column<int>(nullable: false),
                    updated_by = table.Column<int>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: true),
                    deleted = table.Column<bool>(nullable: false),
                    workflow_id = table.Column<int>(nullable: false),
                    callback_url = table.Column<string>(maxLength: 500, nullable: false),
                    method_type = table.Column<int>(nullable: false),
                    parameters = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_webhooks", x => x.id);
                    table.ForeignKey(
                        name: "FK_workflow_webhooks_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_webhooks_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workflow_webhooks_workflows_workflow_id",
                        column: x => x.workflow_id,
                        principalSchema: "public",
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "team_apps",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "apps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_team_apps_teams_team_id",
                        column: x => x.team_id,
                        principalSchema: "public",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "team_users",
                schema: "public",
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
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_team_users_teams_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_settings_app_id",
                schema: "public",
                table: "app_settings",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_app_settings_culture",
                schema: "public",
                table: "app_settings",
                column: "culture");

            migrationBuilder.CreateIndex(
                name: "IX_app_settings_currency",
                schema: "public",
                table: "app_settings",
                column: "currency");

            migrationBuilder.CreateIndex(
                name: "IX_app_settings_domain",
                schema: "public",
                table: "app_settings",
                column: "domain");

            migrationBuilder.CreateIndex(
                name: "IX_app_settings_language",
                schema: "public",
                table: "app_settings",
                column: "language");

            migrationBuilder.CreateIndex(
                name: "IX_app_settings_mail_sender_email",
                schema: "public",
                table: "app_settings",
                column: "mail_sender_email");

            migrationBuilder.CreateIndex(
                name: "IX_app_settings_mail_sender_name",
                schema: "public",
                table: "app_settings",
                column: "mail_sender_name");

            migrationBuilder.CreateIndex(
                name: "IX_app_settings_time_zone",
                schema: "public",
                table: "app_settings",
                column: "time_zone");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_active",
                schema: "public",
                table: "app_templates",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_app_id",
                schema: "public",
                table: "app_templates",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_language",
                schema: "public",
                table: "app_templates",
                column: "language");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_mail_sender_email",
                schema: "public",
                table: "app_templates",
                column: "mail_sender_email");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_mail_sender_name",
                schema: "public",
                table: "app_templates",
                column: "mail_sender_name");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_name",
                schema: "public",
                table: "app_templates",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_subject",
                schema: "public",
                table: "app_templates",
                column: "subject");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_system_code",
                schema: "public",
                table: "app_templates",
                column: "system_code");

            migrationBuilder.CreateIndex(
                name: "IX_app_templates_type",
                schema: "public",
                table: "app_templates",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_apps_created_at",
                schema: "public",
                table: "apps",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_apps_created_by",
                schema: "public",
                table: "apps",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_apps_deleted",
                schema: "public",
                table: "apps",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_apps_description",
                schema: "public",
                table: "apps",
                column: "description");

            migrationBuilder.CreateIndex(
                name: "IX_apps_label",
                schema: "public",
                table: "apps",
                column: "label");

            migrationBuilder.CreateIndex(
                name: "IX_apps_name",
                schema: "public",
                table: "apps",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_apps_template_id",
                schema: "public",
                table: "apps",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_apps_updated_at",
                schema: "public",
                table: "apps",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_apps_updated_by",
                schema: "public",
                table: "apps",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_apps_use_tenant_settings",
                schema: "public",
                table: "apps",
                column: "use_tenant_settings");

            migrationBuilder.CreateIndex(
                name: "IX_cache_key",
                schema: "public",
                table: "cache",
                column: "key");

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

            migrationBuilder.CreateIndex(
                name: "IX_organization_users_organization_id",
                schema: "public",
                table: "organization_users",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_users_user_id",
                schema: "public",
                table: "organization_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_created_at",
                schema: "public",
                table: "organizations",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_created_by",
                schema: "public",
                table: "organizations",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_deleted",
                schema: "public",
                table: "organizations",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_id",
                schema: "public",
                table: "organizations",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_label",
                schema: "public",
                table: "organizations",
                column: "label");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_name",
                schema: "public",
                table: "organizations",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_owner_id",
                schema: "public",
                table: "organizations",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_updated_at",
                schema: "public",
                table: "organizations",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_organizations_updated_by",
                schema: "public",
                table: "organizations",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_team_apps_app_id",
                schema: "public",
                table: "team_apps",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_apps_team_id",
                schema: "public",
                table: "team_apps",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_users_team_id",
                schema: "public",
                table: "team_users",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_users_user_id",
                schema: "public",
                table: "team_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_created_at",
                schema: "public",
                table: "teams",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_teams_created_by",
                schema: "public",
                table: "teams",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_teams_deleted",
                schema: "public",
                table: "teams",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_teams_id",
                schema: "public",
                table: "teams",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_name",
                schema: "public",
                table: "teams",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_teams_organization_id",
                schema: "public",
                table: "teams",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_teams_updated_at",
                schema: "public",
                table: "teams",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_teams_updated_by",
                schema: "public",
                table: "teams",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_licenses_deactivated_at",
                schema: "public",
                table: "tenant_licenses",
                column: "deactivated_at");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_licenses_is_deactivated",
                schema: "public",
                table: "tenant_licenses",
                column: "is_deactivated");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_licenses_is_paid_customer",
                schema: "public",
                table: "tenant_licenses",
                column: "is_paid_customer");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_licenses_is_suspended",
                schema: "public",
                table: "tenant_licenses",
                column: "is_suspended");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_licenses_suspended_at",
                schema: "public",
                table: "tenant_licenses",
                column: "suspended_at");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_licenses_tenant_id",
                schema: "public",
                table: "tenant_licenses",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_settings_culture",
                schema: "public",
                table: "tenant_settings",
                column: "culture");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_settings_currency",
                schema: "public",
                table: "tenant_settings",
                column: "currency");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_settings_custom_domain",
                schema: "public",
                table: "tenant_settings",
                column: "custom_domain");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_settings_language",
                schema: "public",
                table: "tenant_settings",
                column: "language");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_settings_mail_sender_email",
                schema: "public",
                table: "tenant_settings",
                column: "mail_sender_email");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_settings_mail_sender_name",
                schema: "public",
                table: "tenant_settings",
                column: "mail_sender_name");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_settings_tenant_id",
                schema: "public",
                table: "tenant_settings",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_settings_time_zone",
                schema: "public",
                table: "tenant_settings",
                column: "time_zone");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_app_id",
                schema: "public",
                table: "tenants",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_created_at",
                schema: "public",
                table: "tenants",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_created_by",
                schema: "public",
                table: "tenants",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_deleted",
                schema: "public",
                table: "tenants",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_guid_id",
                schema: "public",
                table: "tenants",
                column: "guid_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_owner_id",
                schema: "public",
                table: "tenants",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_updated_at",
                schema: "public",
                table: "tenants",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_updated_by",
                schema: "public",
                table: "tenants",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_use_user_settings",
                schema: "public",
                table: "tenants",
                column: "use_user_settings");

            migrationBuilder.CreateIndex(
                name: "IX_user_settings_culture",
                schema: "public",
                table: "user_settings",
                column: "culture");

            migrationBuilder.CreateIndex(
                name: "IX_user_settings_currency",
                schema: "public",
                table: "user_settings",
                column: "currency");

            migrationBuilder.CreateIndex(
                name: "IX_user_settings_language",
                schema: "public",
                table: "user_settings",
                column: "language");

            migrationBuilder.CreateIndex(
                name: "IX_user_settings_phone",
                schema: "public",
                table: "user_settings",
                column: "phone");

            migrationBuilder.CreateIndex(
                name: "IX_user_settings_time_zone",
                schema: "public",
                table: "user_settings",
                column: "time_zone");

            migrationBuilder.CreateIndex(
                name: "IX_user_tenants_tenant_id",
                schema: "public",
                table: "user_tenants",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_tenants_user_id",
                schema: "public",
                table: "user_tenants",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_created_at",
                schema: "public",
                table: "users",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "public",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_users_first_name",
                schema: "public",
                table: "users",
                column: "first_name");

            migrationBuilder.CreateIndex(
                name: "IX_users_id",
                schema: "public",
                table: "users",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_users_last_name",
                schema: "public",
                table: "users",
                column: "last_name");

            migrationBuilder.CreateIndex(
                name: "IX_users_updated_at",
                schema: "public",
                table: "users",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_warehouses_completed",
                schema: "public",
                table: "warehouses",
                column: "completed");

            migrationBuilder.CreateIndex(
                name: "IX_warehouses_created_by",
                schema: "public",
                table: "warehouses",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_warehouses_database_name",
                schema: "public",
                table: "warehouses",
                column: "database_name");

            migrationBuilder.CreateIndex(
                name: "IX_warehouses_tenant_id",
                schema: "public",
                table: "warehouses",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_warehouses_updated_by",
                schema: "public",
                table: "warehouses",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_app_id",
                schema: "public",
                table: "workflow_logs",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_workflow_id",
                schema: "public",
                table: "workflow_logs",
                column: "workflow_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_created_by",
                schema: "public",
                table: "workflow_logs",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_updated_by",
                schema: "public",
                table: "workflow_logs",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_webhooks_created_by",
                schema: "public",
                table: "workflow_webhooks",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_webhooks_updated_by",
                schema: "public",
                table: "workflow_webhooks",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_webhooks_workflow_id",
                schema: "public",
                table: "workflow_webhooks",
                column: "workflow_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflows_active",
                schema: "public",
                table: "workflows",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_app_id",
                schema: "public",
                table: "workflows",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_created_by",
                schema: "public",
                table: "workflows",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_updated_by",
                schema: "public",
                table: "workflows",
                column: "updated_by");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_settings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "app_templates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "cache",
                schema: "public");

            migrationBuilder.DropTable(
                name: "exchange_rates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "organization_users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "team_apps",
                schema: "public");

            migrationBuilder.DropTable(
                name: "team_users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tenant_licenses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tenant_settings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_settings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_tenants",
                schema: "public");

            migrationBuilder.DropTable(
                name: "warehouses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "workflow_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "workflow_webhooks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "teams",
                schema: "public");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "public");

            migrationBuilder.DropTable(
                name: "workflows",
                schema: "public");

            migrationBuilder.DropTable(
                name: "organizations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "apps",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");
        }
    }
}
