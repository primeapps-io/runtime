using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum ChartTheme
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "zune")]
        Zune = 1,

        [EnumMember(Value = "ocean")]
        Ocean = 2,

        [EnumMember(Value = "carbon")]
        Carbon = 3
    }
}
