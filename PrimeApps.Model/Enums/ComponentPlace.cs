using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum ComponentPlace
    {
        [EnumMember(Value = "")] NotSet = 0,

        [EnumMember(Value = "field_change")] FieldChange = 1,

        [EnumMember(Value = "before_create")] BeforeCreate = 2,

        [EnumMember(Value = "after_create")] AfterCreate = 3,

        [EnumMember(Value = "before_update")] BeforeUpdate = 4,

        [EnumMember(Value = "after_update")] AfterUpdate = 5,

        [EnumMember(Value = "before_delete")] BeforeDelete = 6,

        [EnumMember(Value = "after_delete")] AfterDelete = 7,

        [EnumMember(Value = "after_record_loaded")]
        AfterRecordLoaded = 8,

        [EnumMember(Value = "before_lookup")] BeforeLookup = 9,

        [EnumMember(Value = "picklist_filter")]
        PicklistFilter = 10,

        [EnumMember(Value = "before_approve_process")]
        BeforeApproveProcess = 11,

        [EnumMember(Value = "before_reject_process")]
        BeforeRejectProcess = 12,

        [EnumMember(Value = "after_approve_process")]
        AfterApproveProcess = 13,

        [EnumMember(Value = "after_reject_process")]
        AfterRejectProcess = 14,

        [EnumMember(Value = "before_send_to_process_approval")]
        BeforeSendToProcessApproval = 15,

        [EnumMember(Value = "after_send_to_process_approval")]
        AfterSendToProcessApproval = 16,

        [EnumMember(Value = "before_list_request")]
        BeforeListRequest = 17,

        [EnumMember(Value = "ModuleFormLoaded")]
        ModuleFormCodeInject = 18,

        
        /*
         * Global config script
         */
        [EnumMember(Value = "global_config")] GlobalConfig = 100,

        /*
         * Component Place
         */
        [EnumMember(Value = "page")] Page = 1000,

        [EnumMember(Value = "section")] Section = 1001,

        [EnumMember(Value = "navbar")] Navbar = 1002,
    }
}