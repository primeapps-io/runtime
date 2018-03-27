using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Enums
{
    public enum WorkflowHttpMethod
    {

        [EnumMember(Value = "post")]
        Post = 1,

        [EnumMember(Value = "get")]
        Get = 2
    }
}
