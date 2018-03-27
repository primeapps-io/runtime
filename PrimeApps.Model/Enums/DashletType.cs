using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum DashletType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "chart")]
        Chart = 1,

        [EnumMember(Value = "widget")]
        Widget = 2
    }
}
