using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Auth.Models.AccountViewModels
{
	public class LoginInputModel
	{
		[Required]
		public string Username { get; set; }
		[Required]
		public string Password { get; set; }
		public bool RememberLogin { get; set; }
		public string ReturnUrl { get; set; }
	}
}
