using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum ActionButtonPermissionType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "full")]
        Full = 1,

        [EnumMember(Value = "read_only")]
        ReadOnly = 2,

        [EnumMember(Value = "none")]
        None = 3
    }
}
