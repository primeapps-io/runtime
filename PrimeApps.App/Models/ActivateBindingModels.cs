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
		[DataMember(Name = "email"), Required]
		public string Email { get; set; }

		[DataMember(Name = "app_id"), Required]
		public int AppId { get; set; }

		[DataMember(Name = "culture")]
		public string Culture { get; set; }

		[DataMember(Name = "first_name")]
		public string FirstName { get; set; }

		[DataMember(Name = "email_confirmed")]
		public bool EmailConfirmed { get; set; }

		[DataMember(Name = "last_name")]
		public string LastName { get; set; }

		[DataMember(Name = "token")]
		public string Token { get; set; }
	}
}
