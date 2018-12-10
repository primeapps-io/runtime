using PrimeApps.Model.Common.Organization;
using PrimeApps.Model.Entities.Console;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Common.User
{
    public class Me
    {
        [DataMember]
        public ConsoleUser user { get; set; }
        
        [DataMember]
        public  List<Organization.Organization> organizations { get; set; }
    }
}
