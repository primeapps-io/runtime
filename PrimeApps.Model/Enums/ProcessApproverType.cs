using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum ProcessApproverType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "dynamicApprover")]
        DynamicApprover = 1,

        [EnumMember(Value = "staticApprover")]
        StaticApprover = 2
    }
}
