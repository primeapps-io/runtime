using PrimeApps.Model.Entities.Console;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Common.Organization
{
    public class Organization
    {
        [DataMember]
        public int id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string icon { get; set; }

        [DataMember]
        public int ownerId { get; set; }

        [DataMember]
        public ICollection<Team> teams { get; set; }

        [DataMember]
        public ICollection<OrganizationUser> users { get; set; }

        [DataMember]
        public ICollection<AppDraft> apps { get; set; }
    }
}
