using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum SystemType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "system")]
        System = 1,

        [EnumMember(Value = "custom")]
        Custom = 2,

        [EnumMember(Value = "component")]
        Component = 3
    }
}
