using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum AppDraftStatus
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "draft")]
        Draft = 1,

        [EnumMember(Value = "published")]
        Published = 2,

        [EnumMember(Value = "unpublished")]
        Unpublished = 3
    }
}
