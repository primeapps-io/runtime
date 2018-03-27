using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum LogicType
    {
        NotSet = 0,

        [EnumMember(Value = "and")]
        And = 1,

        [EnumMember(Value = "or")]
        Or = 2
    }
}
