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
        [JsonProperty("id"), DataMember(Name = "id")]
        public int Id { get; set; }
        
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

        [JsonProperty("created_at"), DataMember(Name = "created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
