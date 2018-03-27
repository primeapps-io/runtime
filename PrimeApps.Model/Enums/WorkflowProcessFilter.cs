using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum WorkflowProcessFilter
    {
        [EnumMember(Value = "none")]
        None = 0,

        [EnumMember(Value = "all")]
        All = 1,

        [EnumMember(Value = "pending")]
        Pending = 2,

        [EnumMember(Value = "approved")]
        Approved = 3,

        [EnumMember(Value = "rejected")]
        Rejected = 4
    }
}
