using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum FunctionContentType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "url")]
        Url = 1,

        [EnumMember(Value = "text")]
        Text = 2,
    }
}
