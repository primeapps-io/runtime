using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum ComponentType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "script")]
        Script = 1,

        [EnumMember(Value = "component")]
        Component = 2
    }
}
