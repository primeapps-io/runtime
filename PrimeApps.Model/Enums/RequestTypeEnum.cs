using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Enums
{
    public enum RequestTypeEnum
    {
        [EnumMember(Value = "view")]
        View = 1,

        [EnumMember(Value = "create")]
        Create = 2,

        [EnumMember(Value = "update")]
        Update = 3,

        [EnumMember(Value = "delete")]
        Delete = 4
    }
}