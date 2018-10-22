using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Auth.UI
{
    public class CreateAccountBindingModel
    {
        [Required, JsonProperty("email")]
        public string Email { get; set; }
        
        [JsonProperty("send_activation")]
        public bool SendActivation { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Required, JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("culture")]
        public string Culture { get; set; }

        [JsonProperty("app_name")]
        public string AppName { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }
    }
}
