using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum Rounding
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "off")]
        Off = 1,

        [EnumMember(Value = "down")]
        Down = 2,

        [EnumMember(Value = "up")]
        Up = 3
    }
}
