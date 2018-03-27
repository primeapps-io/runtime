using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum WorkflowFrequency
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "one_time")]
        OneTime = 1,

        [EnumMember(Value = "continuous")]
        Continuous = 2
    }
}
