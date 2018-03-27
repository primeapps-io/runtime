using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum RelationType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "one_to_many")]
        OneToMany = 1,

        [EnumMember(Value = "many_to_many")]
        ManyToMany = 2
    }
}
