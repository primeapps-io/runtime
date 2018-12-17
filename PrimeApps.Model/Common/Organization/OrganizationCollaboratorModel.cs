using Newtonsoft.Json;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Common.Organization
{
    public class OrganizationCollaboratorModel
    {
        [JsonProperty("organization_id"), DataMember(Name = "organization_id"), Required]
        public int OrganizationId { get; set; }

        [JsonProperty("page"), DataMember(Name = "page")]
        public int Page { get; set; }

        [JsonProperty("order_by"), DataMember(Name = "order_by")]
        public string OrderBy { get; set; }

        [JsonProperty("order_field"), DataMember(Name = "order_field")]
        public string OrderField { get; set; }

    }
}
