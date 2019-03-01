using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Common.App
{
    public class AppDraftUserModel
    {
        [JsonProperty("email"), DataMember(Name = "email"), Required]
        public string Email { get; set; }

        [JsonProperty("first_name"), DataMember(Name = "first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name"), DataMember(Name = "last_name")]
        public string LastName { get; set; }

        [JsonProperty("full_name"), DataMember(Name = "full_name")]
        public string FullName { get; set; }

        [JsonProperty("password"), DataMember(Name = "password")]
        public string Password { get; set; }

        [JsonProperty("profile_id"), DataMember(Name = "profile_id")]
        public int ProfileId { get; set; }

        [JsonProperty("role_id"), DataMember(Name = "role_id")]
        public int RoleId { get; set; }

        [JsonProperty("is_active"), DataMember(Name = "is_active")]
        public bool IsActive { get; set; }

        [JsonProperty("created_at"), DataMember(Name = "created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
