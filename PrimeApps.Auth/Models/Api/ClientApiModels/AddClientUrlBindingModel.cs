using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using IdentityServer4.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PrimeApps.Auth.Models.Api.ClientApiModels
{
    public class AddClientUrlBindingModel
    {
        [JsonProperty("client_id"), DataMember(Name = "client_id")]
        public string ClientId { get; set; }

        [Required, JsonProperty("urls"), DataMember(Name = "urls")]
        public string Urls { get; set; }
    }
}