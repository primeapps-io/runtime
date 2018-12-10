using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Common.Organization
{
    public class OrganizationUser
    {
        [DataMember]
        public int id { get; set; }

        [DataMember]
        public OrganizationRole role { get; set; }

        [DataMember]
        public string email { get; set; }

        [DataMember]
        public string firstName { get; set; }

        [DataMember]
        public string lastName { get; set; }

        [DataMember]
        public DateTime createdAt { get; set; }

    }
}
