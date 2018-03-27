using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Enums
{
    public enum DetailViewType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "tab")]
        Tab = 1,

        [EnumMember(Value = "flat")]
        Flat = 2,
    }
}
