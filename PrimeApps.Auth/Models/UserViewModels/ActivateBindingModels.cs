using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Auth.Models.UserViewModels
{
	public class ActivateBindingModels
	{
		[Required]
		public string email { get; set; }

		[Required]
		public int app_id { get; set; }
		public string culture { get; set; }
		public string first_name { get; set; }
		public bool email_confirmed { get; set; }
		public string last_name { get; set; }
		public string token { get; set; }
	}
}
