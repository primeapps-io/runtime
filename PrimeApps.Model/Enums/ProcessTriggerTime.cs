using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum ProcessTriggerTime
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "instant")]
        Instant = 1,

        [EnumMember(Value = "manuel")]
        Manuel = 2
    }
}
