using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum RecordActionType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "inserted")]
        Inserted = 1,

        [EnumMember(Value = "updated")]
        Updated = 2,

        [EnumMember(Value = "deleted")]
        Deleted = 3

    }
}
