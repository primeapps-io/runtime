using System.Runtime.Serialization;

namespace PrimeApps.Model.Enums
{
    public enum FunctionRuntime
    {
        [EnumMember(Value = "")]
        NotSet = 0,

        [EnumMember(Value = "python2.7")]
        Python27 = 1,

        [EnumMember(Value = "python3.4")]
        Python34 = 2,

        [EnumMember(Value = "python3.6")]
        Python36 = 3,

        [EnumMember(Value = "nodejs6")]
        Nodejs6 = 4,

        [EnumMember(Value = "nodejs8")]
        Nodejs8 = 5,

        [EnumMember(Value = "ruby2.4")]
        Ruby24 = 6,

        [EnumMember(Value = "php7.2")]
        Php72 = 7,

        [EnumMember(Value = "go1.10")]
        Go110 = 8,

        [EnumMember(Value = "dotnetcore2.0")]
        Dotnetcore20 = 9,

        [EnumMember(Value = "java1.8")]
        Java18 = 10
    }
}
