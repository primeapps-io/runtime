using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Auth.UI
{
    public class ForgotPasswordViewModel
    {
		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}
}
