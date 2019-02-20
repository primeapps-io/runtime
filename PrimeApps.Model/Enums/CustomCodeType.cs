using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Enums
{
    public enum CustomCodeType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "functions")]
        Functions = 1,

        [EnumMember(Value = "components")]
        Components = 2,

        [EnumMember(Value = "scripts")]
        Scripts = 3
    }
}
