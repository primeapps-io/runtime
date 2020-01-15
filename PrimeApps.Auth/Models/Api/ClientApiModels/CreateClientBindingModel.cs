using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using IdentityServer4.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PrimeApps.Auth.Models.Api.ClientApiModels
{
    public class CreateClientBindingModel
    {
        [Required, JsonProperty("client_id"), DataMember(Name = "client_id")]
        public string ClientId { get; set; }
        
        [JsonProperty("client_name"), DataMember(Name = "client_name")]
        public string ClientName { get; set; }

        [JsonProperty("allowed_grant_types"), DataMember(Name = "allowed_grant_types")]
        public string AllowedGrantTypes { get; set; }

        [JsonProperty("allow_remember_consent"), DataMember(Name = "allow_remember_consent")]
        public bool AllowRememberConsent { get; set; }

        [JsonProperty("always_send_client_claims"), DataMember(Name = "always_send_client_claims")]
        public bool AlwaysSendClientClaims { get; set; }

        [JsonProperty("require_consent"), DataMember(Name = "require_consent")]
        public bool RequireConsent { get; set; }

        [JsonProperty("access_token_life_time"), DataMember(Name = "access_token_life_time")]
        public int AccessTokenLifetime { get; set; }

        [JsonProperty("redirect_uris"), DataMember(Name = "redirect_uris")]
        public string RedirectUris { get; set; }

        [JsonProperty("post_logout_redirect_uris"), DataMember(Name = "post_logout_redirect_uris")]
        public string PostLogoutRedirectUris { get; set; }

        [JsonProperty("allowed_scopes"), DataMember(Name = "allowed_scopes")]
        public string AllowedScopes { get; set; }

        [JsonProperty("client_secrets"), DataMember(Name = "client_secrets")]
        public string ClientSecrets { get; set; }

    }
}