using Newtonsoft.Json;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Common.Organization
{
    public class OrganizationUserModel
    {
        [JsonProperty("id"), DataMember(Name = "id")]
        public int Id { get; set; }

        [JsonProperty("organization_id"), DataMember(Name = "organization_id"), Required]
        public int OrganizationId { get; set; }

        [JsonProperty("role"), DataMember(Name = "role"), Required]
        public OrganizationRole Role { get; set; }

        [JsonProperty("email"), DataMember(Name = "email"), Required]
        public string Email { get; set; }

        [JsonProperty("password"), DataMember(Name = "password")]
        public string Password { get; set; }

        [JsonProperty("first_name"), DataMember(Name = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name"), DataMember(Name = "last_name")]
        public string LastName { get; set; }

        [JsonProperty("full_name"), DataMember(Name = "full_name")]
        public string FullName { get; set; }

        [JsonProperty("created_at"), DataMember(Name = "created_at")]
        public DateTime CreatedAt { get; set; }

    }
}
