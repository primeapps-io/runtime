using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum DashletArea
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "dashboard")]
        Dashboard = 1
    }
}
