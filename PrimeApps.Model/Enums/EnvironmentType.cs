using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Enums
{
    public enum EnvironmentType
    {
        [EnumMember(Value = "development")]
        Development = 1,
        [EnumMember(Value = "test")]
        Test = 2,
        [EnumMember(Value = "product")]
        Product = 3
    }
}
