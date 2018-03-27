using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum ReportType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "summary")]
        Summary = 1,

        [EnumMember(Value = "tabular")]
        Tabular = 2,

        [EnumMember(Value = "single")]
        Single = 3
    }
}
