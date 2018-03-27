using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Enums
{
    public enum SettingType
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "custom")]
        Custom = 1,

        [EnumMember(Value = "email")]
        Email = 2,

        [EnumMember(Value = "sms")]
        SMS = 3,

        [EnumMember(Value = "phone")]
        Phone = 4,

        [EnumMember(Value = "module")]
        Module = 5,

        [EnumMember(Value = "template")]
        Template = 6,

        [EnumMember(Value = "outlook")]
        Outlook = 7
    }
}
