using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Enums
{
    public enum ProcessStatus
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "waiting")]
        Waiting = 1,

        [EnumMember(Value = "approved")]
        Approved = 2,

        [EnumMember(Value = "rejected")]
        Rejected = 3
    }
}
