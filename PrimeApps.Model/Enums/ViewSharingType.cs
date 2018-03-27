using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum ViewSharingType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "everybody")]
        Everybody = 1,

        [EnumMember(Value = "me")]
        Me = 2,

        [EnumMember(Value = "custom")]
        Custom = 3
    }
}
