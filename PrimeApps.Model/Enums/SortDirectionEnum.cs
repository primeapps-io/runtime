using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum SortDirection
    {
        NotSet = 0,

        [EnumMember(Value = "asc")]
        Asc = 1,

        [EnumMember(Value = "desc")]
        Desc = 2,
    }
}
