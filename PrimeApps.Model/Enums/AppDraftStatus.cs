using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum AppDraftStatus
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "draft")]
        Draft = 1,

        [EnumMember(Value = "publish")]
        Publish = 2,

        [EnumMember(Value = "Unpublish")]
        Unpublish = 3
    }
}
