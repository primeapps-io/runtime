using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Enums
{
    public enum FieldViewType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "dropdown")]
        Dropdown = 1,

        [EnumMember(Value = "radio")]
        Radio = 2,

        [EnumMember(Value = "checkbox")]
        Checkbox = 2,
    }
}
