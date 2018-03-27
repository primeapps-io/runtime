using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Enums
{
    public enum AddressType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "country")]
        Country = 1,

        [EnumMember(Value = "city")]
        City = 2,

        [EnumMember(Value = "disrict")]
        Disrict = 3,

        [EnumMember(Value = "street")]
        Street = 4
    }
}
