using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Enums
{
    public enum RegistrationType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "studio")]
        Studio = 1,

        [EnumMember(Value = "tenant")]
        Tenant = 2,

        [EnumMember(Value = "external")]
        External = 3
    }
}
