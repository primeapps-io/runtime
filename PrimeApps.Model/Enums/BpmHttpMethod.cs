using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Enums
{
    public enum BpmHttpMethod
    {
        [EnumMember(Value = "post")]
        Post = 1,

        [EnumMember(Value = "get")]
        Get = 2
    }
}
