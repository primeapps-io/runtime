using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Auth.Models.UserViewModels
{
    public class TenantUserViewModel
    {
		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}
