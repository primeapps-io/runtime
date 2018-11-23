using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum BpmTriggerType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "record")]
        Record = 1,

        [EnumMember(Value = "timer")]
        Timer = 2
    }
}
