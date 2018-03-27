using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum MultilineType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "small")]
        Small = 1,

        [EnumMember(Value = "large")]
        Large = 2
    }
}
