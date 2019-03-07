using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Common.Role;

namespace PrimeApps.Model.Common.User
{
    public class StudioUser
    {
        /// <summary>
        /// User ID
        /// </summary>
        [JsonProperty("id"), DataMember(Name = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [JsonProperty("email"), DataMember(Name = "email")]
        public string Email { get; set; }

        /// <summary>
        /// First Name
        /// </summary>
        [JsonProperty("first_name"), DataMember(Name = "first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last Name
        /// </summary>
        [JsonProperty("last_name"), DataMember(Name = "last_name")]
        public string LastName { get; set; }

        /// <summary>
        ///Full name of user
        /// </summary>
        [JsonProperty("full_name"), DataMember(Name = "full_name")]
        public string FullName { get; set; }

        /// <summary>
        /// Phone Number
        /// </summary>
        [JsonProperty("phone"), DataMember(Name = "phone")]
        public string Phone { get; set; }


        /// <summary>
        /// Avatar ID from file storage
        /// </summary>
        [JsonProperty("picture"), DataMember(Name = "picture")]
        public string Picture { get; set; }
    }
}
