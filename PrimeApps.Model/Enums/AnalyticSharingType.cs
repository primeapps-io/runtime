using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum AnalyticSharingType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "everybody")]
        Everybody = 1,
        
        [EnumMember(Value = "custom")]
        Custom = 2
    }
}
