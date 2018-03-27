using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum ShowType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "publish")]
        Publish = 1,

        [EnumMember(Value = "draft")]
        Draft = 2,
       
    }
}
