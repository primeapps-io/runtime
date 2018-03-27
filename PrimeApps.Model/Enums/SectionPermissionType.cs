using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Enums
{
    public enum SectionPermissionType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "full")]
        Full = 1,

        [EnumMember(Value = "read_only")]
        ReadOnly = 2,

        [EnumMember(Value = "none")]
        None = 3
    }
}
