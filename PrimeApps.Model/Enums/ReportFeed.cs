using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum ReportFeed
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "module")]
        Module = 1, //Defined modules data (records data)

        [EnumMember(Value = "custom")]
        Custom = 2 //Custom SQL
    }
}
