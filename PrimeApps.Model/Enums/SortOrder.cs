using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum SortOrder
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "alphabetical")]
        Alphabetical = 1,

        [EnumMember(Value = "order")]
        Order = 2
    }
}
