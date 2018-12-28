using Newtonsoft.Json;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Common.App
{
    public class AppDraftModel
    {
        [JsonProperty("id"), DataMember(Name = "id")]
        public int Id { get; set; }

        [JsonProperty("name"), DataMember(Name = "name"), Required]
        public string Name { get; set; }

        [JsonProperty("label"), DataMember(Name = "label")]
        public string Label { get; set; }

        [JsonProperty("description"), DataMember(Name = "description")]
        public string Description { get; set; }

        [JsonProperty("logo"), DataMember(Name = "logo")]
        public string Logo { get; set; }

        [JsonProperty("templet_id"), DataMember(Name = "templet_id")]
        public int TempletId { get; set; }

        [JsonProperty("organization_id"), DataMember(Name = "organization_id")]
        public int OrganizationId { get; set; }

        [JsonProperty("status"), DataMember(Name = "status")]
        public AppDraftStatus Status { get; set; }

        [JsonProperty("use_tenant_settings"), DataMember(Name = "use_tenant_settings")]
        public bool UseTenantSettings { get; set; }
    }
}
