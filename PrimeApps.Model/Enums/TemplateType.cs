using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum TemplateType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "quote")]
        Quote = 1,

        [EnumMember(Value = "email")]
        Email = 2,

        [EnumMember(Value = "sms")]
        Sms = 2,

        [EnumMember(Value = "module")]
        Module = 3,

        [EnumMember(Value = "excel")]
        Excel = 4
    }
}
