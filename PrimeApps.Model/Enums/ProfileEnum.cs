using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Enums
{
    public enum ProfileEnum
    {
        [EnumMember(Value = "manager")]
        Manager = 1,

        [EnumMember(Value = "developer")]
        Developer = 2,

        [EnumMember(Value = "viewer")]
        Viewer = 3,

        [EnumMember(Value = "tenant_admin")]
        TenantAdmin = 4
    }
}
