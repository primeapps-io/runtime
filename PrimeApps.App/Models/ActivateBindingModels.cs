using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace PrimeApps.App.Models
{
	public class CreateBindingModels
	{
		[DataMember(Name = "email"), Required, JsonProperty("email")]
		public string Email { get; set; }

		[DataMember(Name = "app_id"), JsonProperty("app_id")]
		public int? AppId { get; set; }

        [DataMember(Name = "app_name"), JsonProperty("app_name")]
        public string AppName { get; set; }

        [DataMember(Name = "culture"), JsonProperty("culture")]
		public string Culture { get; set; }

		[DataMember(Name = "first_name"), JsonProperty("first_name")]
		public string FirstName { get; set; }

		[DataMember(Name = "last_name"), JsonProperty("last_name")]
		public string LastName { get; set; }

		[DataMember(Name = "token"), JsonProperty("token")]
		public string Token { get; set; }
	}
}
