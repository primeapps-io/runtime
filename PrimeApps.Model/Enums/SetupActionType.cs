using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum SetupActionType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "module_created")]
        ModuleCreated = 1,

        [EnumMember(Value = "module_updated")]
        ModuleUpdated = 2,

        [EnumMember(Value = "module_deleted")]
        ModuleDeleted = 3,

        [EnumMember(Value = "picklist_created")]
        PicklistCreated = 4,

        [EnumMember(Value = "picklist_updated")]
        PicklistUpdated = 5,

        [EnumMember(Value = "picklist_deleted")]
        PicklistDeleted = 6,

        [EnumMember(Value = "user_created")]
        UserCreated = 7,

        [EnumMember(Value = "user_updated")]
        UserUpdated = 8,

        [EnumMember(Value = "user_deleted")]
        UserDeleted = 9
    }
}
