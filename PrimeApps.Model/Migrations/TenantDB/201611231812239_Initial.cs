namespace PrimeApps.Model.Migrations.TenantDB
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.action_buttons",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    name = c.String(nullable: false, maxLength: 15),
                    template = c.String(nullable: false),
                    url = c.String(nullable: false),
                    icon = c.String(),
                    css_class = c.String(),
                    type = c.Int(nullable: false),
                    trigger = c.Int(nullable: false),
                    module_id = c.Int(nullable: false),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.module_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.users",
                c => new
                {
                    id = c.Int(nullable: false),
                    email = c.String(nullable: false, maxLength: 200),
                    first_name = c.String(nullable: false, maxLength: 200),
                    last_name = c.String(nullable: false, maxLength: 200),
                    full_name = c.String(nullable: false, maxLength: 400),
                    is_active = c.Boolean(nullable: false),
                    culture = c.String(maxLength: 10),
                    currency = c.String(maxLength: 3),
                    is_subscriber = c.Boolean(nullable: false),
                    created_by = c.String(),
                    updated_by = c.String(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                    picture = c.String(),
                    profile_id = c.Int(),
                    role_id = c.Int(),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.profiles", t => t.profile_id)
                .ForeignKey("public.roles", t => t.role_id)
                .Index(t => t.email)
                .Index(t => t.full_name)
                .Index(t => t.profile_id)
                .Index(t => t.role_id);

            CreateTable(
                "public.user_groups",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    name = c.String(),
                    description = c.String(),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.profiles",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    name = c.String(),
                    description = c.String(),
                    has_admin_rights = c.Boolean(nullable: false),
                    send_email = c.Boolean(nullable: false),
                    send_sms = c.Boolean(nullable: false),
                    business_intelligence = c.Boolean(nullable: false),
                    is_persistent = c.Boolean(nullable: false),
                    migration_id = c.String(),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.profile_permissions",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    type = c.Int(nullable: false),
                    read = c.Boolean(nullable: false),
                    write = c.Boolean(nullable: false),
                    modify = c.Boolean(nullable: false),
                    remove = c.Boolean(nullable: false),
                    profile_id = c.Int(nullable: false),
                    module_id = c.Int(),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.modules", t => t.module_id)
                .ForeignKey("public.profiles", t => t.profile_id, cascadeDelete: true)
                .Index(t => t.profile_id)
                .Index(t => t.module_id);

            CreateTable(
                "public.modules",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    name = c.String(nullable: false, maxLength: 50),
                    system_type = c.Int(nullable: false),
                    order = c.Short(nullable: false),
                    display = c.Boolean(nullable: false),
                    sharing = c.Int(nullable: false),
                    label_en_singular = c.String(nullable: false, maxLength: 50),
                    label_en_plural = c.String(nullable: false, maxLength: 50),
                    label_tr_singular = c.String(nullable: false, maxLength: 50),
                    label_tr_plural = c.String(nullable: false, maxLength: 50),
                    menu_icon = c.String(maxLength: 100),
                    location_enabled = c.Boolean(nullable: false),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.name, unique: true)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.audit_logs",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    audit_type = c.Int(nullable: false),
                    module_id = c.Int(),
                    record_id = c.Int(),
                    record_name = c.String(maxLength: 50),
                    record_action_type = c.Int(nullable: false),
                    setup_action_type = c.Int(nullable: false),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.module_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.calculations",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    module_id = c.Int(nullable: false),
                    result_field = c.String(nullable: false, maxLength: 50),
                    field1 = c.String(maxLength: 50),
                    field2 = c.String(maxLength: 50),
                    custom_value = c.Double(),
                    _operator = c.String(name: "operator", nullable: false, maxLength: 1),
                    order = c.Short(nullable: false),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.module_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.dependencies",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    module_id = c.Int(nullable: false),
                    dependency_type = c.Int(nullable: false),
                    parent_field = c.String(nullable: false, maxLength: 50),
                    child_field = c.String(maxLength: 50),
                    child_section = c.String(maxLength: 50),
                    values = c.String(maxLength: 4000),
                    field_map_parent = c.String(maxLength: 50),
                    field_map_child = c.String(maxLength: 50),
                    value_map = c.String(maxLength: 4000),
                    otherwise = c.Boolean(nullable: false),
                    clear = c.Boolean(nullable: false),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.module_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.fields",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    module_id = c.Int(nullable: false),
                    name = c.String(nullable: false, maxLength: 50),
                    system_type = c.Int(nullable: false),
                    data_type = c.Int(nullable: false),
                    order = c.Short(nullable: false),
                    section = c.String(maxLength: 50),
                    section_column = c.Short(nullable: false),
                    primary = c.Boolean(nullable: false),
                    default_value = c.String(maxLength: 500),
                    inline_edit = c.Boolean(nullable: false),
                    editable = c.Boolean(nullable: false),
                    show_label = c.Boolean(nullable: false),
                    multiline_type = c.Int(nullable: false),
                    picklist_id = c.Int(),
                    picklist_sortorder = c.Int(nullable: false),
                    lookup_type = c.String(maxLength: 50),
                    lookup_relation = c.String(maxLength: 50),
                    decimal_places = c.Short(nullable: false),
                    rounding = c.Int(nullable: false),
                    currency_symbol = c.String(maxLength: 10),
                    auto_number_prefix = c.String(maxLength: 10),
                    auto_number_suffix = c.String(maxLength: 10),
                    mask = c.String(maxLength: 100),
                    placeholder = c.String(maxLength: 50),
                    unique_combine = c.String(maxLength: 50),
                    label_en = c.String(nullable: false, maxLength: 50),
                    label_tr = c.String(nullable: false, maxLength: 50),
                    display_list = c.Boolean(nullable: false),
                    display_form = c.Boolean(nullable: false),
                    display_detail = c.Boolean(nullable: false),
                    show_only_edit = c.Boolean(nullable: false),
                    style_label = c.String(maxLength: 400),
                    style_input = c.String(maxLength: 400),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id, cascadeDelete: true)
                .ForeignKey("public.picklists", t => t.picklist_id)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => new { t.module_id, t.name }, unique: true, name: "fields_IX_module_id_name")
                .Index(t => t.picklist_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.field_combinations",
                c => new
                {
                    field_id = c.Int(nullable: false),
                    field1 = c.String(nullable: false, maxLength: 50),
                    field2 = c.String(nullable: false, maxLength: 50),
                })
                .PrimaryKey(t => t.field_id)
                .ForeignKey("public.fields", t => t.field_id, cascadeDelete: true)
                .Index(t => t.field_id);

            CreateTable(
                "public.picklists",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    system_type = c.Int(nullable: false),
                    label_en = c.String(nullable: false, maxLength: 50),
                    label_tr = c.String(nullable: false, maxLength: 50),
                    migration_id = c.String(),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.label_en, unique: true)
                .Index(t => t.label_tr, unique: true)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.picklist_items",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    picklist_id = c.Int(nullable: false),
                    label_en = c.String(nullable: false, maxLength: 100),
                    label_tr = c.String(nullable: false, maxLength: 100),
                    value = c.String(maxLength: 100),
                    value2 = c.String(maxLength: 100),
                    value3 = c.String(maxLength: 100),
                    system_code = c.String(maxLength: 50),
                    order = c.Short(nullable: false),
                    inactive = c.Boolean(nullable: false),
                    migration_id = c.String(maxLength: 50),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.picklists", t => t.picklist_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.picklist_id)
                .Index(t => t.system_code, unique: true)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.field_validations",
                c => new
                {
                    field_id = c.Int(nullable: false),
                    required = c.Boolean(),
                    _readonly = c.Boolean(name: "readonly"),
                    min_length = c.Short(),
                    max_length = c.Short(),
                    min = c.Double(),
                    max = c.Double(),
                    pattern = c.String(maxLength: 200),
                    unique = c.Boolean(),
                })
                .PrimaryKey(t => t.field_id)
                .ForeignKey("public.fields", t => t.field_id, cascadeDelete: true)
                .Index(t => t.field_id);

            CreateTable(
                "public.notes",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    text = c.String(nullable: false, maxLength: 500),
                    module_id = c.Int(),
                    record_id = c.Int(),
                    note_id = c.Int(),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id)
                .ForeignKey("public.notes", t => t.note_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.module_id)
                .Index(t => t.record_id)
                .Index(t => t.note_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.notifications",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    type = c.Int(nullable: false),
                    module_id = c.Int(nullable: false),
                    rev = c.String(),
                    ids = c.String(nullable: false),
                    query = c.String(),
                    status = c.Int(nullable: false),
                    template = c.String(nullable: false),
                    lang = c.String(nullable: false),
                    queue_date = c.DateTime(nullable: false),
                    phone_field = c.String(maxLength: 50),
                    email_field = c.String(maxLength: 50),
                    sender_alias = c.String(maxLength: 50),
                    sender_email = c.String(maxLength: 50),
                    attachment_container = c.String(maxLength: 50),
                    subject = c.String(maxLength: 128),
                    result = c.String(),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.type)
                .Index(t => t.module_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.relations",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    module_id = c.Int(nullable: false),
                    related_module = c.String(nullable: false),
                    relation_type = c.Int(nullable: false),
                    relation_field = c.String(maxLength: 50),
                    display_fields = c.String(nullable: false, maxLength: 1000),
                    _readonly = c.Boolean(name: "readonly", nullable: false),
                    order = c.Short(nullable: false),
                    label_en_singular = c.String(nullable: false, maxLength: 50),
                    label_en_plural = c.String(nullable: false, maxLength: 50),
                    label_tr_singular = c.String(nullable: false, maxLength: 50),
                    label_tr_plural = c.String(nullable: false, maxLength: 50),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.module_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.reminders",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    reminder_scope = c.String(nullable: false, maxLength: 70),
                    module_id = c.Int(),
                    record_id = c.Int(),
                    owner = c.Int(),
                    reminder_type = c.String(nullable: false, maxLength: 20),
                    reminder_start = c.DateTime(nullable: false),
                    reminder_end = c.DateTime(nullable: false),
                    subject = c.String(nullable: false, maxLength: 200),
                    reminder_frequency = c.Int(),
                    reminded_on = c.DateTime(),
                    rev = c.String(maxLength: 30),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.reminder_scope)
                .Index(t => t.module_id)
                .Index(t => t.reminder_type)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.sections",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    module_id = c.Int(nullable: false),
                    name = c.String(nullable: false, maxLength: 50),
                    system_type = c.Int(nullable: false),
                    order = c.Short(nullable: false),
                    column_count = c.Short(nullable: false),
                    label_en = c.String(nullable: false, maxLength: 50),
                    label_tr = c.String(nullable: false, maxLength: 50),
                    display_form = c.Boolean(nullable: false),
                    display_detail = c.Boolean(nullable: false),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => new { t.module_id, t.name }, unique: true, name: "sections_IX_module_id_name")
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.roles",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    label_en = c.String(nullable: false, maxLength: 200),
                    label_tr = c.String(nullable: false, maxLength: 200),
                    description_en = c.String(maxLength: 500),
                    description_tr = c.String(maxLength: 500),
                    master = c.Boolean(nullable: false),
                    owners = c.String(),
                    migration_id = c.String(),
                    reports_to_id = c.Int(),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.roles", t => t.reports_to_id)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.reports_to_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.views",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    module_id = c.Int(nullable: false),
                    system_type = c.Int(nullable: false),
                    label_en = c.String(nullable: false, maxLength: 50),
                    label_tr = c.String(nullable: false, maxLength: 50),
                    active = c.Boolean(nullable: false),
                    sharing_type = c.Int(nullable: false),
                    filter_logic = c.String(maxLength: 50),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.module_id)
                .Index(t => t.sharing_type)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.view_fields",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    view_id = c.Int(nullable: false),
                    field = c.String(nullable: false, maxLength: 120),
                    order = c.Int(nullable: false),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .ForeignKey("public.views", t => t.view_id, cascadeDelete: true)
                .Index(t => t.view_id)
                .Index(t => t.updated_by)
                .Index(t => t.created_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.view_filters",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    view_id = c.Int(nullable: false),
                    field = c.String(nullable: false, maxLength: 120),
                    Operator = c.Int(nullable: false),
                    Value = c.String(nullable: false, maxLength: 100),
                    No = c.Int(nullable: false),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .ForeignKey("public.views", t => t.view_id, cascadeDelete: true)
                .Index(t => t.view_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.conversion_mappings",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    module_id = c.Int(nullable: false),
                    mapping_module_id = c.Int(nullable: false),
                    field_id = c.Int(nullable: false),
                    mapping_field_id = c.Int(nullable: false),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.mapping_module_id, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.module_id)
                .Index(t => t.mapping_module_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.documents",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    name = c.String(),
                    record_id = c.Int(nullable: false),
                    module_id = c.Int(nullable: false),
                    unique_name = c.String(),
                    description = c.String(),
                    type = c.String(),
                    file_size = c.Long(nullable: false),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.module_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.settings",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    type = c.Int(nullable: false),
                    user_id = c.Int(),
                    key = c.String(nullable: false),
                    value = c.String(nullable: false),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .ForeignKey("public.users", t => t.user_id)
                .Index(t => t.type)
                .Index(t => t.user_id, name: "settings_IX_user_id")
                .Index(t => t.key)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.templates",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    template_type = c.Int(nullable: false),
                    name = c.String(maxLength: 200),
                    content = c.String(),
                    language = c.Int(nullable: false),
                    active = c.Boolean(nullable: false),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.view_states",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    module_id = c.Int(nullable: false),
                    user_id = c.Int(nullable: false),
                    active_view = c.Int(nullable: false),
                    sort_field = c.String(maxLength: 120),
                    sort_direction = c.Int(nullable: false),
                    row_per_page = c.Int(),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .ForeignKey("public.users", t => t.user_id, cascadeDelete: true)
                .Index(t => new { t.module_id, t.user_id }, unique: true, name: "view_states_IX_module_id_user_id")
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.workflow_filters",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    workflow_id = c.Int(nullable: false),
                    field = c.String(nullable: false, maxLength: 120),
                    Operator = c.Int(nullable: false),
                    Value = c.String(nullable: false, maxLength: 100),
                    No = c.Int(nullable: false),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .ForeignKey("public.workflows", t => t.workflow_id, cascadeDelete: true)
                .Index(t => t.workflow_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.workflows",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    module_id = c.Int(nullable: false),
                    name = c.String(nullable: false, maxLength: 200),
                    frequency = c.Int(nullable: false),
                    active = c.Boolean(nullable: false),
                    operations = c.String(nullable: false, maxLength: 50),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.modules", t => t.module_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .Index(t => t.module_id)
                .Index(t => t.active)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.workflow_tasks",
                c => new
                {
                    workflow_id = c.Int(nullable: false),
                    owner = c.Int(nullable: false),
                    subject = c.String(nullable: false, maxLength: 2000),
                    task_due_date = c.Int(nullable: false),
                    task_status = c.Int(),
                    task_priority = c.Int(),
                    task_notification = c.Int(),
                    task_reminder = c.DateTime(),
                    reminder_recurrence = c.Int(),
                    description = c.String(maxLength: 2000),
                })
                .PrimaryKey(t => t.workflow_id)
                .ForeignKey("public.workflows", t => t.workflow_id, cascadeDelete: true)
                .Index(t => t.workflow_id);

            CreateTable(
                "public.workflow_updates",
                c => new
                {
                    workflow_id = c.Int(nullable: false),
                    module = c.String(nullable: false, maxLength: 120),
                    field = c.String(nullable: false, maxLength: 120),
                    value = c.String(nullable: false, maxLength: 2000),
                })
                .PrimaryKey(t => t.workflow_id)
                .ForeignKey("public.workflows", t => t.workflow_id, cascadeDelete: true)
                .Index(t => t.workflow_id);

            CreateTable(
                "public.workflow_logs",
                c => new
                {
                    id = c.Int(nullable: false, identity: true),
                    workflow_id = c.Int(nullable: false),
                    module_id = c.Int(nullable: false),
                    record_id = c.Int(nullable: false),
                    created_by = c.Int(nullable: false),
                    updated_by = c.Int(),
                    created_at = c.DateTime(nullable: false),
                    updated_at = c.DateTime(),
                    deleted = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.users", t => t.created_by, cascadeDelete: true)
                .ForeignKey("public.users", t => t.updated_by)
                .ForeignKey("public.workflows", t => t.workflow_id, cascadeDelete: true)
                .Index(t => t.workflow_id)
                .Index(t => t.module_id)
                .Index(t => t.record_id)
                .Index(t => t.created_by)
                .Index(t => t.updated_by)
                .Index(t => t.created_at)
                .Index(t => t.updated_at)
                .Index(t => t.deleted);

            CreateTable(
                "public.workflow_notifications",
                c => new
                {
                    workflow_id = c.Int(nullable: false),
                    subject = c.String(nullable: false, maxLength: 200),
                    message = c.String(nullable: false, maxLength: 500),
                    recipients = c.String(nullable: false, maxLength: 4000),
                    schedule = c.Int(),
                })
                .PrimaryKey(t => t.workflow_id)
                .ForeignKey("public.workflows", t => t.workflow_id, cascadeDelete: true)
                .Index(t => t.workflow_id);

            CreateTable(
                "public.users_user_groups",
                c => new
                {
                    user_id = c.Int(nullable: false),
                    group_id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.user_id, t.group_id })
                .ForeignKey("public.users", t => t.user_id, cascadeDelete: true)
                .ForeignKey("public.user_groups", t => t.group_id, cascadeDelete: true)
                .Index(t => t.user_id)
                .Index(t => t.group_id);

            CreateTable(
                "public.view_shares",
                c => new
                {
                    view_id = c.Int(nullable: false),
                    user_id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.view_id, t.user_id })
                .ForeignKey("public.views", t => t.view_id, cascadeDelete: true)
                .ForeignKey("public.users", t => t.user_id, cascadeDelete: true)
                .Index(t => t.view_id)
                .Index(t => t.user_id);

        }

        public override void Down()
        {
            DropForeignKey("public.workflow_filters", "workflow_id", "public.workflows");
            DropForeignKey("public.workflows", "updated_by", "public.users");
            DropForeignKey("public.workflow_notifications", "workflow_id", "public.workflows");
            DropForeignKey("public.workflows", "module_id", "public.modules");
            DropForeignKey("public.workflow_logs", "workflow_id", "public.workflows");
            DropForeignKey("public.workflow_logs", "updated_by", "public.users");
            DropForeignKey("public.workflow_logs", "created_by", "public.users");
            DropForeignKey("public.workflow_updates", "workflow_id", "public.workflows");
            DropForeignKey("public.workflow_tasks", "workflow_id", "public.workflows");
            DropForeignKey("public.workflows", "created_by", "public.users");
            DropForeignKey("public.workflow_filters", "updated_by", "public.users");
            DropForeignKey("public.workflow_filters", "created_by", "public.users");
            DropForeignKey("public.view_states", "user_id", "public.users");
            DropForeignKey("public.view_states", "updated_by", "public.users");
            DropForeignKey("public.view_states", "module_id", "public.modules");
            DropForeignKey("public.view_states", "created_by", "public.users");
            DropForeignKey("public.templates", "updated_by", "public.users");
            DropForeignKey("public.templates", "created_by", "public.users");
            DropForeignKey("public.settings", "user_id", "public.users");
            DropForeignKey("public.settings", "updated_by", "public.users");
            DropForeignKey("public.settings", "created_by", "public.users");
            DropForeignKey("public.documents", "updated_by", "public.users");
            DropForeignKey("public.documents", "module_id", "public.modules");
            DropForeignKey("public.documents", "created_by", "public.users");
            DropForeignKey("public.conversion_mappings", "updated_by", "public.users");
            DropForeignKey("public.conversion_mappings", "module_id", "public.modules");
            DropForeignKey("public.conversion_mappings", "mapping_module_id", "public.modules");
            DropForeignKey("public.conversion_mappings", "created_by", "public.users");
            DropForeignKey("public.action_buttons", "updated_by", "public.users");
            DropForeignKey("public.action_buttons", "module_id", "public.modules");
            DropForeignKey("public.action_buttons", "created_by", "public.users");
            DropForeignKey("public.views", "updated_by", "public.users");
            DropForeignKey("public.view_shares", "user_id", "public.users");
            DropForeignKey("public.view_shares", "view_id", "public.views");
            DropForeignKey("public.views", "module_id", "public.modules");
            DropForeignKey("public.view_filters", "view_id", "public.views");
            DropForeignKey("public.view_filters", "updated_by", "public.users");
            DropForeignKey("public.view_filters", "created_by", "public.users");
            DropForeignKey("public.view_fields", "view_id", "public.views");
            DropForeignKey("public.view_fields", "updated_by", "public.users");
            DropForeignKey("public.view_fields", "created_by", "public.users");
            DropForeignKey("public.views", "created_by", "public.users");
            DropForeignKey("public.users", "role_id", "public.roles");
            DropForeignKey("public.roles", "updated_by", "public.users");
            DropForeignKey("public.roles", "reports_to_id", "public.roles");
            DropForeignKey("public.roles", "created_by", "public.users");
            DropForeignKey("public.users", "profile_id", "public.profiles");
            DropForeignKey("public.profiles", "updated_by", "public.users");
            DropForeignKey("public.profile_permissions", "profile_id", "public.profiles");
            DropForeignKey("public.profile_permissions", "module_id", "public.modules");
            DropForeignKey("public.modules", "updated_by", "public.users");
            DropForeignKey("public.sections", "updated_by", "public.users");
            DropForeignKey("public.sections", "module_id", "public.modules");
            DropForeignKey("public.sections", "created_by", "public.users");
            DropForeignKey("public.reminders", "updated_by", "public.users");
            DropForeignKey("public.reminders", "module_id", "public.modules");
            DropForeignKey("public.reminders", "created_by", "public.users");
            DropForeignKey("public.relations", "updated_by", "public.users");
            DropForeignKey("public.relations", "module_id", "public.modules");
            DropForeignKey("public.relations", "created_by", "public.users");
            DropForeignKey("public.notifications", "updated_by", "public.users");
            DropForeignKey("public.notifications", "module_id", "public.modules");
            DropForeignKey("public.notifications", "created_by", "public.users");
            DropForeignKey("public.notes", "updated_by", "public.users");
            DropForeignKey("public.notes", "note_id", "public.notes");
            DropForeignKey("public.notes", "module_id", "public.modules");
            DropForeignKey("public.notes", "created_by", "public.users");
            DropForeignKey("public.field_validations", "field_id", "public.fields");
            DropForeignKey("public.fields", "updated_by", "public.users");
            DropForeignKey("public.fields", "picklist_id", "public.picklists");
            DropForeignKey("public.picklists", "updated_by", "public.users");
            DropForeignKey("public.picklist_items", "updated_by", "public.users");
            DropForeignKey("public.picklist_items", "picklist_id", "public.picklists");
            DropForeignKey("public.picklist_items", "created_by", "public.users");
            DropForeignKey("public.picklists", "created_by", "public.users");
            DropForeignKey("public.fields", "module_id", "public.modules");
            DropForeignKey("public.fields", "created_by", "public.users");
            DropForeignKey("public.field_combinations", "field_id", "public.fields");
            DropForeignKey("public.dependencies", "updated_by", "public.users");
            DropForeignKey("public.dependencies", "module_id", "public.modules");
            DropForeignKey("public.dependencies", "created_by", "public.users");
            DropForeignKey("public.modules", "created_by", "public.users");
            DropForeignKey("public.calculations", "updated_by", "public.users");
            DropForeignKey("public.calculations", "module_id", "public.modules");
            DropForeignKey("public.calculations", "created_by", "public.users");
            DropForeignKey("public.audit_logs", "updated_by", "public.users");
            DropForeignKey("public.audit_logs", "module_id", "public.modules");
            DropForeignKey("public.audit_logs", "created_by", "public.users");
            DropForeignKey("public.profiles", "created_by", "public.users");
            DropForeignKey("public.users_user_groups", "group_id", "public.user_groups");
            DropForeignKey("public.users_user_groups", "user_id", "public.users");
            DropForeignKey("public.user_groups", "updated_by", "public.users");
            DropForeignKey("public.user_groups", "created_by", "public.users");
            DropIndex("public.view_shares", new[] { "user_id" });
            DropIndex("public.view_shares", new[] { "view_id" });
            DropIndex("public.users_user_groups", new[] { "group_id" });
            DropIndex("public.users_user_groups", new[] { "user_id" });
            DropIndex("public.workflow_notifications", new[] { "workflow_id" });
            DropIndex("public.workflow_logs", new[] { "deleted" });
            DropIndex("public.workflow_logs", new[] { "updated_at" });
            DropIndex("public.workflow_logs", new[] { "created_at" });
            DropIndex("public.workflow_logs", new[] { "updated_by" });
            DropIndex("public.workflow_logs", new[] { "created_by" });
            DropIndex("public.workflow_logs", new[] { "record_id" });
            DropIndex("public.workflow_logs", new[] { "module_id" });
            DropIndex("public.workflow_logs", new[] { "workflow_id" });
            DropIndex("public.workflow_updates", new[] { "workflow_id" });
            DropIndex("public.workflow_tasks", new[] { "workflow_id" });
            DropIndex("public.workflows", new[] { "deleted" });
            DropIndex("public.workflows", new[] { "updated_at" });
            DropIndex("public.workflows", new[] { "created_at" });
            DropIndex("public.workflows", new[] { "updated_by" });
            DropIndex("public.workflows", new[] { "created_by" });
            DropIndex("public.workflows", new[] { "active" });
            DropIndex("public.workflows", new[] { "module_id" });
            DropIndex("public.workflow_filters", new[] { "deleted" });
            DropIndex("public.workflow_filters", new[] { "updated_at" });
            DropIndex("public.workflow_filters", new[] { "created_at" });
            DropIndex("public.workflow_filters", new[] { "updated_by" });
            DropIndex("public.workflow_filters", new[] { "created_by" });
            DropIndex("public.workflow_filters", new[] { "workflow_id" });
            DropIndex("public.view_states", new[] { "deleted" });
            DropIndex("public.view_states", new[] { "updated_at" });
            DropIndex("public.view_states", new[] { "created_at" });
            DropIndex("public.view_states", new[] { "updated_by" });
            DropIndex("public.view_states", new[] { "created_by" });
            DropIndex("public.view_states", "view_states_IX_module_id_user_id");
            DropIndex("public.templates", new[] { "deleted" });
            DropIndex("public.templates", new[] { "updated_at" });
            DropIndex("public.templates", new[] { "created_at" });
            DropIndex("public.templates", new[] { "updated_by" });
            DropIndex("public.templates", new[] { "created_by" });
            DropIndex("public.settings", new[] { "deleted" });
            DropIndex("public.settings", new[] { "updated_at" });
            DropIndex("public.settings", new[] { "created_at" });
            DropIndex("public.settings", new[] { "updated_by" });
            DropIndex("public.settings", new[] { "created_by" });
            DropIndex("public.settings", new[] { "key" });
            DropIndex("public.settings", "settings_IX_user_id");
            DropIndex("public.settings", new[] { "type" });
            DropIndex("public.documents", new[] { "deleted" });
            DropIndex("public.documents", new[] { "updated_at" });
            DropIndex("public.documents", new[] { "created_at" });
            DropIndex("public.documents", new[] { "updated_by" });
            DropIndex("public.documents", new[] { "created_by" });
            DropIndex("public.documents", new[] { "module_id" });
            DropIndex("public.conversion_mappings", new[] { "deleted" });
            DropIndex("public.conversion_mappings", new[] { "updated_at" });
            DropIndex("public.conversion_mappings", new[] { "created_at" });
            DropIndex("public.conversion_mappings", new[] { "updated_by" });
            DropIndex("public.conversion_mappings", new[] { "created_by" });
            DropIndex("public.conversion_mappings", new[] { "mapping_module_id" });
            DropIndex("public.conversion_mappings", new[] { "module_id" });
            DropIndex("public.view_filters", new[] { "deleted" });
            DropIndex("public.view_filters", new[] { "updated_at" });
            DropIndex("public.view_filters", new[] { "created_at" });
            DropIndex("public.view_filters", new[] { "updated_by" });
            DropIndex("public.view_filters", new[] { "created_by" });
            DropIndex("public.view_filters", new[] { "view_id" });
            DropIndex("public.view_fields", new[] { "deleted" });
            DropIndex("public.view_fields", new[] { "updated_at" });
            DropIndex("public.view_fields", new[] { "created_at" });
            DropIndex("public.view_fields", new[] { "updated_by" });
            DropIndex("public.view_fields", new[] { "created_by" });
            DropIndex("public.view_fields", new[] { "view_id" });
            DropIndex("public.views", new[] { "deleted" });
            DropIndex("public.views", new[] { "updated_at" });
            DropIndex("public.views", new[] { "created_at" });
            DropIndex("public.views", new[] { "updated_by" });
            DropIndex("public.views", new[] { "created_by" });
            DropIndex("public.views", new[] { "sharing_type" });
            DropIndex("public.views", new[] { "module_id" });
            DropIndex("public.roles", new[] { "deleted" });
            DropIndex("public.roles", new[] { "updated_at" });
            DropIndex("public.roles", new[] { "created_at" });
            DropIndex("public.roles", new[] { "updated_by" });
            DropIndex("public.roles", new[] { "created_by" });
            DropIndex("public.roles", new[] { "reports_to_id" });
            DropIndex("public.sections", new[] { "deleted" });
            DropIndex("public.sections", new[] { "updated_at" });
            DropIndex("public.sections", new[] { "created_at" });
            DropIndex("public.sections", new[] { "updated_by" });
            DropIndex("public.sections", new[] { "created_by" });
            DropIndex("public.sections", "sections_IX_module_id_name");
            DropIndex("public.reminders", new[] { "deleted" });
            DropIndex("public.reminders", new[] { "updated_at" });
            DropIndex("public.reminders", new[] { "created_at" });
            DropIndex("public.reminders", new[] { "updated_by" });
            DropIndex("public.reminders", new[] { "created_by" });
            DropIndex("public.reminders", new[] { "reminder_type" });
            DropIndex("public.reminders", new[] { "module_id" });
            DropIndex("public.reminders", new[] { "reminder_scope" });
            DropIndex("public.relations", new[] { "deleted" });
            DropIndex("public.relations", new[] { "updated_at" });
            DropIndex("public.relations", new[] { "created_at" });
            DropIndex("public.relations", new[] { "updated_by" });
            DropIndex("public.relations", new[] { "created_by" });
            DropIndex("public.relations", new[] { "module_id" });
            DropIndex("public.notifications", new[] { "deleted" });
            DropIndex("public.notifications", new[] { "updated_at" });
            DropIndex("public.notifications", new[] { "created_at" });
            DropIndex("public.notifications", new[] { "updated_by" });
            DropIndex("public.notifications", new[] { "created_by" });
            DropIndex("public.notifications", new[] { "module_id" });
            DropIndex("public.notifications", new[] { "type" });
            DropIndex("public.notes", new[] { "deleted" });
            DropIndex("public.notes", new[] { "updated_at" });
            DropIndex("public.notes", new[] { "created_at" });
            DropIndex("public.notes", new[] { "updated_by" });
            DropIndex("public.notes", new[] { "created_by" });
            DropIndex("public.notes", new[] { "note_id" });
            DropIndex("public.notes", new[] { "record_id" });
            DropIndex("public.notes", new[] { "module_id" });
            DropIndex("public.field_validations", new[] { "field_id" });
            DropIndex("public.picklist_items", new[] { "deleted" });
            DropIndex("public.picklist_items", new[] { "updated_at" });
            DropIndex("public.picklist_items", new[] { "created_at" });
            DropIndex("public.picklist_items", new[] { "updated_by" });
            DropIndex("public.picklist_items", new[] { "created_by" });
            DropIndex("public.picklist_items", new[] { "system_code" });
            DropIndex("public.picklist_items", new[] { "picklist_id" });
            DropIndex("public.picklists", new[] { "deleted" });
            DropIndex("public.picklists", new[] { "updated_at" });
            DropIndex("public.picklists", new[] { "created_at" });
            DropIndex("public.picklists", new[] { "updated_by" });
            DropIndex("public.picklists", new[] { "created_by" });
            DropIndex("public.picklists", new[] { "label_tr" });
            DropIndex("public.picklists", new[] { "label_en" });
            DropIndex("public.field_combinations", new[] { "field_id" });
            DropIndex("public.fields", new[] { "deleted" });
            DropIndex("public.fields", new[] { "updated_at" });
            DropIndex("public.fields", new[] { "created_at" });
            DropIndex("public.fields", new[] { "updated_by" });
            DropIndex("public.fields", new[] { "created_by" });
            DropIndex("public.fields", new[] { "picklist_id" });
            DropIndex("public.fields", "fields_IX_module_id_name");
            DropIndex("public.dependencies", new[] { "deleted" });
            DropIndex("public.dependencies", new[] { "updated_at" });
            DropIndex("public.dependencies", new[] { "created_at" });
            DropIndex("public.dependencies", new[] { "updated_by" });
            DropIndex("public.dependencies", new[] { "created_by" });
            DropIndex("public.dependencies", new[] { "module_id" });
            DropIndex("public.calculations", new[] { "deleted" });
            DropIndex("public.calculations", new[] { "updated_at" });
            DropIndex("public.calculations", new[] { "created_at" });
            DropIndex("public.calculations", new[] { "updated_by" });
            DropIndex("public.calculations", new[] { "created_by" });
            DropIndex("public.calculations", new[] { "module_id" });
            DropIndex("public.audit_logs", new[] { "deleted" });
            DropIndex("public.audit_logs", new[] { "updated_at" });
            DropIndex("public.audit_logs", new[] { "created_at" });
            DropIndex("public.audit_logs", new[] { "updated_by" });
            DropIndex("public.audit_logs", new[] { "created_by" });
            DropIndex("public.audit_logs", new[] { "module_id" });
            DropIndex("public.modules", new[] { "deleted" });
            DropIndex("public.modules", new[] { "updated_at" });
            DropIndex("public.modules", new[] { "created_at" });
            DropIndex("public.modules", new[] { "updated_by" });
            DropIndex("public.modules", new[] { "created_by" });
            DropIndex("public.modules", new[] { "name" });
            DropIndex("public.profile_permissions", new[] { "module_id" });
            DropIndex("public.profile_permissions", new[] { "profile_id" });
            DropIndex("public.profiles", new[] { "deleted" });
            DropIndex("public.profiles", new[] { "updated_at" });
            DropIndex("public.profiles", new[] { "created_at" });
            DropIndex("public.profiles", new[] { "updated_by" });
            DropIndex("public.profiles", new[] { "created_by" });
            DropIndex("public.user_groups", new[] { "deleted" });
            DropIndex("public.user_groups", new[] { "updated_at" });
            DropIndex("public.user_groups", new[] { "created_at" });
            DropIndex("public.user_groups", new[] { "updated_by" });
            DropIndex("public.user_groups", new[] { "created_by" });
            DropIndex("public.users", new[] { "role_id" });
            DropIndex("public.users", new[] { "profile_id" });
            DropIndex("public.users", new[] { "full_name" });
            DropIndex("public.users", new[] { "email" });
            DropIndex("public.action_buttons", new[] { "deleted" });
            DropIndex("public.action_buttons", new[] { "updated_at" });
            DropIndex("public.action_buttons", new[] { "created_at" });
            DropIndex("public.action_buttons", new[] { "updated_by" });
            DropIndex("public.action_buttons", new[] { "created_by" });
            DropIndex("public.action_buttons", new[] { "module_id" });
            DropTable("public.view_shares");
            DropTable("public.users_user_groups");
            DropTable("public.workflow_notifications");
            DropTable("public.workflow_logs");
            DropTable("public.workflow_updates");
            DropTable("public.workflow_tasks");
            DropTable("public.workflows");
            DropTable("public.workflow_filters");
            DropTable("public.view_states");
            DropTable("public.templates");
            DropTable("public.settings");
            DropTable("public.documents");
            DropTable("public.conversion_mappings");
            DropTable("public.view_filters");
            DropTable("public.view_fields");
            DropTable("public.views");
            DropTable("public.roles");
            DropTable("public.sections");
            DropTable("public.reminders");
            DropTable("public.relations");
            DropTable("public.notifications");
            DropTable("public.notes");
            DropTable("public.field_validations");
            DropTable("public.picklist_items");
            DropTable("public.picklists");
            DropTable("public.field_combinations");
            DropTable("public.fields");
            DropTable("public.dependencies");
            DropTable("public.calculations");
            DropTable("public.audit_logs");
            DropTable("public.modules");
            DropTable("public.profile_permissions");
            DropTable("public.profiles");
            DropTable("public.user_groups");
            DropTable("public.users");
            DropTable("public.action_buttons");
        }
    }
}
