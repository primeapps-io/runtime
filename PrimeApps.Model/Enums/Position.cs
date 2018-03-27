using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Enums
{
    public enum Position
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "left")]
        Left = 1,

        [EnumMember(Value = "right")]
        Right = 2,
    }
}
