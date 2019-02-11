using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Enums
{
    public enum DeploymentStatus
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "running")]
        Running = 1,

        [EnumMember(Value = "failed")]
        Failed = 2,

        [EnumMember(Value = "succeed")]
        Succeed = 3
    }
}
