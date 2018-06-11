using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.App.Models
{
    public class SendActivationBindingModels
    {
		[Required]
		public Guid UserId { get; set; }
		[Required]
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		[Required]
		public int AppId { get; set; }
		public bool IsFreeLicense { get; set; }
		public string Culture { get; set; }
		[Required]
		public string Token { get; set; }

	}
}
