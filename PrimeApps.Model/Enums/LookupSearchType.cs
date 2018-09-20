using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum LookupSearchType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "starts_with")]
        StartsWith = 1,

        [EnumMember(Value = "contains")]
        Contains = 2
    }
}
