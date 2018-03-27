using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum AuditType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "record")]
        Record = 1,

        [EnumMember(Value = "setup")]
        Setup = 2
    }
}
