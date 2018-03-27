using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum DashboardSharingType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "everybody")]
        Everybody = 1,

        [EnumMember(Value = "me")]
        Me = 2,

        [EnumMember(Value = "profile")]
        Profile = 3
    }
}
