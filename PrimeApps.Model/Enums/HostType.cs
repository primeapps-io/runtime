using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Enums
{
    public enum HostType
    {
        [EnumMember(Value = "primeapps_cloud")]
        PrimeappsCloud = 1,
        [EnumMember(Value = "private_cloud")]
        PrivateCloud = 2,
        [EnumMember(Value = "on_premise")]
        OnPremise = 3
    }
}