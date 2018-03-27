using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum DocumentFilterQueryOperator
    {
        NotSet = 0,

        [EnumMember(Value = "starts_with")]
        StartsWith = 1,

        [EnumMember(Value = "equals")]
        Equals = 2
    }
    public enum DocumentFilterOperator
    {
        NotSet = 0,

        [EnumMember(Value = "and")]
        And = 1,

        [EnumMember(Value = "or")]
        Or = 2
    }
}
