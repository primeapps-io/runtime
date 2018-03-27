using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum ModuleType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "module_list")]
        ModuleList = 1,

        [EnumMember(Value = "module_detail")]
        ModuleDetail = 2,

        [EnumMember(Value = "module_form")]
        ModuleForm = 3,

    }
}
