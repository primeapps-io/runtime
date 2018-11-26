using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum OrganizationRole
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "administrator")]
        Administrator = 1,

        [EnumMember(Value = "collaborator")]
        Collaborator = 2
    }
}
