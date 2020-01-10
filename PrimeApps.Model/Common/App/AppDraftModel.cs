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

        [JsonProperty("color"), DataMember(Name = "color")]
        public string Color { get; set; }

        [JsonProperty("icon"), DataMember(Name = "icon")]
        public string Icon { get; set; }

        [JsonProperty("app_domain"), DataMember(Name = "app_domain")]
        public string AppDomain { get; set; }

        [JsonProperty("auth_domain"), DataMember(Name = "auth_domain")]
        public string AuthDomain { get; set; }

        [JsonProperty("templet_id"), DataMember(Name = "templet_id")]
        public int TempletId { get; set; }

        /*[JsonProperty("organization_id"), DataMember(Name = "organization_id")]
        public int OrganizationId { get; set; }*/

        [JsonProperty("use_tenant_settings"), DataMember(Name = "use_tenant_settings")]
        public bool UseTenantSettings { get; set; }

        [JsonProperty("clear_all_records"), DataMember(Name = "clear_all_records")]
        public bool ClearAllRecords { get; set; }

        [JsonProperty("enable_registration"), DataMember(Name = "enable_registration")]
        public bool EnableRegistration { get; set; }

        [JsonProperty("enable_api_registration"), DataMember(Name = "enable_api_registration")]
        public bool EnableAPIRegistration { get; set; }

        [JsonProperty("enable_ldap"), DataMember(Name = "enable_ldap")]
        public bool EnableLDAP { get; set; }
    }
}