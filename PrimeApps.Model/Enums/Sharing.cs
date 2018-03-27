using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum Sharing
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "private")]
        Private = 1,

        [EnumMember(Value = "public")]
        Public = 2
    }
}
