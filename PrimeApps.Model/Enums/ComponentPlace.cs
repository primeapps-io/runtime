using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum ComponentPlace
    {
        /*
         * Script Places
         */
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

        [EnumMember(Value = "before_form_loaded")]
        BeforeFormLoaded = 18,

        [EnumMember(Value = "after_form_loaded")]
        AfterFormLoaded = 19,

        [EnumMember(Value = "before_form_picklist_loaded")]
        BeforeFormPicklistLoaded = 20,

        [EnumMember(Value = "after_form_picklist_loaded")]
        AfterFormPicklistLoaded = 21,

        [EnumMember(Value = "before_form_record_loaded")]
        BeforeFormRecordLoaded = 22,

        [EnumMember(Value = "after_form_record_loaded")]
        AfterFormRecordLoaded = 23,

        [EnumMember(Value = "before_detail_loaded")]
        BeforeDetailLoaded = 24,

        [EnumMember(Value = "after_detail_loaded")]
        AfterDetailLoaded = 25,

        [EnumMember(Value = "before_form_submit")]
        BeforeFormSubmit = 26,

        [EnumMember(Value = "before_form_submit_result")]
        BeforeFormSubmitResult = 27,

        [EnumMember(Value = "sub_list_loaded")]
        SubListLoaded = 28,

        [EnumMember(Value = "before_import")]
        BeforeImport = 29,

        [EnumMember(Value = "empty_list")]
        EmptyList = 30,

        [EnumMember(Value = "after_bulk_email")]
        AfterBulkEmail = 31,

        [EnumMember(Value = "before_bulk_email")]
        BeforeBulkEmail = 32,

        /*
         * Global Config
         */
        [EnumMember(Value = "global_config")]
        GlobalConfig = 100,

        /*
         * Component Places
         */
        [EnumMember(Value = "page")]
        Page = 1000,

        [EnumMember(Value = "section")]
        Section = 1001,

        [EnumMember(Value = "navbar")]
        Navbar = 1002,
    }
}