using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum Operator
    {
        NotSet = 0,

        [EnumMember(Value = "is")]
        Is = 1,

        [EnumMember(Value = "is_not")]
        IsNot = 2,

        [EnumMember(Value = "equals")]
        Equals = 3,

        [EnumMember(Value = "not_equal")]
        NotEqual = 4,

        [EnumMember(Value = "contains")]
        Contains = 5,

        [EnumMember(Value = "not_contain")]
        NotContain = 6,

        [EnumMember(Value = "starts_with")]
        StartsWith = 7,

        [EnumMember(Value = "ends_with")]
        EndsWith = 8,

        [EnumMember(Value = "empty")]
        Empty = 9,

        [EnumMember(Value = "not_empty")]
        NotEmpty = 10,

        [EnumMember(Value = "greater")]
        Greater = 11,

        [EnumMember(Value = "greater_equal")]
        GreaterEqual = 12,

        [EnumMember(Value = "less")]
        Less = 13,

        [EnumMember(Value = "less_equal")]
        LessEqual = 14,

        [EnumMember(Value = "not_in")]
        NotIn = 15

    }
}
