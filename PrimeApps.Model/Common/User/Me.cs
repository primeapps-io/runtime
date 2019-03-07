using PrimeApps.Model.Common.Organization;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.User
{
    public class Me
    {
        [DataMember]
        public StudioUser user { get; set; }
        
        [DataMember]
        public  List<OrganizationModel> organizations { get; set; }
    }
}
