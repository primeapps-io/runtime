using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum LanguageType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "en")]
        En = 1,

        [EnumMember(Value = "tr")]
        Tr = 2
    }
}
