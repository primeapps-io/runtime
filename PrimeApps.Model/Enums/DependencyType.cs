using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum DependencyType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "display")]
        Display = 1,

        [EnumMember(Value = "list_text")]
        ListText = 2,

        [EnumMember(Value = "list_value")]
        ListValue = 3,

        [EnumMember(Value = "list_field")]
        ListField = 4,

        [EnumMember(Value = "lookup_text")]
        LookupText = 5,

        [EnumMember(Value = "lookup_list")]
        LookupList = 6,

        [EnumMember(Value = "freeze")]
        Freeze = 7,

        [EnumMember(Value = "lookup_field")]
        LookupField = 8
    }
}
