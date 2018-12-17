using Newtonsoft.Json;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Common.Organization
{
    public class OrganizationAppModel
    {
        [JsonProperty("organization_id"), DataMember(Name = "organization_id"), Required]
        public int OrganizationId { get; set; }

    }
}
