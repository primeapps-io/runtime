using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PrimeApps.Model.Migrations.TenantDB
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "action_button_permissions",
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
                    action_button_id = table.Column<int>(nullable: false),
                    profile_id = table.Column<int>(nullable: false),
                    type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_action_button_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "analytic_shares",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    analytic_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analytic_shares", x => new { x.user_id, x.analytic_id });
                });

            migrationBuilder.CreateTable(
                name: "dashlets",
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
                    name = table.Column<string>(maxLength: 50, nullable: false),
                    dashlet_area = table.Column<int>(nullable: false),
                    dashlet_type = table.Column<int>(nullable: false),
                    chart_id = table.Column<int>(nullable: true),
                    widget_id = table.Column<int>(nullable: true),
                    order = table.Column<int>(nullable: false),
                    x_tile_height = table.Column<int>(nullable: false),
                    y_tile_length = table.Column<int>(nullable: false),
                    dashboard_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dashlets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "field_combinations",
                schema: "public",
                columns: table => new
                {
                    field_id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    field1 = table.Column<string>(maxLength: 50, nullable: false),
                    field2 = table.Column<string>(maxLength: 50, nullable: false),
                    combination_character = table.Column<string>(maxLength: 50, nullable: true),
                    FieldId1 = table.Column<int>(nullable: true),
                    Combination = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_field_combinations", x => x.field_id);
                });

            migrationBuilder.CreateTable(
                name: "field_filters",
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
                    field_id = table.Column<int>(nullable: false),
                    filter_field = table.Column<string>(maxLength: 120, nullable: false),
                    Operator = table.Column<int>(nullable: false),
                    Value = table.Column<string>(maxLength: 100, nullable: false),
                    No = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_field_filters", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "field_permissions",
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
                    field_id = table.Column<int>(nullable: false),
                    profile_id = table.Column<int>(nullable: false),
                    type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_field_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "field_validations",
                schema: "public",
                columns: table => new
                {
                    field_id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    required = table.Column<bool>(nullable: true),
                    @readonly = table.Column<bool>(name: "readonly", nullable: true),
                    min_length = table.Column<short>(nullable: true),
                    max_length = table.Column<short>(nullable: true),
                    min = table.Column<double>(nullable: true),
                    max = table.Column<double>(nullable: true),
                    pattern = table.Column<string>(maxLength: 200, nullable: true),
                    unique = table.Column<bool>(nullable: true),
                    FieldId1 = table.Column<int>(nullable: true),
                    Validation = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_field_validations", x => x.field_id);
                });

            migrationBuilder.CreateTable(
                name: "action_buttons",
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
                    name = table.Column<string>(maxLength: 15, nullable: false),
                    template = table.Column<string>(nullable: false),
                    url = table.Column<string>(nullable: false),
                    icon = table.Column<string>(nullable: true),
                    css_class = table.Column<string>(nullable: true),
                    dependent_field = table.Column<string>(nullable: true),
                    dependent = table.Column<string>(nullable: true),
                    method_type = table.Column<int>(nullable: false),
                    parameters = table.Column<string>(nullable: true),
                    type = table.Column<int>(nullable: false),
                    trigger = table.Column<int>(nullable: false),
                    module_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_action_buttons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
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
                    audit_type = table.Column<int>(nullable: false),
                    module_id = table.Column<int>(nullable: true),
                    record_id = table.Column<int>(nullable: true),
                    record_name = table.Column<string>(maxLength: 50, nullable: true),
                    record_action_type = table.Column<int>(nullable: false),
                    setup_action_type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calculations",
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
                    module_id = table.Column<int>(nullable: false),
                    result_field = table.Column<string>(maxLength: 50, nullable: false),
                    field1 = table.Column<string>(maxLength: 50, nullable: true),
                    field2 = table.Column<string>(maxLength: 50, nullable: true),
                    custom_value = table.Column<double>(nullable: true),
                    @operator = table.Column<string>(name: "operator", maxLength: 1, nullable: false),
                    order = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "components",
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
                    name = table.Column<string>(maxLength: 15, nullable: false),
                    content = table.Column<string>(nullable: true),
                    type = table.Column<int>(nullable: false),
                    place = table.Column<int>(nullable: false),
                    module_id = table.Column<int>(nullable: false),
                    order = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_components", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "conversion_mappings",
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
                    module_id = table.Column<int>(nullable: false),
                    mapping_module_id = table.Column<int>(nullable: false),
                    field_id = table.Column<int>(nullable: false),
                    mapping_field_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversion_mappings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "conversion_sub_modules",
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
                    module_id = table.Column<int>(nullable: false),
                    sub_module_id = table.Column<int>(nullable: false),
                    submodule_source_field = table.Column<string>(nullable: true),
                    submodule_destination_field = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversion_sub_modules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dependencies",
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
                    module_id = table.Column<int>(nullable: false),
                    dependency_type = table.Column<int>(nullable: false),
                    parent_field = table.Column<string>(maxLength: 50, nullable: false),
                    child_field = table.Column<string>(maxLength: 50, nullable: true),
                    child_section = table.Column<string>(maxLength: 50, nullable: true),
                    values = table.Column<string>(maxLength: 4000, nullable: true),
                    field_map_parent = table.Column<string>(maxLength: 50, nullable: true),
                    field_map_child = table.Column<string>(maxLength: 50, nullable: true),
                    value_map = table.Column<string>(maxLength: 4000, nullable: true),
                    otherwise = table.Column<bool>(nullable: false),
                    clear = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dependencies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "documents",
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
                    name = table.Column<string>(nullable: true),
                    record_id = table.Column<int>(nullable: false),
                    module_id = table.Column<int>(nullable: false),
                    unique_name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    type = table.Column<string>(nullable: true),
                    file_size = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fields",
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
                    module_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 50, nullable: false),
                    system_type = table.Column<int>(nullable: false),
                    data_type = table.Column<int>(nullable: false),
                    order = table.Column<short>(nullable: false),
                    section = table.Column<string>(maxLength: 50, nullable: true),
                    section_column = table.Column<short>(nullable: false),
                    primary = table.Column<bool>(nullable: false),
                    default_value = table.Column<string>(maxLength: 500, nullable: true),
                    inline_edit = table.Column<bool>(nullable: false),
                    editable = table.Column<bool>(nullable: false),
                    show_label = table.Column<bool>(nullable: false),
                    multiline_type = table.Column<int>(nullable: false),
                    multiline_type_use_html = table.Column<bool>(nullable: false),
                    picklist_id = table.Column<int>(nullable: true),
                    picklist_sortorder = table.Column<int>(nullable: false),
                    lookup_type = table.Column<string>(maxLength: 50, nullable: true),
                    lookup_relation = table.Column<string>(maxLength: 50, nullable: true),
                    decimal_places = table.Column<short>(nullable: false),
                    rounding = table.Column<int>(nullable: false),
                    currency_symbol = table.Column<string>(maxLength: 10, nullable: true),
                    auto_number_prefix = table.Column<string>(maxLength: 10, nullable: true),
                    auto_number_suffix = table.Column<string>(maxLength: 10, nullable: true),
                    mask = table.Column<string>(maxLength: 100, nullable: true),
                    placeholder = table.Column<string>(maxLength: 400, nullable: true),
                    unique_combine = table.Column<string>(maxLength: 50, nullable: true),
                    address_type = table.Column<int>(nullable: false),
                    label_en = table.Column<string>(maxLength: 50, nullable: false),
                    label_tr = table.Column<string>(maxLength: 50, nullable: false),
                    display_list = table.Column<bool>(nullable: false),
                    display_form = table.Column<bool>(nullable: false),
                    display_detail = table.Column<bool>(nullable: false),
                    show_only_edit = table.Column<bool>(nullable: false),
                    style_label = table.Column<string>(maxLength: 400, nullable: true),
                    style_input = table.Column<string>(maxLength: 400, nullable: true),
                    calendar_date_type = table.Column<int>(nullable: false),
                    document_search = table.Column<bool>(nullable: false),
                    primary_lookup = table.Column<bool>(nullable: false),
                    custom_label = table.Column<string>(maxLength: 1000, nullable: true),
                    image_size_list = table.Column<int>(nullable: false),
                    image_size_detail = table.Column<int>(nullable: false),
                    view_type = table.Column<int>(nullable: false),
                    position = table.Column<int>(nullable: false),
                    show_as_dropdown = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fields", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "helps",
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
                    name = table.Column<string>(nullable: false),
                    template = table.Column<string>(nullable: false),
                    module_id = table.Column<int>(nullable: true),
                    modal_type = table.Column<int>(nullable: false),
                    show_type = table.Column<int>(nullable: false),
                    module_type = table.Column<int>(nullable: false),
                    route_url = table.Column<string>(nullable: true),
                    first_screen = table.Column<bool>(nullable: false),
                    custom_help = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_helps", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "imports",
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
                    module_id = table.Column<int>(nullable: false),
                    total_count = table.Column<int>(nullable: false),
                    excel_url = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_imports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "module_profile_settings",
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
                    module_id = table.Column<int>(nullable: false),
                    profiles = table.Column<string>(nullable: true),
                    label_en_singular = table.Column<string>(maxLength: 50, nullable: false),
                    label_en_plural = table.Column<string>(maxLength: 50, nullable: false),
                    label_tr_singular = table.Column<string>(maxLength: 50, nullable: false),
                    label_tr_plural = table.Column<string>(maxLength: 50, nullable: false),
                    menu_icon = table.Column<string>(maxLength: 100, nullable: true),
                    display = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_module_profile_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notes",
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
                    text = table.Column<string>(nullable: false),
                    module_id = table.Column<int>(nullable: true),
                    record_id = table.Column<int>(nullable: true),
                    note_id = table.Column<int>(nullable: true),
                    NoteId1 = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notes", x => x.id);
                    table.ForeignKey(
                        name: "FK_notes_notes_note_id",
                        column: x => x.note_id,
                        principalSchema: "public",
                        principalTable: "notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_notes_notes_NoteId1",
                        column: x => x.NoteId1,
                        principalSchema: "public",
                        principalTable: "notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
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
                    type = table.Column<int>(nullable: false),
                    module_id = table.Column<int>(nullable: false),
                    rev = table.Column<string>(nullable: true),
                    ids = table.Column<string>(nullable: false),
                    query = table.Column<string>(nullable: true),
                    status = table.Column<int>(nullable: false),
                    template = table.Column<string>(nullable: false),
                    lang = table.Column<string>(nullable: false),
                    queue_date = table.Column<DateTime>(nullable: false),
                    phone_field = table.Column<string>(maxLength: 50, nullable: true),
                    email_field = table.Column<string>(maxLength: 50, nullable: true),
                    sender_alias = table.Column<string>(maxLength: 50, nullable: true),
                    sender_email = table.Column<string>(maxLength: 50, nullable: true),
                    attachment_container = table.Column<string>(maxLength: 50, nullable: true),
                    subject = table.Column<string>(maxLength: 128, nullable: true),
                    attachment_link = table.Column<string>(maxLength: 500, nullable: true),
                    attachment_name = table.Column<string>(maxLength: 50, nullable: true),
                    result = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "processes",
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
                    module_id = table.Column<int>(nullable: false),
                    user_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 200, nullable: false),
                    frequency = table.Column<int>(nullable: false),
                    approver_type = table.Column<int>(nullable: false),
                    trigger_time = table.Column<int>(nullable: false),
                    approver_field = table.Column<string>(nullable: true),
                    active = table.Column<bool>(nullable: false),
                    operations = table.Column<string>(maxLength: 50, nullable: false),
                    profiles = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "profile_permissions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    type = table.Column<int>(nullable: false),
                    read = table.Column<bool>(nullable: false),
                    write = table.Column<bool>(nullable: false),
                    modify = table.Column<bool>(nullable: false),
                    remove = table.Column<bool>(nullable: false),
                    profile_id = table.Column<int>(nullable: false),
                    module_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profile_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "relations",
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
                    module_id = table.Column<int>(nullable: false),
                    related_module = table.Column<string>(nullable: false),
                    relation_type = table.Column<int>(nullable: false),
                    relation_field = table.Column<string>(maxLength: 50, nullable: true),
                    display_fields = table.Column<string>(maxLength: 1000, nullable: false),
                    @readonly = table.Column<bool>(name: "readonly", nullable: false),
                    order = table.Column<short>(nullable: false),
                    label_en_singular = table.Column<string>(maxLength: 50, nullable: false),
                    label_en_plural = table.Column<string>(maxLength: 50, nullable: false),
                    label_tr_singular = table.Column<string>(maxLength: 50, nullable: false),
                    label_tr_plural = table.Column<string>(maxLength: 50, nullable: false),
                    detail_view_type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reminders",
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
                    reminder_scope = table.Column<string>(maxLength: 70, nullable: false),
                    module_id = table.Column<int>(nullable: true),
                    record_id = table.Column<int>(nullable: true),
                    owner = table.Column<int>(nullable: true),
                    reminder_type = table.Column<string>(maxLength: 20, nullable: false),
                    reminder_start = table.Column<DateTime>(nullable: false),
                    reminder_end = table.Column<DateTime>(nullable: false),
                    subject = table.Column<string>(maxLength: 200, nullable: false),
                    reminder_frequency = table.Column<int>(nullable: true),
                    reminded_on = table.Column<DateTime>(nullable: true),
                    rev = table.Column<string>(maxLength: 30, nullable: true),
                    timezone_offset = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reports",
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
                    name = table.Column<string>(maxLength: 100, nullable: false),
                    report_type = table.Column<int>(nullable: false),
                    report_feed = table.Column<int>(nullable: false),
                    sql_function = table.Column<string>(nullable: true),
                    module_id = table.Column<int>(nullable: false),
                    user_id = table.Column<int>(nullable: true),
                    category_id = table.Column<int>(nullable: true),
                    group_field = table.Column<string>(nullable: true),
                    sort_field = table.Column<string>(nullable: true),
                    sort_direction = table.Column<int>(nullable: false),
                    sharing_type = table.Column<int>(nullable: false),
                    filter_logic = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sections",
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
                    module_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 50, nullable: false),
                    system_type = table.Column<int>(nullable: false),
                    order = table.Column<short>(nullable: false),
                    column_count = table.Column<short>(nullable: false),
                    label_en = table.Column<string>(maxLength: 50, nullable: false),
                    label_tr = table.Column<string>(maxLength: 50, nullable: false),
                    display_form = table.Column<bool>(nullable: false),
                    display_detail = table.Column<bool>(nullable: false),
                    custom_label = table.Column<string>(maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sections", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "view_states",
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
                    module_id = table.Column<int>(nullable: false),
                    user_id = table.Column<int>(nullable: false),
                    active_view = table.Column<int>(nullable: false),
                    sort_field = table.Column<string>(maxLength: 120, nullable: true),
                    sort_direction = table.Column<int>(nullable: false),
                    row_per_page = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_view_states", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "views",
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
                    module_id = table.Column<int>(nullable: false),
                    system_type = table.Column<int>(nullable: false),
                    label_en = table.Column<string>(maxLength: 50, nullable: false),
                    label_tr = table.Column<string>(maxLength: 50, nullable: false),
                    active = table.Column<bool>(nullable: false),
                    sharing_type = table.Column<int>(nullable: false),
                    filter_logic = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_views", x => x.id);
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
                    module_id = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 200, nullable: false),
                    frequency = table.Column<int>(nullable: false),
                    process_filter = table.Column<int>(nullable: false),
                    active = table.Column<bool>(nullable: false),
                    operations = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflows", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_notifications",
                schema: "public",
                columns: table => new
                {
                    workflow_id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    subject = table.Column<string>(maxLength: 200, nullable: false),
                    message = table.Column<string>(nullable: false),
                    recipients = table.Column<string>(maxLength: 4000, nullable: false),
                    cc = table.Column<string>(maxLength: 4000, nullable: true),
                    bcc = table.Column<string>(maxLength: 4000, nullable: true),
                    schedule = table.Column<int>(nullable: true),
                    WorkflowId1 = table.Column<int>(nullable: true),
                    SendNotification = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_notifications", x => x.workflow_id);
                    table.ForeignKey(
                        name: "FK_workflow_notifications_workflows_SendNotification",
                        column: x => x.SendNotification,
                        principalSchema: "public",
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_notifications_workflows_WorkflowId1",
                        column: x => x.WorkflowId1,
                        principalSchema: "public",
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_tasks",
                schema: "public",
                columns: table => new
                {
                    workflow_id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    owner = table.Column<int>(nullable: false),
                    subject = table.Column<string>(maxLength: 2000, nullable: false),
                    task_due_date = table.Column<int>(nullable: false),
                    task_status = table.Column<int>(nullable: true),
                    task_priority = table.Column<int>(nullable: true),
                    task_notification = table.Column<int>(nullable: true),
                    task_reminder = table.Column<DateTime>(nullable: true),
                    reminder_recurrence = table.Column<int>(nullable: true),
                    description = table.Column<string>(maxLength: 2000, nullable: true),
                    WorkflowId1 = table.Column<int>(nullable: true),
                    CreateTask = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_tasks", x => x.workflow_id);
                    table.ForeignKey(
                        name: "FK_workflow_tasks_workflows_CreateTask",
                        column: x => x.CreateTask,
                        principalSchema: "public",
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_tasks_workflows_WorkflowId1",
                        column: x => x.WorkflowId1,
                        principalSchema: "public",
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_updates",
                schema: "public",
                columns: table => new
                {
                    workflow_id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    module = table.Column<string>(maxLength: 120, nullable: false),
                    field = table.Column<string>(maxLength: 120, nullable: false),
                    value = table.Column<string>(maxLength: 2000, nullable: false),
                    WorkflowId1 = table.Column<int>(nullable: true),
                    FieldUpdate = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_updates", x => x.workflow_id);
                    table.ForeignKey(
                        name: "FK_workflow_updates_workflows_FieldUpdate",
                        column: x => x.FieldUpdate,
                        principalSchema: "public",
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_updates_workflows_WorkflowId1",
                        column: x => x.WorkflowId1,
                        principalSchema: "public",
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_webhooks",
                schema: "public",
                columns: table => new
                {
                    workflow_id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    callback_url = table.Column<string>(maxLength: 500, nullable: false),
                    method_type = table.Column<int>(nullable: false),
                    parameters = table.Column<string>(nullable: true),
                    WorkflowId1 = table.Column<int>(nullable: true),
                    WebHook = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_webhooks", x => x.workflow_id);
                    table.ForeignKey(
                        name: "FK_workflow_webhooks_workflows_WebHook",
                        column: x => x.WebHook,
                        principalSchema: "public",
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_webhooks_workflows_WorkflowId1",
                        column: x => x.WorkflowId1,
                        principalSchema: "public",
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "note_likes",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    note_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_note_likes", x => new { x.user_id, x.note_id });
                    table.ForeignKey(
                        name: "FK_note_likes_notes_note_id",
                        column: x => x.note_id,
                        principalSchema: "public",
                        principalTable: "notes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "picklist_items",
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
                    picklist_id = table.Column<int>(nullable: false),
                    label_en = table.Column<string>(maxLength: 100, nullable: false),
                    label_tr = table.Column<string>(maxLength: 100, nullable: false),
                    value = table.Column<string>(maxLength: 100, nullable: true),
                    value2 = table.Column<string>(maxLength: 100, nullable: true),
                    value3 = table.Column<string>(maxLength: 100, nullable: true),
                    system_code = table.Column<string>(maxLength: 50, nullable: true),
                    order = table.Column<short>(nullable: false),
                    inactive = table.Column<bool>(nullable: false),
                    migration_id = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_picklist_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "process_approvers",
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
                    process_id = table.Column<int>(nullable: false),
                    user_id = table.Column<int>(nullable: false),
                    order = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_approvers", x => x.id);
                    table.ForeignKey(
                        name: "FK_process_approvers_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "public",
                        principalTable: "processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "process_filters",
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
                    process_id = table.Column<int>(nullable: false),
                    field = table.Column<string>(maxLength: 120, nullable: false),
                    Operator = table.Column<int>(nullable: false),
                    Value = table.Column<string>(maxLength: 100, nullable: false),
                    No = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_filters", x => x.id);
                    table.ForeignKey(
                        name: "FK_process_filters_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "public",
                        principalTable: "processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "process_logs",
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
                    process_id = table.Column<int>(nullable: false),
                    module_id = table.Column<int>(nullable: false),
                    record_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_process_logs_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "public",
                        principalTable: "processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "process_requests",
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
                    process_id = table.Column<int>(nullable: false),
                    module = table.Column<string>(nullable: true),
                    record_id = table.Column<int>(nullable: false),
                    process_status = table.Column<int>(nullable: false),
                    operation_type = table.Column<int>(nullable: false),
                    process_status_order = table.Column<int>(nullable: false),
                    active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_process_requests_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "public",
                        principalTable: "processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dashboard",
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
                    name = table.Column<string>(maxLength: 50, nullable: false),
                    description = table.Column<string>(maxLength: 250, nullable: true),
                    user_id = table.Column<int>(nullable: true),
                    profile_id = table.Column<int>(nullable: true),
                    is_active = table.Column<bool>(nullable: false),
                    sharing_type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dashboard", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "section_permissions",
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
                    section_id = table.Column<int>(nullable: false),
                    profile_id = table.Column<int>(nullable: false),
                    type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_section_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_section_permissions_sections_section_id",
                        column: x => x.section_id,
                        principalSchema: "public",
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "template_permissions",
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
                    template_id = table.Column<int>(nullable: false),
                    profile_id = table.Column<int>(nullable: false),
                    type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false),
                    email = table.Column<string>(maxLength: 200, nullable: false),
                    first_name = table.Column<string>(maxLength: 200, nullable: false),
                    last_name = table.Column<string>(maxLength: 200, nullable: false),
                    full_name = table.Column<string>(maxLength: 400, nullable: false),
                    is_active = table.Column<bool>(nullable: false),
                    culture = table.Column<string>(maxLength: 10, nullable: true),
                    currency = table.Column<string>(maxLength: 3, nullable: true),
                    is_subscriber = table.Column<bool>(nullable: false),
                    created_by = table.Column<string>(nullable: true),
                    updated_by = table.Column<string>(nullable: true),
                    created_at = table.Column<DateTime>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: true),
                    deleted = table.Column<bool>(nullable: false),
                    picture = table.Column<string>(nullable: true),
                    profile_id = table.Column<int>(nullable: true),
                    role_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "analytics",
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
                    label = table.Column<string>(maxLength: 50, nullable: false),
                    powerbi_report_id = table.Column<string>(nullable: true),
                    pbix_url = table.Column<string>(nullable: false),
                    sharing_type = table.Column<int>(nullable: false),
                    menu_icon = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analytics", x => x.id);
                    table.ForeignKey(
                        name: "FK_analytics_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_analytics_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "changelogs",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    record_id = table.Column<int>(nullable: false),
                    record = table.Column<string>(nullable: true),
                    updated_by = table.Column<int>(nullable: false),
                    updated_at = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_changelogs", x => x.id);
                    table.ForeignKey(
                        name: "FK_changelogs_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "charts",
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
                    chart_type = table.Column<int>(nullable: false),
                    caption = table.Column<string>(maxLength: 100, nullable: false),
                    sub_caption = table.Column<string>(maxLength: 200, nullable: true),
                    theme = table.Column<int>(nullable: false),
                    x_axis_name = table.Column<string>(maxLength: 80, nullable: false),
                    y_axis_name = table.Column<string>(maxLength: 80, nullable: false),
                    report_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_charts", x => x.id);
                    table.ForeignKey(
                        name: "FK_charts_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_charts_reports_report_id",
                        column: x => x.report_id,
                        principalSchema: "public",
                        principalTable: "reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_charts_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "modules",
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
                    name = table.Column<string>(maxLength: 50, nullable: false),
                    system_type = table.Column<int>(nullable: false),
                    order = table.Column<short>(nullable: false),
                    display = table.Column<bool>(nullable: false),
                    sharing = table.Column<int>(nullable: false),
                    label_en_singular = table.Column<string>(maxLength: 50, nullable: false),
                    label_en_plural = table.Column<string>(maxLength: 50, nullable: false),
                    label_tr_singular = table.Column<string>(maxLength: 50, nullable: false),
                    label_tr_plural = table.Column<string>(maxLength: 50, nullable: false),
                    menu_icon = table.Column<string>(maxLength: 100, nullable: true),
                    location_enabled = table.Column<bool>(nullable: false),
                    display_calendar = table.Column<bool>(nullable: false),
                    calendar_color_dark = table.Column<string>(nullable: true),
                    calendar_color_light = table.Column<string>(nullable: true),
                    detail_view_type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modules", x => x.id);
                    table.ForeignKey(
                        name: "FK_modules_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_modules_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "picklists",
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
                    system_type = table.Column<int>(nullable: false),
                    label_en = table.Column<string>(maxLength: 50, nullable: false),
                    label_tr = table.Column<string>(maxLength: 50, nullable: false),
                    migration_id = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_picklists", x => x.id);
                    table.ForeignKey(
                        name: "FK_picklists_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_picklists_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "profiles",
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
                    name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true),
                    has_admin_rights = table.Column<bool>(nullable: false),
                    send_email = table.Column<bool>(nullable: false),
                    send_sms = table.Column<bool>(nullable: false),
                    lead_convert = table.Column<bool>(nullable: false),
                    export_data = table.Column<bool>(nullable: false),
                    import_data = table.Column<bool>(nullable: false),
                    word_pdf_download = table.Column<bool>(nullable: false),
                    document_search = table.Column<bool>(nullable: false),
                    business_intelligence = table.Column<bool>(nullable: false),
                    tasks = table.Column<bool>(nullable: false),
                    calendar = table.Column<bool>(nullable: false),
                    newsfeeed = table.Column<bool>(nullable: false),
                    is_persistent = table.Column<bool>(nullable: false),
                    report = table.Column<bool>(nullable: false),
                    dashboard = table.Column<bool>(nullable: false),
                    home = table.Column<bool>(nullable: false),
                    collective_annual_leave = table.Column<bool>(nullable: false),
                    startpage = table.Column<string>(nullable: true),
                    migration_id = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_profiles_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_profiles_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "report_aggregations",
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
                    report_id = table.Column<int>(nullable: false),
                    type = table.Column<int>(nullable: false),
                    field = table.Column<string>(maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_aggregations", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_aggregations_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_aggregations_reports_report_id",
                        column: x => x.report_id,
                        principalSchema: "public",
                        principalTable: "reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_aggregations_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "report_categories",
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
                    name = table.Column<string>(maxLength: 100, nullable: false),
                    order = table.Column<int>(nullable: false),
                    user_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_categories_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_categories_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_report_categories_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "report_fields",
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
                    report_id = table.Column<int>(nullable: false),
                    field = table.Column<string>(maxLength: 120, nullable: false),
                    order = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_fields", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_fields_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_fields_reports_report_id",
                        column: x => x.report_id,
                        principalSchema: "public",
                        principalTable: "reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_fields_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "report_filters",
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
                    report_id = table.Column<int>(nullable: false),
                    field = table.Column<string>(maxLength: 120, nullable: false),
                    Operator = table.Column<int>(nullable: false),
                    Value = table.Column<string>(maxLength: 100, nullable: false),
                    No = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_filters", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_filters_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_filters_reports_report_id",
                        column: x => x.report_id,
                        principalSchema: "public",
                        principalTable: "reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_filters_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "report_shares",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    report_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_shares", x => new { x.user_id, x.report_id });
                    table.ForeignKey(
                        name: "FK_report_shares_reports_report_id",
                        column: x => x.report_id,
                        principalSchema: "public",
                        principalTable: "reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_shares_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "roles",
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
                    label_en = table.Column<string>(maxLength: 200, nullable: false),
                    label_tr = table.Column<string>(maxLength: 200, nullable: false),
                    description_en = table.Column<string>(maxLength: 500, nullable: true),
                    description_tr = table.Column<string>(maxLength: 500, nullable: true),
                    master = table.Column<bool>(nullable: false),
                    owners = table.Column<string>(nullable: true),
                    migration_id = table.Column<string>(nullable: true),
                    share_data = table.Column<bool>(nullable: false),
                    reports_to_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_roles_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_roles_roles_reports_to_id",
                        column: x => x.reports_to_id,
                        principalSchema: "public",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_roles_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "settings",
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
                    type = table.Column<int>(nullable: false),
                    user_id = table.Column<int>(nullable: true),
                    key = table.Column<string>(nullable: false),
                    value = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_settings_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_settings_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_settings_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "templates",
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
                    template_type = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 200, nullable: true),
                    subject = table.Column<string>(maxLength: 200, nullable: true),
                    content = table.Column<string>(nullable: true),
                    language = table.Column<int>(nullable: false),
                    module = table.Column<string>(nullable: true),
                    active = table.Column<bool>(nullable: false),
                    sharing_type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_templates_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_templates_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_custom_shares",
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
                    user_id = table.Column<int>(nullable: false),
                    shared_user_id = table.Column<int>(nullable: false),
                    users = table.Column<string>(nullable: true),
                    user_groups = table.Column<string>(nullable: true),
                    modules = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_custom_shares", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_custom_shares_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_custom_shares_users_shared_user_id",
                        column: x => x.shared_user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_custom_shares_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_custom_shares_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_groups",
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
                    name = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_groups_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_groups_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "view_fields",
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
                    view_id = table.Column<int>(nullable: false),
                    field = table.Column<string>(maxLength: 120, nullable: false),
                    order = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_view_fields", x => x.id);
                    table.ForeignKey(
                        name: "FK_view_fields_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_view_fields_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_view_fields_views_view_id",
                        column: x => x.view_id,
                        principalSchema: "public",
                        principalTable: "views",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "view_filters",
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
                    view_id = table.Column<int>(nullable: false),
                    field = table.Column<string>(maxLength: 120, nullable: false),
                    Operator = table.Column<int>(nullable: false),
                    Value = table.Column<string>(maxLength: 100, nullable: false),
                    No = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_view_filters", x => x.id);
                    table.ForeignKey(
                        name: "FK_view_filters_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_view_filters_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_view_filters_views_view_id",
                        column: x => x.view_id,
                        principalSchema: "public",
                        principalTable: "views",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "view_shares",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    view_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_view_shares", x => new { x.user_id, x.view_id });
                    table.ForeignKey(
                        name: "FK_view_shares_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_view_shares_views_view_id",
                        column: x => x.view_id,
                        principalSchema: "public",
                        principalTable: "views",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "widgets",
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
                    widget_type = table.Column<int>(nullable: false),
                    name = table.Column<string>(maxLength: 200, nullable: false),
                    load_url = table.Column<string>(maxLength: 100, nullable: true),
                    color = table.Column<string>(maxLength: 30, nullable: true),
                    icon = table.Column<string>(maxLength: 30, nullable: true),
                    report_id = table.Column<int>(nullable: true),
                    view_id = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_widgets", x => x.id);
                    table.ForeignKey(
                        name: "FK_widgets_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_widgets_reports_report_id",
                        column: x => x.report_id,
                        principalSchema: "public",
                        principalTable: "reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_widgets_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_widgets_views_view_id",
                        column: x => x.view_id,
                        principalSchema: "public",
                        principalTable: "views",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "workflow_filters",
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
                    field = table.Column<string>(maxLength: 120, nullable: false),
                    Operator = table.Column<int>(nullable: false),
                    Value = table.Column<string>(maxLength: 100, nullable: false),
                    No = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_filters", x => x.id);
                    table.ForeignKey(
                        name: "FK_workflow_filters_users_created_by",
                        column: x => x.created_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_filters_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workflow_filters_workflows_workflow_id",
                        column: x => x.workflow_id,
                        principalSchema: "public",
                        principalTable: "workflows",
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
                    module_id = table.Column<int>(nullable: false),
                    record_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_logs", x => x.id);
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
                    table.ForeignKey(
                        name: "FK_workflow_logs_workflows_workflow_id",
                        column: x => x.workflow_id,
                        principalSchema: "public",
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "template_shares",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    template_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_shares", x => new { x.user_id, x.template_id });
                    table.ForeignKey(
                        name: "FK_template_shares_templates_template_id",
                        column: x => x.template_id,
                        principalSchema: "public",
                        principalTable: "templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_template_shares_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_user_groups",
                schema: "public",
                columns: table => new
                {
                    user_id = table.Column<int>(nullable: false),
                    group_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_user_groups", x => new { x.user_id, x.group_id });
                    table.ForeignKey(
                        name: "FK_users_user_groups_user_groups_group_id",
                        column: x => x.group_id,
                        principalSchema: "public",
                        principalTable: "user_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_users_user_groups_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "public",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_action_button_permissions_created_at",
                schema: "public",
                table: "action_button_permissions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_action_button_permissions_created_by",
                schema: "public",
                table: "action_button_permissions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_action_button_permissions_deleted",
                schema: "public",
                table: "action_button_permissions",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_action_button_permissions_profile_id",
                schema: "public",
                table: "action_button_permissions",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_action_button_permissions_updated_at",
                schema: "public",
                table: "action_button_permissions",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_action_button_permissions_updated_by",
                schema: "public",
                table: "action_button_permissions",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "action_button_permissions_IX_action_button_id_profile_id",
                schema: "public",
                table: "action_button_permissions",
                columns: new[] { "action_button_id", "profile_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_action_buttons_created_at",
                schema: "public",
                table: "action_buttons",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_action_buttons_created_by",
                schema: "public",
                table: "action_buttons",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_action_buttons_deleted",
                schema: "public",
                table: "action_buttons",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_action_buttons_module_id",
                schema: "public",
                table: "action_buttons",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_action_buttons_updated_at",
                schema: "public",
                table: "action_buttons",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_action_buttons_updated_by",
                schema: "public",
                table: "action_buttons",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_analytic_shares_analytic_id",
                schema: "public",
                table: "analytic_shares",
                column: "analytic_id");

            migrationBuilder.CreateIndex(
                name: "IX_analytics_created_at",
                schema: "public",
                table: "analytics",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_analytics_created_by",
                schema: "public",
                table: "analytics",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_analytics_deleted",
                schema: "public",
                table: "analytics",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_analytics_powerbi_report_id",
                schema: "public",
                table: "analytics",
                column: "powerbi_report_id");

            migrationBuilder.CreateIndex(
                name: "IX_analytics_sharing_type",
                schema: "public",
                table: "analytics",
                column: "sharing_type");

            migrationBuilder.CreateIndex(
                name: "IX_analytics_updated_at",
                schema: "public",
                table: "analytics",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_analytics_updated_by",
                schema: "public",
                table: "analytics",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_created_at",
                schema: "public",
                table: "audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_created_by",
                schema: "public",
                table: "audit_logs",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_deleted",
                schema: "public",
                table: "audit_logs",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_module_id",
                schema: "public",
                table: "audit_logs",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_updated_at",
                schema: "public",
                table: "audit_logs",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_updated_by",
                schema: "public",
                table: "audit_logs",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_calculations_created_at",
                schema: "public",
                table: "calculations",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_calculations_created_by",
                schema: "public",
                table: "calculations",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_calculations_deleted",
                schema: "public",
                table: "calculations",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_calculations_module_id",
                schema: "public",
                table: "calculations",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculations_updated_at",
                schema: "public",
                table: "calculations",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_calculations_updated_by",
                schema: "public",
                table: "calculations",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_changelogs_updated_by",
                schema: "public",
                table: "changelogs",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_charts_created_at",
                schema: "public",
                table: "charts",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_charts_created_by",
                schema: "public",
                table: "charts",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_charts_deleted",
                schema: "public",
                table: "charts",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_charts_report_id",
                schema: "public",
                table: "charts",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_charts_updated_at",
                schema: "public",
                table: "charts",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_charts_updated_by",
                schema: "public",
                table: "charts",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_components_created_at",
                schema: "public",
                table: "components",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_components_created_by",
                schema: "public",
                table: "components",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_components_deleted",
                schema: "public",
                table: "components",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_components_module_id",
                schema: "public",
                table: "components",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_components_updated_at",
                schema: "public",
                table: "components",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_components_updated_by",
                schema: "public",
                table: "components",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_conversion_mappings_created_at",
                schema: "public",
                table: "conversion_mappings",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_conversion_mappings_created_by",
                schema: "public",
                table: "conversion_mappings",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_conversion_mappings_deleted",
                schema: "public",
                table: "conversion_mappings",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_conversion_mappings_mapping_module_id",
                schema: "public",
                table: "conversion_mappings",
                column: "mapping_module_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversion_mappings_module_id",
                schema: "public",
                table: "conversion_mappings",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversion_mappings_updated_at",
                schema: "public",
                table: "conversion_mappings",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_conversion_mappings_updated_by",
                schema: "public",
                table: "conversion_mappings",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_conversion_sub_modules_created_at",
                schema: "public",
                table: "conversion_sub_modules",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_conversion_sub_modules_created_by",
                schema: "public",
                table: "conversion_sub_modules",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_conversion_sub_modules_deleted",
                schema: "public",
                table: "conversion_sub_modules",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_conversion_sub_modules_sub_module_id",
                schema: "public",
                table: "conversion_sub_modules",
                column: "sub_module_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversion_sub_modules_module_id",
                schema: "public",
                table: "conversion_sub_modules",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversion_sub_modules_updated_at",
                schema: "public",
                table: "conversion_sub_modules",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_conversion_sub_modules_updated_by",
                schema: "public",
                table: "conversion_sub_modules",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_dashboard_created_at",
                schema: "public",
                table: "dashboard",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_dashboard_created_by",
                schema: "public",
                table: "dashboard",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_dashboard_deleted",
                schema: "public",
                table: "dashboard",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_dashboard_profile_id",
                schema: "public",
                table: "dashboard",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_dashboard_sharing_type",
                schema: "public",
                table: "dashboard",
                column: "sharing_type");

            migrationBuilder.CreateIndex(
                name: "IX_dashboard_updated_at",
                schema: "public",
                table: "dashboard",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_dashboard_updated_by",
                schema: "public",
                table: "dashboard",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_dashboard_user_id",
                schema: "public",
                table: "dashboard",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_dashlets_chart_id",
                schema: "public",
                table: "dashlets",
                column: "chart_id");

            migrationBuilder.CreateIndex(
                name: "IX_dashlets_created_at",
                schema: "public",
                table: "dashlets",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_dashlets_created_by",
                schema: "public",
                table: "dashlets",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_dashlets_dashboard_id",
                schema: "public",
                table: "dashlets",
                column: "dashboard_id");

            migrationBuilder.CreateIndex(
                name: "IX_dashlets_deleted",
                schema: "public",
                table: "dashlets",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_dashlets_updated_at",
                schema: "public",
                table: "dashlets",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_dashlets_updated_by",
                schema: "public",
                table: "dashlets",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_dashlets_widget_id",
                schema: "public",
                table: "dashlets",
                column: "widget_id");

            migrationBuilder.CreateIndex(
                name: "IX_dependencies_created_at",
                schema: "public",
                table: "dependencies",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_dependencies_created_by",
                schema: "public",
                table: "dependencies",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_dependencies_deleted",
                schema: "public",
                table: "dependencies",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_dependencies_module_id",
                schema: "public",
                table: "dependencies",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_dependencies_updated_at",
                schema: "public",
                table: "dependencies",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_dependencies_updated_by",
                schema: "public",
                table: "dependencies",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_documents_created_at",
                schema: "public",
                table: "documents",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_documents_created_by",
                schema: "public",
                table: "documents",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_documents_deleted",
                schema: "public",
                table: "documents",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_documents_module_id",
                schema: "public",
                table: "documents",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_documents_updated_at",
                schema: "public",
                table: "documents",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_documents_updated_by",
                schema: "public",
                table: "documents",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_field_combinations_Combination",
                schema: "public",
                table: "field_combinations",
                column: "Combination",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_field_combinations_FieldId1",
                schema: "public",
                table: "field_combinations",
                column: "FieldId1");

            migrationBuilder.CreateIndex(
                name: "IX_field_filters_created_by",
                schema: "public",
                table: "field_filters",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_field_filters_field_id",
                schema: "public",
                table: "field_filters",
                column: "field_id");

            migrationBuilder.CreateIndex(
                name: "IX_field_filters_updated_by",
                schema: "public",
                table: "field_filters",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_field_permissions_created_at",
                schema: "public",
                table: "field_permissions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_field_permissions_created_by",
                schema: "public",
                table: "field_permissions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_field_permissions_deleted",
                schema: "public",
                table: "field_permissions",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_field_permissions_profile_id",
                schema: "public",
                table: "field_permissions",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_field_permissions_updated_at",
                schema: "public",
                table: "field_permissions",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_field_permissions_updated_by",
                schema: "public",
                table: "field_permissions",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "field_permissions_IX_field_id_profile_id",
                schema: "public",
                table: "field_permissions",
                columns: new[] { "field_id", "profile_id" });

            migrationBuilder.CreateIndex(
                name: "IX_field_validations_FieldId1",
                schema: "public",
                table: "field_validations",
                column: "FieldId1");

            migrationBuilder.CreateIndex(
                name: "IX_field_validations_Validation",
                schema: "public",
                table: "field_validations",
                column: "Validation",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fields_created_at",
                schema: "public",
                table: "fields",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_fields_created_by",
                schema: "public",
                table: "fields",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_fields_deleted",
                schema: "public",
                table: "fields",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_fields_picklist_id",
                schema: "public",
                table: "fields",
                column: "picklist_id");

            migrationBuilder.CreateIndex(
                name: "IX_fields_updated_at",
                schema: "public",
                table: "fields",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_fields_updated_by",
                schema: "public",
                table: "fields",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "fields_IX_module_id_name",
                schema: "public",
                table: "fields",
                columns: new[] { "module_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_helps_created_at",
                schema: "public",
                table: "helps",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_helps_created_by",
                schema: "public",
                table: "helps",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_helps_deleted",
                schema: "public",
                table: "helps",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_helps_module_id",
                schema: "public",
                table: "helps",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_helps_updated_at",
                schema: "public",
                table: "helps",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_helps_updated_by",
                schema: "public",
                table: "helps",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_imports_created_at",
                schema: "public",
                table: "imports",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_imports_created_by",
                schema: "public",
                table: "imports",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_imports_deleted",
                schema: "public",
                table: "imports",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_imports_module_id",
                schema: "public",
                table: "imports",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_imports_updated_at",
                schema: "public",
                table: "imports",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_imports_updated_by",
                schema: "public",
                table: "imports",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_module_profile_settings_created_at",
                schema: "public",
                table: "module_profile_settings",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_module_profile_settings_created_by",
                schema: "public",
                table: "module_profile_settings",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_module_profile_settings_deleted",
                schema: "public",
                table: "module_profile_settings",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_module_profile_settings_module_id",
                schema: "public",
                table: "module_profile_settings",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_module_profile_settings_updated_at",
                schema: "public",
                table: "module_profile_settings",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_module_profile_settings_updated_by",
                schema: "public",
                table: "module_profile_settings",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_modules_created_at",
                schema: "public",
                table: "modules",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_modules_created_by",
                schema: "public",
                table: "modules",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_modules_deleted",
                schema: "public",
                table: "modules",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_modules_name",
                schema: "public",
                table: "modules",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_modules_updated_at",
                schema: "public",
                table: "modules",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_modules_updated_by",
                schema: "public",
                table: "modules",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_note_likes_note_id",
                schema: "public",
                table: "note_likes",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "IX_notes_created_at",
                schema: "public",
                table: "notes",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_notes_created_by",
                schema: "public",
                table: "notes",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_notes_deleted",
                schema: "public",
                table: "notes",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_notes_module_id",
                schema: "public",
                table: "notes",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_notes_note_id",
                schema: "public",
                table: "notes",
                column: "note_id");

            migrationBuilder.CreateIndex(
                name: "IX_notes_NoteId1",
                schema: "public",
                table: "notes",
                column: "NoteId1");

            migrationBuilder.CreateIndex(
                name: "IX_notes_record_id",
                schema: "public",
                table: "notes",
                column: "record_id");

            migrationBuilder.CreateIndex(
                name: "IX_notes_updated_at",
                schema: "public",
                table: "notes",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_notes_updated_by",
                schema: "public",
                table: "notes",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_created_at",
                schema: "public",
                table: "notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_created_by",
                schema: "public",
                table: "notifications",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_deleted",
                schema: "public",
                table: "notifications",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_module_id",
                schema: "public",
                table: "notifications",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_type",
                schema: "public",
                table: "notifications",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_updated_at",
                schema: "public",
                table: "notifications",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_updated_by",
                schema: "public",
                table: "notifications",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_picklist_items_created_at",
                schema: "public",
                table: "picklist_items",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_picklist_items_created_by",
                schema: "public",
                table: "picklist_items",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_picklist_items_deleted",
                schema: "public",
                table: "picklist_items",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_picklist_items_picklist_id",
                schema: "public",
                table: "picklist_items",
                column: "picklist_id");

            migrationBuilder.CreateIndex(
                name: "IX_picklist_items_system_code",
                schema: "public",
                table: "picklist_items",
                column: "system_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_picklist_items_updated_at",
                schema: "public",
                table: "picklist_items",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_picklist_items_updated_by",
                schema: "public",
                table: "picklist_items",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_picklists_created_at",
                schema: "public",
                table: "picklists",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_picklists_created_by",
                schema: "public",
                table: "picklists",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_picklists_deleted",
                schema: "public",
                table: "picklists",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_picklists_label_en",
                schema: "public",
                table: "picklists",
                column: "label_en",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_picklists_label_tr",
                schema: "public",
                table: "picklists",
                column: "label_tr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_picklists_updated_at",
                schema: "public",
                table: "picklists",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_picklists_updated_by",
                schema: "public",
                table: "picklists",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_process_approvers_created_at",
                schema: "public",
                table: "process_approvers",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_process_approvers_created_by",
                schema: "public",
                table: "process_approvers",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_process_approvers_deleted",
                schema: "public",
                table: "process_approvers",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_process_approvers_process_id",
                schema: "public",
                table: "process_approvers",
                column: "process_id");

            migrationBuilder.CreateIndex(
                name: "IX_process_approvers_updated_at",
                schema: "public",
                table: "process_approvers",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_process_approvers_updated_by",
                schema: "public",
                table: "process_approvers",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_process_approvers_user_id",
                schema: "public",
                table: "process_approvers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_process_filters_created_at",
                schema: "public",
                table: "process_filters",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_process_filters_created_by",
                schema: "public",
                table: "process_filters",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_process_filters_deleted",
                schema: "public",
                table: "process_filters",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_process_filters_process_id",
                schema: "public",
                table: "process_filters",
                column: "process_id");

            migrationBuilder.CreateIndex(
                name: "IX_process_filters_updated_at",
                schema: "public",
                table: "process_filters",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_process_filters_updated_by",
                schema: "public",
                table: "process_filters",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_process_logs_created_at",
                schema: "public",
                table: "process_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_process_logs_created_by",
                schema: "public",
                table: "process_logs",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_process_logs_deleted",
                schema: "public",
                table: "process_logs",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_process_logs_module_id",
                schema: "public",
                table: "process_logs",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_process_logs_process_id",
                schema: "public",
                table: "process_logs",
                column: "process_id");

            migrationBuilder.CreateIndex(
                name: "IX_process_logs_record_id",
                schema: "public",
                table: "process_logs",
                column: "record_id");

            migrationBuilder.CreateIndex(
                name: "IX_process_logs_updated_at",
                schema: "public",
                table: "process_logs",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_process_logs_updated_by",
                schema: "public",
                table: "process_logs",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_process_requests_active",
                schema: "public",
                table: "process_requests",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_process_requests_created_at",
                schema: "public",
                table: "process_requests",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_process_requests_created_by",
                schema: "public",
                table: "process_requests",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_process_requests_deleted",
                schema: "public",
                table: "process_requests",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_process_requests_module",
                schema: "public",
                table: "process_requests",
                column: "module");

            migrationBuilder.CreateIndex(
                name: "IX_process_requests_operation_type",
                schema: "public",
                table: "process_requests",
                column: "operation_type");

            migrationBuilder.CreateIndex(
                name: "IX_process_requests_process_id",
                schema: "public",
                table: "process_requests",
                column: "process_id");

            migrationBuilder.CreateIndex(
                name: "IX_process_requests_process_status_order",
                schema: "public",
                table: "process_requests",
                column: "process_status_order");

            migrationBuilder.CreateIndex(
                name: "IX_process_requests_record_id",
                schema: "public",
                table: "process_requests",
                column: "record_id");

            migrationBuilder.CreateIndex(
                name: "IX_process_requests_process_status",
                schema: "public",
                table: "process_requests",
                column: "process_status");

            migrationBuilder.CreateIndex(
                name: "IX_process_requests_updated_at",
                schema: "public",
                table: "process_requests",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_process_requests_updated_by",
                schema: "public",
                table: "process_requests",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_processes_active",
                schema: "public",
                table: "processes",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_processes_created_at",
                schema: "public",
                table: "processes",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_processes_created_by",
                schema: "public",
                table: "processes",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_processes_deleted",
                schema: "public",
                table: "processes",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_processes_module_id",
                schema: "public",
                table: "processes",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_processes_updated_at",
                schema: "public",
                table: "processes",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_processes_updated_by",
                schema: "public",
                table: "processes",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_profile_permissions_module_id",
                schema: "public",
                table: "profile_permissions",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_profile_permissions_profile_id",
                schema: "public",
                table: "profile_permissions",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_created_by",
                schema: "public",
                table: "profiles",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_updated_by",
                schema: "public",
                table: "profiles",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_relations_created_at",
                schema: "public",
                table: "relations",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_relations_created_by",
                schema: "public",
                table: "relations",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_relations_deleted",
                schema: "public",
                table: "relations",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_relations_module_id",
                schema: "public",
                table: "relations",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_relations_updated_at",
                schema: "public",
                table: "relations",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_relations_updated_by",
                schema: "public",
                table: "relations",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_reminders_created_at",
                schema: "public",
                table: "reminders",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_reminders_created_by",
                schema: "public",
                table: "reminders",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_reminders_deleted",
                schema: "public",
                table: "reminders",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_reminders_module_id",
                schema: "public",
                table: "reminders",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_reminders_reminder_scope",
                schema: "public",
                table: "reminders",
                column: "reminder_scope");

            migrationBuilder.CreateIndex(
                name: "IX_reminders_reminder_type",
                schema: "public",
                table: "reminders",
                column: "reminder_type");

            migrationBuilder.CreateIndex(
                name: "IX_reminders_updated_at",
                schema: "public",
                table: "reminders",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_reminders_updated_by",
                schema: "public",
                table: "reminders",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_report_aggregations_created_by",
                schema: "public",
                table: "report_aggregations",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_report_aggregations_report_id",
                schema: "public",
                table: "report_aggregations",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_aggregations_updated_by",
                schema: "public",
                table: "report_aggregations",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_report_categories_created_at",
                schema: "public",
                table: "report_categories",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_report_categories_created_by",
                schema: "public",
                table: "report_categories",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_report_categories_deleted",
                schema: "public",
                table: "report_categories",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_report_categories_updated_at",
                schema: "public",
                table: "report_categories",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_report_categories_updated_by",
                schema: "public",
                table: "report_categories",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_report_categories_user_id",
                schema: "public",
                table: "report_categories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_fields_created_by",
                schema: "public",
                table: "report_fields",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_report_fields_report_id",
                schema: "public",
                table: "report_fields",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_fields_updated_by",
                schema: "public",
                table: "report_fields",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_report_filters_created_by",
                schema: "public",
                table: "report_filters",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_report_filters_report_id",
                schema: "public",
                table: "report_filters",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_filters_updated_by",
                schema: "public",
                table: "report_filters",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_report_shares_report_id",
                schema: "public",
                table: "report_shares",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_reports_category_id",
                schema: "public",
                table: "reports",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_reports_created_at",
                schema: "public",
                table: "reports",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_reports_created_by",
                schema: "public",
                table: "reports",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_reports_deleted",
                schema: "public",
                table: "reports",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_reports_module_id",
                schema: "public",
                table: "reports",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_reports_sharing_type",
                schema: "public",
                table: "reports",
                column: "sharing_type");

            migrationBuilder.CreateIndex(
                name: "IX_reports_updated_at",
                schema: "public",
                table: "reports",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_reports_updated_by",
                schema: "public",
                table: "reports",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_reports_user_id",
                schema: "public",
                table: "reports",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_created_by",
                schema: "public",
                table: "roles",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_roles_reports_to_id",
                schema: "public",
                table: "roles",
                column: "reports_to_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_updated_by",
                schema: "public",
                table: "roles",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_section_permissions_created_at",
                schema: "public",
                table: "section_permissions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_section_permissions_created_by",
                schema: "public",
                table: "section_permissions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_section_permissions_deleted",
                schema: "public",
                table: "section_permissions",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_section_permissions_profile_id",
                schema: "public",
                table: "section_permissions",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_section_permissions_updated_at",
                schema: "public",
                table: "section_permissions",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_section_permissions_updated_by",
                schema: "public",
                table: "section_permissions",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "section_permissions_IX_section_id_profile_id",
                schema: "public",
                table: "section_permissions",
                columns: new[] { "section_id", "profile_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sections_created_at",
                schema: "public",
                table: "sections",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_sections_created_by",
                schema: "public",
                table: "sections",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_sections_deleted",
                schema: "public",
                table: "sections",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_sections_updated_at",
                schema: "public",
                table: "sections",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_sections_updated_by",
                schema: "public",
                table: "sections",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "sections_IX_module_id_name",
                schema: "public",
                table: "sections",
                columns: new[] { "module_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_settings_created_at",
                schema: "public",
                table: "settings",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_settings_created_by",
                schema: "public",
                table: "settings",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_settings_deleted",
                schema: "public",
                table: "settings",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_settings_key",
                schema: "public",
                table: "settings",
                column: "key");

            migrationBuilder.CreateIndex(
                name: "IX_settings_updated_at",
                schema: "public",
                table: "settings",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_settings_updated_by",
                schema: "public",
                table: "settings",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "settings_IX_user_id",
                schema: "public",
                table: "settings",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_template_permissions_created_at",
                schema: "public",
                table: "template_permissions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_template_permissions_created_by",
                schema: "public",
                table: "template_permissions",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_template_permissions_deleted",
                schema: "public",
                table: "template_permissions",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_template_permissions_profile_id",
                schema: "public",
                table: "template_permissions",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_template_permissions_updated_at",
                schema: "public",
                table: "template_permissions",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_template_permissions_updated_by",
                schema: "public",
                table: "template_permissions",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "template_permissions_IX_template_id_profile_id",
                schema: "public",
                table: "template_permissions",
                columns: new[] { "template_id", "profile_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_template_shares_template_id",
                schema: "public",
                table: "template_shares",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_templates_created_at",
                schema: "public",
                table: "templates",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_templates_created_by",
                schema: "public",
                table: "templates",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_templates_deleted",
                schema: "public",
                table: "templates",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_templates_sharing_type",
                schema: "public",
                table: "templates",
                column: "sharing_type");

            migrationBuilder.CreateIndex(
                name: "IX_templates_updated_at",
                schema: "public",
                table: "templates",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_templates_updated_by",
                schema: "public",
                table: "templates",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_user_custom_shares_created_by",
                schema: "public",
                table: "user_custom_shares",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_user_custom_shares_shared_user_id",
                schema: "public",
                table: "user_custom_shares",
                column: "shared_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_custom_shares_updated_by",
                schema: "public",
                table: "user_custom_shares",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_user_custom_shares_user_id",
                schema: "public",
                table: "user_custom_shares",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_groups_created_by",
                schema: "public",
                table: "user_groups",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_user_groups_updated_by",
                schema: "public",
                table: "user_groups",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_users_created_at",
                schema: "public",
                table: "users",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_users_deleted",
                schema: "public",
                table: "users",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "public",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_users_full_name",
                schema: "public",
                table: "users",
                column: "full_name");

            migrationBuilder.CreateIndex(
                name: "IX_users_profile_id",
                schema: "public",
                table: "users",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                schema: "public",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_updated_at",
                schema: "public",
                table: "users",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_users_user_groups_group_id",
                schema: "public",
                table: "users_user_groups",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_view_fields_created_by",
                schema: "public",
                table: "view_fields",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_view_fields_updated_by",
                schema: "public",
                table: "view_fields",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_view_fields_view_id",
                schema: "public",
                table: "view_fields",
                column: "view_id");

            migrationBuilder.CreateIndex(
                name: "IX_view_filters_created_by",
                schema: "public",
                table: "view_filters",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_view_filters_updated_by",
                schema: "public",
                table: "view_filters",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_view_filters_view_id",
                schema: "public",
                table: "view_filters",
                column: "view_id");

            migrationBuilder.CreateIndex(
                name: "IX_view_shares_view_id",
                schema: "public",
                table: "view_shares",
                column: "view_id");

            migrationBuilder.CreateIndex(
                name: "IX_view_states_created_at",
                schema: "public",
                table: "view_states",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_view_states_created_by",
                schema: "public",
                table: "view_states",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_view_states_deleted",
                schema: "public",
                table: "view_states",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_view_states_updated_at",
                schema: "public",
                table: "view_states",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_view_states_updated_by",
                schema: "public",
                table: "view_states",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_view_states_user_id",
                schema: "public",
                table: "view_states",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "view_states_IX_module_id_user_id",
                schema: "public",
                table: "view_states",
                columns: new[] { "module_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_views_created_at",
                schema: "public",
                table: "views",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_views_created_by",
                schema: "public",
                table: "views",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_views_deleted",
                schema: "public",
                table: "views",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_views_module_id",
                schema: "public",
                table: "views",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_views_sharing_type",
                schema: "public",
                table: "views",
                column: "sharing_type");

            migrationBuilder.CreateIndex(
                name: "IX_views_updated_at",
                schema: "public",
                table: "views",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_views_updated_by",
                schema: "public",
                table: "views",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_widgets_created_at",
                schema: "public",
                table: "widgets",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_widgets_created_by",
                schema: "public",
                table: "widgets",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_widgets_deleted",
                schema: "public",
                table: "widgets",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_widgets_report_id",
                schema: "public",
                table: "widgets",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_widgets_updated_at",
                schema: "public",
                table: "widgets",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_widgets_updated_by",
                schema: "public",
                table: "widgets",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_widgets_view_id",
                schema: "public",
                table: "widgets",
                column: "view_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_filters_created_at",
                schema: "public",
                table: "workflow_filters",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_filters_created_by",
                schema: "public",
                table: "workflow_filters",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_filters_deleted",
                schema: "public",
                table: "workflow_filters",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_filters_updated_at",
                schema: "public",
                table: "workflow_filters",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_filters_updated_by",
                schema: "public",
                table: "workflow_filters",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_filters_workflow_id",
                schema: "public",
                table: "workflow_filters",
                column: "workflow_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_created_at",
                schema: "public",
                table: "workflow_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_created_by",
                schema: "public",
                table: "workflow_logs",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_deleted",
                schema: "public",
                table: "workflow_logs",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_module_id",
                schema: "public",
                table: "workflow_logs",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_record_id",
                schema: "public",
                table: "workflow_logs",
                column: "record_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_updated_at",
                schema: "public",
                table: "workflow_logs",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_updated_by",
                schema: "public",
                table: "workflow_logs",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_workflow_id",
                schema: "public",
                table: "workflow_logs",
                column: "workflow_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_notifications_SendNotification",
                schema: "public",
                table: "workflow_notifications",
                column: "SendNotification",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_notifications_WorkflowId1",
                schema: "public",
                table: "workflow_notifications",
                column: "WorkflowId1");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_tasks_CreateTask",
                schema: "public",
                table: "workflow_tasks",
                column: "CreateTask",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_tasks_WorkflowId1",
                schema: "public",
                table: "workflow_tasks",
                column: "WorkflowId1");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_updates_FieldUpdate",
                schema: "public",
                table: "workflow_updates",
                column: "FieldUpdate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_updates_WorkflowId1",
                schema: "public",
                table: "workflow_updates",
                column: "WorkflowId1");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_webhooks_WebHook",
                schema: "public",
                table: "workflow_webhooks",
                column: "WebHook",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_webhooks_WorkflowId1",
                schema: "public",
                table: "workflow_webhooks",
                column: "WorkflowId1");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_active",
                schema: "public",
                table: "workflows",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_created_at",
                schema: "public",
                table: "workflows",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_created_by",
                schema: "public",
                table: "workflows",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_deleted",
                schema: "public",
                table: "workflows",
                column: "deleted");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_module_id",
                schema: "public",
                table: "workflows",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_updated_at",
                schema: "public",
                table: "workflows",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_updated_by",
                schema: "public",
                table: "workflows",
                column: "updated_by");

            migrationBuilder.AddForeignKey(
                name: "FK_action_button_permissions_action_buttons_action_button_id",
                schema: "public",
                table: "action_button_permissions",
                column: "action_button_id",
                principalSchema: "public",
                principalTable: "action_buttons",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_action_button_permissions_users_created_by",
                schema: "public",
                table: "action_button_permissions",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_action_button_permissions_users_updated_by",
                schema: "public",
                table: "action_button_permissions",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_action_button_permissions_profiles_profile_id",
                schema: "public",
                table: "action_button_permissions",
                column: "profile_id",
                principalSchema: "public",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_analytic_shares_users_user_id",
                schema: "public",
                table: "analytic_shares",
                column: "user_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_analytic_shares_analytics_analytic_id",
                schema: "public",
                table: "analytic_shares",
                column: "analytic_id",
                principalSchema: "public",
                principalTable: "analytics",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dashlets_users_created_by",
                schema: "public",
                table: "dashlets",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dashlets_users_updated_by",
                schema: "public",
                table: "dashlets",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dashlets_charts_chart_id",
                schema: "public",
                table: "dashlets",
                column: "chart_id",
                principalSchema: "public",
                principalTable: "charts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dashlets_dashboard_dashboard_id",
                schema: "public",
                table: "dashlets",
                column: "dashboard_id",
                principalSchema: "public",
                principalTable: "dashboard",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dashlets_widgets_widget_id",
                schema: "public",
                table: "dashlets",
                column: "widget_id",
                principalSchema: "public",
                principalTable: "widgets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_field_combinations_fields_Combination",
                schema: "public",
                table: "field_combinations",
                column: "Combination",
                principalSchema: "public",
                principalTable: "fields",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_field_combinations_fields_FieldId1",
                schema: "public",
                table: "field_combinations",
                column: "FieldId1",
                principalSchema: "public",
                principalTable: "fields",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_field_filters_users_created_by",
                schema: "public",
                table: "field_filters",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_field_filters_users_updated_by",
                schema: "public",
                table: "field_filters",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_field_filters_fields_field_id",
                schema: "public",
                table: "field_filters",
                column: "field_id",
                principalSchema: "public",
                principalTable: "fields",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_field_permissions_users_created_by",
                schema: "public",
                table: "field_permissions",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_field_permissions_users_updated_by",
                schema: "public",
                table: "field_permissions",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_field_permissions_profiles_profile_id",
                schema: "public",
                table: "field_permissions",
                column: "profile_id",
                principalSchema: "public",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_field_permissions_fields_field_id",
                schema: "public",
                table: "field_permissions",
                column: "field_id",
                principalSchema: "public",
                principalTable: "fields",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_field_validations_fields_FieldId1",
                schema: "public",
                table: "field_validations",
                column: "FieldId1",
                principalSchema: "public",
                principalTable: "fields",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_field_validations_fields_Validation",
                schema: "public",
                table: "field_validations",
                column: "Validation",
                principalSchema: "public",
                principalTable: "fields",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_action_buttons_users_created_by",
                schema: "public",
                table: "action_buttons",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_action_buttons_users_updated_by",
                schema: "public",
                table: "action_buttons",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_action_buttons_modules_module_id",
                schema: "public",
                table: "action_buttons",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_audit_logs_users_created_by",
                schema: "public",
                table: "audit_logs",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_audit_logs_users_updated_by",
                schema: "public",
                table: "audit_logs",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_audit_logs_modules_module_id",
                schema: "public",
                table: "audit_logs",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_calculations_users_created_by",
                schema: "public",
                table: "calculations",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_calculations_users_updated_by",
                schema: "public",
                table: "calculations",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_calculations_modules_module_id",
                schema: "public",
                table: "calculations",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_components_users_created_by",
                schema: "public",
                table: "components",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_components_users_updated_by",
                schema: "public",
                table: "components",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_components_modules_module_id",
                schema: "public",
                table: "components",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_conversion_mappings_users_created_by",
                schema: "public",
                table: "conversion_mappings",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_conversion_mappings_users_updated_by",
                schema: "public",
                table: "conversion_mappings",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_conversion_mappings_modules_mapping_module_id",
                schema: "public",
                table: "conversion_mappings",
                column: "mapping_module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_conversion_mappings_modules_module_id",
                schema: "public",
                table: "conversion_mappings",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_conversion_sub_modules_users_created_by",
                schema: "public",
                table: "conversion_sub_modules",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_conversion_sub_modules_users_updated_by",
                schema: "public",
                table: "conversion_sub_modules",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_conversion_sub_modules_modules_sub_module_id",
                schema: "public",
                table: "conversion_sub_modules",
                column: "sub_module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_conversion_sub_modules_modules_module_id",
                schema: "public",
                table: "conversion_sub_modules",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dependencies_users_created_by",
                schema: "public",
                table: "dependencies",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dependencies_users_updated_by",
                schema: "public",
                table: "dependencies",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dependencies_modules_module_id",
                schema: "public",
                table: "dependencies",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_documents_users_created_by",
                schema: "public",
                table: "documents",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_documents_users_updated_by",
                schema: "public",
                table: "documents",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_documents_modules_module_id",
                schema: "public",
                table: "documents",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_fields_users_created_by",
                schema: "public",
                table: "fields",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_fields_users_updated_by",
                schema: "public",
                table: "fields",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_fields_modules_module_id",
                schema: "public",
                table: "fields",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_fields_picklists_picklist_id",
                schema: "public",
                table: "fields",
                column: "picklist_id",
                principalSchema: "public",
                principalTable: "picklists",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_helps_users_created_by",
                schema: "public",
                table: "helps",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_helps_users_updated_by",
                schema: "public",
                table: "helps",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_helps_modules_module_id",
                schema: "public",
                table: "helps",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_imports_users_created_by",
                schema: "public",
                table: "imports",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_imports_users_updated_by",
                schema: "public",
                table: "imports",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_imports_modules_module_id",
                schema: "public",
                table: "imports",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_module_profile_settings_users_created_by",
                schema: "public",
                table: "module_profile_settings",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_module_profile_settings_users_updated_by",
                schema: "public",
                table: "module_profile_settings",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_module_profile_settings_modules_module_id",
                schema: "public",
                table: "module_profile_settings",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_notes_users_created_by",
                schema: "public",
                table: "notes",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_notes_users_updated_by",
                schema: "public",
                table: "notes",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_notes_modules_module_id",
                schema: "public",
                table: "notes",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_users_created_by",
                schema: "public",
                table: "notifications",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_users_updated_by",
                schema: "public",
                table: "notifications",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_modules_module_id",
                schema: "public",
                table: "notifications",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_processes_users_created_by",
                schema: "public",
                table: "processes",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_processes_users_updated_by",
                schema: "public",
                table: "processes",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_processes_modules_module_id",
                schema: "public",
                table: "processes",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_profile_permissions_profiles_profile_id",
                schema: "public",
                table: "profile_permissions",
                column: "profile_id",
                principalSchema: "public",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_profile_permissions_modules_module_id",
                schema: "public",
                table: "profile_permissions",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_relations_users_created_by",
                schema: "public",
                table: "relations",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_relations_users_updated_by",
                schema: "public",
                table: "relations",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_relations_modules_module_id",
                schema: "public",
                table: "relations",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reminders_users_created_by",
                schema: "public",
                table: "reminders",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reminders_users_updated_by",
                schema: "public",
                table: "reminders",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_reminders_modules_module_id",
                schema: "public",
                table: "reminders",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_reports_users_created_by",
                schema: "public",
                table: "reports",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reports_users_updated_by",
                schema: "public",
                table: "reports",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_reports_users_user_id",
                schema: "public",
                table: "reports",
                column: "user_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_reports_modules_module_id",
                schema: "public",
                table: "reports",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reports_report_categories_category_id",
                schema: "public",
                table: "reports",
                column: "category_id",
                principalSchema: "public",
                principalTable: "report_categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_sections_users_created_by",
                schema: "public",
                table: "sections",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sections_users_updated_by",
                schema: "public",
                table: "sections",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_sections_modules_module_id",
                schema: "public",
                table: "sections",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_view_states_users_created_by",
                schema: "public",
                table: "view_states",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_view_states_users_updated_by",
                schema: "public",
                table: "view_states",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_view_states_users_user_id",
                schema: "public",
                table: "view_states",
                column: "user_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_view_states_modules_module_id",
                schema: "public",
                table: "view_states",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_views_users_created_by",
                schema: "public",
                table: "views",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_views_users_updated_by",
                schema: "public",
                table: "views",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_views_modules_module_id",
                schema: "public",
                table: "views",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_workflows_users_created_by",
                schema: "public",
                table: "workflows",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_workflows_users_updated_by",
                schema: "public",
                table: "workflows",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_workflows_modules_module_id",
                schema: "public",
                table: "workflows",
                column: "module_id",
                principalSchema: "public",
                principalTable: "modules",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_note_likes_users_user_id",
                schema: "public",
                table: "note_likes",
                column: "user_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_picklist_items_users_created_by",
                schema: "public",
                table: "picklist_items",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_picklist_items_users_updated_by",
                schema: "public",
                table: "picklist_items",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_picklist_items_picklists_picklist_id",
                schema: "public",
                table: "picklist_items",
                column: "picklist_id",
                principalSchema: "public",
                principalTable: "picklists",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_process_approvers_users_created_by",
                schema: "public",
                table: "process_approvers",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_process_approvers_users_updated_by",
                schema: "public",
                table: "process_approvers",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_process_approvers_users_user_id",
                schema: "public",
                table: "process_approvers",
                column: "user_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_process_filters_users_created_by",
                schema: "public",
                table: "process_filters",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_process_filters_users_updated_by",
                schema: "public",
                table: "process_filters",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_process_logs_users_created_by",
                schema: "public",
                table: "process_logs",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_process_logs_users_updated_by",
                schema: "public",
                table: "process_logs",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_process_requests_users_created_by",
                schema: "public",
                table: "process_requests",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_process_requests_users_updated_by",
                schema: "public",
                table: "process_requests",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dashboard_users_created_by",
                schema: "public",
                table: "dashboard",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dashboard_users_updated_by",
                schema: "public",
                table: "dashboard",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dashboard_users_user_id",
                schema: "public",
                table: "dashboard",
                column: "user_id",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dashboard_profiles_profile_id",
                schema: "public",
                table: "dashboard",
                column: "profile_id",
                principalSchema: "public",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_section_permissions_users_created_by",
                schema: "public",
                table: "section_permissions",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_section_permissions_users_updated_by",
                schema: "public",
                table: "section_permissions",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_section_permissions_profiles_profile_id",
                schema: "public",
                table: "section_permissions",
                column: "profile_id",
                principalSchema: "public",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_template_permissions_users_created_by",
                schema: "public",
                table: "template_permissions",
                column: "created_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_template_permissions_users_updated_by",
                schema: "public",
                table: "template_permissions",
                column: "updated_by",
                principalSchema: "public",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_template_permissions_profiles_profile_id",
                schema: "public",
                table: "template_permissions",
                column: "profile_id",
                principalSchema: "public",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_template_permissions_templates_template_id",
                schema: "public",
                table: "template_permissions",
                column: "template_id",
                principalSchema: "public",
                principalTable: "templates",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_users_profiles_profile_id",
                schema: "public",
                table: "users",
                column: "profile_id",
                principalSchema: "public",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_users_roles_role_id",
                schema: "public",
                table: "users",
                column: "role_id",
                principalSchema: "public",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_profiles_users_created_by",
                schema: "public",
                table: "profiles");

            migrationBuilder.DropForeignKey(
                name: "FK_profiles_users_updated_by",
                schema: "public",
                table: "profiles");

            migrationBuilder.DropForeignKey(
                name: "FK_roles_users_created_by",
                schema: "public",
                table: "roles");

            migrationBuilder.DropForeignKey(
                name: "FK_roles_users_updated_by",
                schema: "public",
                table: "roles");

            migrationBuilder.DropTable(
                name: "action_button_permissions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "analytic_shares",
                schema: "public");

            migrationBuilder.DropTable(
                name: "audit_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "calculations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "changelogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "components",
                schema: "public");

            migrationBuilder.DropTable(
                name: "conversion_mappings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "conversion_sub_modules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "dashlets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "dependencies",
                schema: "public");

            migrationBuilder.DropTable(
                name: "documents",
                schema: "public");

            migrationBuilder.DropTable(
                name: "field_combinations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "field_filters",
                schema: "public");

            migrationBuilder.DropTable(
                name: "field_permissions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "field_validations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "helps",
                schema: "public");

            migrationBuilder.DropTable(
                name: "imports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "module_profile_settings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "note_likes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "public");

            migrationBuilder.DropTable(
                name: "picklist_items",
                schema: "public");

            migrationBuilder.DropTable(
                name: "process_approvers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "process_filters",
                schema: "public");

            migrationBuilder.DropTable(
                name: "process_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "process_requests",
                schema: "public");

            migrationBuilder.DropTable(
                name: "profile_permissions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "relations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "reminders",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_aggregations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_fields",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_filters",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_shares",
                schema: "public");

            migrationBuilder.DropTable(
                name: "section_permissions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "settings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "template_permissions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "template_shares",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_custom_shares",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users_user_groups",
                schema: "public");

            migrationBuilder.DropTable(
                name: "view_fields",
                schema: "public");

            migrationBuilder.DropTable(
                name: "view_filters",
                schema: "public");

            migrationBuilder.DropTable(
                name: "view_shares",
                schema: "public");

            migrationBuilder.DropTable(
                name: "view_states",
                schema: "public");

            migrationBuilder.DropTable(
                name: "workflow_filters",
                schema: "public");

            migrationBuilder.DropTable(
                name: "workflow_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "workflow_notifications",
                schema: "public");

            migrationBuilder.DropTable(
                name: "workflow_tasks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "workflow_updates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "workflow_webhooks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "action_buttons",
                schema: "public");

            migrationBuilder.DropTable(
                name: "analytics",
                schema: "public");

            migrationBuilder.DropTable(
                name: "charts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "dashboard",
                schema: "public");

            migrationBuilder.DropTable(
                name: "widgets",
                schema: "public");

            migrationBuilder.DropTable(
                name: "fields",
                schema: "public");

            migrationBuilder.DropTable(
                name: "notes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "processes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sections",
                schema: "public");

            migrationBuilder.DropTable(
                name: "templates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_groups",
                schema: "public");

            migrationBuilder.DropTable(
                name: "workflows",
                schema: "public");

            migrationBuilder.DropTable(
                name: "reports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "views",
                schema: "public");

            migrationBuilder.DropTable(
                name: "picklists",
                schema: "public");

            migrationBuilder.DropTable(
                name: "report_categories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "modules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "profiles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "public");
        }
    }
}
