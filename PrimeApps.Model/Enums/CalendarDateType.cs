using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum CalendarDateType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "start_date")]
        StartDate = 1,

        [EnumMember(Value = "end_date")]
        EndDate = 2
    }
}
