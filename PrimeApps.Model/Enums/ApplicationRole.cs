using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum ApplicationRole
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "manager")]
        Manager = 1,

        [EnumMember(Value = "developer")]
        Developer = 2,

        [EnumMember(Value = "viewer")]
        Viewer = 3
    }
}
