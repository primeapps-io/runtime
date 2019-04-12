using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace PrimeApps.Auth.UI
{
    public class AddOrganizationUserBindingModel
    {
        [DataMember(Name = "app_name"), Required]
        public string AppName { get; set; }
        
        [DataMember(Name = "email"), Required]
        public string Email { get; set; }

        [DataMember(Name = "first_name")]
        public string FirstName { get; set; }

        [DataMember(Name = "last_name")]
        public string LastName { get; set; }

        [JsonProperty("password"), DataMember(Name = "password")]
        public string Password { get; set; }
    }
}
