using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum AggregationType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "count")]
        Count = 1,

        [EnumMember(Value = "sum")]
        Sum = 2,

        [EnumMember(Value = "avg")]
        Avg = 3,

        [EnumMember(Value = "min")]
        Min = 4,

        [EnumMember(Value = "max")]
        Max = 5
    }
}
